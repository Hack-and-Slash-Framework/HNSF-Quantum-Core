using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Deterministic;
using Photon.Realtime;
using Quantum;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace HnSF.sessionhandling.handlers
{
    // TODO : Handle starting errors (probably via events).
    public class SessionHandlerLocalMatch : SessionHandlerBase
    {
        private string matchSessionHandlerIdentifier = "ReplaceThis";
        
        public AppSettings quantumAppSettings;
        public RuntimeConfig defaultRuntimeConfig;
        private string clientSecret = "aaa";
        
        public UnityEvent<string> OnMatchEnded = new();
        
        public int expectedPlayerCount;
        
        // Assets
        public LoadedAssetHandleWrapper gamemodeAssetHandle;
        public string gamemodeConfiguration;
        public LoadedAssetHandleWrapper mapAssetHandle;
        public List<PlayerMatchContentBundle> localPlayerContentBundles = new List<PlayerMatchContentBundle>();
        public SystemsConfig builtSystemConfig;
        public GamemodeSettingsBase settingsAssetInstance;
        
        // ...
        public SessionHandlerMatch matchSessionHandlerPrefab;
        public SessionHandlerMatch matchSessionHandler;
        public MatchHandlerBase gamemodeMatchHandlerInstance;
        
        public override bool Initialize()
        {
            defaultRuntimeConfig.SystemsConfig = QuantumDefaultConfigs.Global.SystemsConfig;
            defaultRuntimeConfig.SimulationConfig = QuantumDefaultConfigs.Global.SimulationConfig;
            clientSecret = $"a{Random.Range(0, 10000)}_{Random.Range(0, 10000)}_{Time.realtimeSinceStartup}";
            PhotonServerSettings.Global.AppSettings.CopyTo(quantumAppSettings);
            return base.Initialize();
        }

        public override void Teardown()
        {
            if (TornDown)
                return;
            
            if (HnSFManagersContainer.instance != null)
            {
                if (builtSystemConfig)
                {
                    QuantumUnityDB.Global.RemoveSource(builtSystemConfig.Guid);
                    Destroy(builtSystemConfig);
                }

                if (settingsAssetInstance)
                {
                    QuantumUnityDB.Global.RemoveSource(settingsAssetInstance.Guid);
                    Destroy(settingsAssetInstance);
                }
                
                var sessionManager = HnSFManagersContainer.instance.sessionHandlerManager;

                if (matchSessionHandler != null)
                {
                    sessionManager.DestroySessionHandler(matchSessionHandlerIdentifier, teardown: true);
                    matchSessionHandler = null;
                }
            }
            
            if (gamemodeMatchHandlerInstance != null)
            {
                gamemodeMatchHandlerInstance.Teardown();
            }
            
            base.Teardown();
        }
        
        public async UniTask<bool> PrepareForMatchAndStart(QuantumMatchContentBundle matchContentBundle)
        {
            localPlayerContentBundles = matchContentBundle.localPlayerBundles;
            Debug.Log("Preparing for match.");
            var contentManager = HnSFManagersContainer.instance.contentManager;
            
            this.gamemodeAssetHandle = await contentManager.LoadAssetFromModAsync(matchContentBundle.gamemodeReference);
            this.mapAssetHandle = await contentManager.LoadAssetFromModAsync(matchContentBundle.mapReference);

            expectedPlayerCount = matchContentBundle.playerCount;
            
            var gamemodeSetupResult = await InitializeMatchHandler();
            if (gamemodeSetupResult == false)
            {
                Debug.Log("Setup failed.");
                return false;
            }

            await TransitionToQuantumGameSession();
            return true;
        }
        
        protected virtual void WhenQuitMatch(int arg0)
        {
            //ForceQuit();
            OnMatchEnded.Invoke(null);
        }
        
        protected async UniTask TransitionToQuantumGameSession()
        {
            var sessionManager = HnSFManagersContainer.instance.sessionHandlerManager;

            matchSessionHandler = sessionManager.CreateSessionHandler(matchSessionHandlerIdentifier, matchSessionHandlerPrefab);
            if (matchSessionHandler == null)
            {
                Debug.LogError("Could not create match session handler.");
                return;
            }
            matchSessionHandler.OnQuitMatch.AddListener(WhenQuitMatch);
            
            // GAMEMODE SYSTEMS
            BuildGamemodeSystemsConfig();

            // GAMEMODE CONFIG
            BuildGamemodeSettingsAsset();

            // MATCH HANDLER SETUP
            bool gamemodeSetupResult = await gamemodeMatchHandlerInstance.Setup(defaultRuntimeConfig);
            
            // MAP
            await LoadMap();

            await UniTask.NextFrame();

            await PreSessionCreation();
            
            int gameFlags = 0;

            var sessionRunnerArgs = new SessionRunner.Arguments
            {
                RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
                GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
                ClientId = clientSecret, // TODO: Actual client secret.
                RuntimeConfig = defaultRuntimeConfig,
                SessionConfig = QuantumDeterministicSessionConfigAsset.DefaultConfig,
                GameMode = DeterministicGameMode.Local,
                PlayerCount = expectedPlayerCount,
                StartGameTimeoutInSeconds = 10.0f,
                GameFlags = gameFlags,
                RecordingFlags = RecordingFlags.None,
#if UNITY_EDITOR
                DeltaTimeType = SimulationUpdateTime.EngineDeltaTime
#endif
            };

            gamemodeMatchHandlerInstance.SetLocalPlayerInfo(localPlayerContentBundles);
            
            matchSessionHandler.runner = (QuantumRunner)await SessionRunner.StartAsync(sessionRunnerArgs);
            matchSessionHandler.matchHandlerInstance = gamemodeMatchHandlerInstance;
            matchSessionHandler.selectedMapDefinition = mapAssetHandle;
            matchSessionHandler.InitMatch();
        }

        protected virtual UniTask PreSessionCreation()
        {
            defaultRuntimeConfig.Seed = Random.Range(int.MinValue, int.MaxValue);
            return UniTask.CompletedTask;
        }
        
        protected virtual async UniTask LoadMap()
        {
            await mapAssetHandle.GetAsset<IMapDefinition>().LoadAssets();
            defaultRuntimeConfig.Map.Id = mapAssetHandle.GetAsset<IMapDefinition>().GetMapAsset().Guid.Value;
            await mapAssetHandle.GetAsset<IMapDefinition>().LoadMap(LoadSceneMode.Single);
        }

        protected virtual void BuildGamemodeSettingsAsset()
        {
            var gamemodeDefinition = gamemodeAssetHandle.GetAsset<BaseGamemodeDefinition>();
            if (!string.IsNullOrEmpty(gamemodeConfiguration))
            {
                try
                {
                    settingsAssetInstance =
                        JsonUtilityExtensions.FromJsonWithTypeAnnotation(gamemodeConfiguration) as GamemodeSettingsBase;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception thrown while trying to load gamemode settings asset: {e}. Using default settings.");
                    settingsAssetInstance = gamemodeMatchHandlerInstance.defaultSettings.GetInstance();
                }
            }
            
            if(settingsAssetInstance == null)
            {
                settingsAssetInstance = gamemodeMatchHandlerInstance.defaultSettings.GetInstance();
            }
            
            // Register instance to quantum
            settingsAssetInstance.name = $"GamemodeSettingsAsset_RUNTIME_{gamemodeDefinition.Name}";
            settingsAssetInstance.Guid = QuantumUnityDB.CreateRuntimeDeterministicGuid(settingsAssetInstance);
            QuantumUnityDB.Global.AddAsset(settingsAssetInstance);
            
            defaultRuntimeConfig.gamemodeConfigAsset = settingsAssetInstance;
            
            // Set settings
            settingsAssetInstance.teamRules = gamemodeDefinition.GetTeamRules();
            settingsAssetInstance.teamConfigs = gamemodeDefinition.GetDefaultTeamConfig();
            settingsAssetInstance.Initialize();
        }

        protected virtual void BuildGamemodeSystemsConfig()
        {
            if (QuantumUnityDB.TryGetGlobalAsset(gamemodeMatchHandlerInstance.gamemodeSystemConfig,
                    out var gamemodeSystemConfigOverrider))
            {
                builtSystemConfig = gamemodeSystemConfigOverrider.BuildSystemsConfig();
                builtSystemConfig.name = $"SYSTEMCONFIG_{gamemodeMatchHandlerInstance.name}_{gamemodeSystemConfigOverrider.name}";
                builtSystemConfig.Guid = QuantumUnityDB.CreateRuntimeDeterministicGuid(builtSystemConfig);
                QuantumUnityDB.Global.AddAsset(builtSystemConfig);
                defaultRuntimeConfig.SystemsConfig = builtSystemConfig;
            }
            else
            {
                Debug.LogError("No system config provided in match handler.");
            }
        }

        protected async UniTask<bool> InitializeMatchHandler()
        {
            var gamemodeDefinition = gamemodeAssetHandle.GetAsset<BaseGamemodeDefinition>();
            if (gamemodeDefinition == null) return false;
            if ((await gamemodeDefinition.LoadAssets()) == false)
            {
                gamemodeDefinition.UnloadAssets();
                return false;
            }

            gamemodeMatchHandlerInstance = GameObject.Instantiate(gamemodeDefinition.GetMatchHandler().GetComponent<MatchHandlerBase>(), transform,
                false);
            return true;
        }
    }
}