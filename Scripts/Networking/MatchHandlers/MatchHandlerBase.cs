using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.Events;

namespace HnSF
{
    public class MatchHandlerBase : MonoBehaviour, ILocalEntityHolder
    {
        public UnityEvent<int> OnQuitMatch = new();

        public QuantumRunner gameRunner;
        public virtual Dictionary<int, QuantumEntityView> LocalPlayerEntitys => null;
        
        public AssetRef<SystemsConfigOverrider> gamemodeSystemConfig;

        public List<PlayerMatchContentBundle> localPlayerInfo = new();
        
        public GamemodeSettingsBase defaultSettings;

        [NonSerialized] public GenericGamemodeStates lastKnownGamemodeState = GenericGamemodeStates.Off;
        
        public Dictionary<int, PlayerRef> localPlayerToGlobalPlayer = new();
        public Dictionary<PlayerRef, int> GlobalPlayerToLocalPlayer = new();

        [NonSerialized] public QuantumEntityViewUpdater entityViewUpdater = null;
        
        public BaseSongDefinition bgmDefinition = null;
        
        protected virtual void Awake()
        {
            lastKnownGamemodeState = GenericGamemodeStates.Off;
            FindEntityViewUpdater();
        }
        
        public virtual void FindEntityViewUpdater()
        {
            if (entityViewUpdater != null) return;
            entityViewUpdater = FindAnyObjectByType<QuantumEntityViewUpdater>(FindObjectsInactive.Exclude);
        }

        public virtual void SetLocalPlayerInfo(List<PlayerMatchContentBundle> playerContent)
        {
            this.localPlayerInfo = playerContent;
        }

        public virtual void SetBGM(BaseSongDefinition songDefinition)
        {
            
        }

        protected virtual void OnGameStart(CallbackGameStarted callback)
        {
            Debug.Log("Game started.");
        } 
        
        protected virtual void OnGameDestroyed(CallbackGameDestroyed callback)
        {
            Teardown();
        }
        
        public virtual UniTask<bool> Setup(RuntimeConfig runtimeConfig)
        {
            return new UniTask<bool>(true);
        }
        
        public virtual void ShutdownRunner()
        {
            gameRunner.Destroy();
        }
        
        public virtual void OnDestroy()
        {
            Teardown();
        }
        
        public virtual void Teardown()
        {
            foreach (var lpb in localPlayerInfo)
            {
                lpb.Teardown();
            }
        }
        
        public virtual void PollInput(CallbackPollInput callback)
        {
            var inputManager = HnSFManagersContainer.instance.inputManager;
            var inputPlayer = inputManager.GetPlayer(callback.PlayerSlot + 1);
            if (inputPlayer == null)
            {
                Debug.LogError($"Invalid input player. {callback.PlayerSlot+1} vs input player count of {inputManager.playerInputManagers.Count-1}.");
                return;
            }

            Quantum.Input input = new Quantum.Input();
            
            
            
            callback.SetInput(input, DeterministicInputFlags.Repeatable);
        }
    }
}