using System;
using HnSF.core.state;
using Photon.Deterministic;
using Quantum;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace HnSF
{
    public partial class StatePreviewEditorWindow : EditorWindow
    {
        [SerializeField] public UnityEvent OnWindowClosed = new UnityEvent();
        
        public PreviewRenderUtility renderUtils;
        protected int frame = 0;
        protected bool rotationMode = false;
        protected bool moveMode = false;
        protected Vector2 mousePos = new Vector2(0, 0);
        protected Vector2 diff = Vector2.zero;
        protected float rotSpeed = 1;
        protected float scrollWheel = 0;
        protected float scrollSpeed = 0.5f;
        protected float moveSpeed = 0.5f;
        private bool autoPlay = false;

        public GameObject rootGameObject;
        
        protected GameObject attackerSceneReference;
        protected StatePreviewEntityViewUpdater evu;
        public QuantumRunner runner;
        
        // Config
        public StatePreviewConfiguration previewConfig;
        public HNSFStateSet stateSetAsset;
        public HNSFState stateAsset;
        public BattleActorDefinition battleActorDefinition;
        public EntityRef attackerEntityRef;
        public EntityRef defenderEntityRef;
        
        unsafe partial void InitializeSimulation();
        unsafe partial void SetStateForPlaybackUser(HNSFState state);
        unsafe partial void PreQuantumInitUser();
        unsafe partial void TeardownUser();
        unsafe partial void HandleControlsUser(Rect pos);
        
        [MenuItem("Tools/State Preview Window")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<StatePreviewEditorWindow>();
            wnd.titleContent = new GUIContent("State Preview");
        }

        protected virtual void OnEnable()
        {
            //playInterval = Time.fixedDeltaTime;
            //nextPlayTime = EditorApplication.timeSinceStartup+playInterval;
            if (renderUtils == null)
            {
                renderUtils = new PreviewRenderUtility(true);
            }

            //renderUtils.camera.cameraType = CameraType.Preview;
            renderUtils.camera.cameraType = CameraType.SceneView;
            renderUtils.camera.fieldOfView = 40;
            renderUtils.camera.transform.position = new Vector3(0, 1.5f, -15);
            renderUtils.camera.transform.LookAt(new Vector3(0, 1, 0));
            renderUtils.camera.farClipPlane = 100;
            
            rootGameObject = new GameObject("StatePreviewRoot");
            rootGameObject.hideFlags = HideFlags.HideAndDontSave;
            renderUtils.AddSingleGO(rootGameObject);
        }

        public virtual void Teardown()
        {
            CleanupQuantum();
            TeardownUser();
        }
        
        protected virtual void OnDisable()
        {
            CleanupQuantum();
            if (renderUtils != null)
            {
                renderUtils.Cleanup();
            }
            OnWindowClosed?.Invoke();
        }

        private void CleanupQuantum()
        {
            autoPlay = false;
            if (runner != null)
            {
                runner.Shutdown();
                runner = null;
            }
            
            var instance = FindFirstObjectByType<QuantumTaskRunnerJobs>();
            if (instance) {
                DestroyImmediate(instance.gameObject);
            }
            
            Debug.Log("Quantum Cleaned up.");
        }
        
        public bool Initialize(StatePreviewConfiguration configuration, HNSFStateSet stateSet, HNSFState sAsset)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("Cannot preview state in Play Mode.");
                return false;
            }

            if (configuration == null || stateSet == null || sAsset == null)
            {
                Debug.LogError("Select a state first.");
                return false;
            }

            if (stateSet.previewActor.IsValid == false)
            {
                Debug.LogError($"Set a preview actor on the State Set.");
                return false;
            }

            if (!QuantumUnityDB.TryGetGlobalAssetEditorInstance(stateSet.previewActor, out battleActorDefinition))
            {
                Debug.LogError("Could not get BattleActorDefinition asset.");
                return false;
            }
            
            this.stateSetAsset = stateSet;
            this.stateAsset = sAsset;
            
            this.previewConfig = configuration;
            evu = renderUtils.InstantiatePrefabInScene(configuration.evuPrefab.gameObject).GetComponent<StatePreviewEntityViewUpdater>();
            evu.RenderUtility = renderUtils;
            evu.rootObject = rootGameObject;

            PreQuantumInitUser();
            InitializeQuantum(configuration);
            evu.Awake();
            evu.SetCurrentGame(runner.Game, false);

            InitializeSimulation();
            TickSimulation();
            Debug.Log("Preview Initialized");
            return true;
        }
        
        private void InitializeQuantum(StatePreviewConfiguration configuration)
        {
            Debug.Log("Initializing Quantum");

            if (previewConfig.generatedConfig == null)
            {
                previewConfig.generatedConfig = configuration.systemsConfigOverrider.BuildSystemsConfig();
                previewConfig.generatedConfig.name = $"SYSTEMCONFIG_STATEPREVIEW_{configuration.systemsConfigOverrider.name}";
                //previewConfig.generatedConfig.Guid = QuantumUnityDB.CreateRuntimeDeterministicGuid(previewConfig.generatedConfig);
                
                string existingPath = AssetDatabase.GetAssetPath(previewConfig.systemsConfigOverrider);
                string directory = System.IO.Path.GetDirectoryName(existingPath);
                string newPath = directory + $"/{previewConfig.generatedConfig.name}.asset";
                
                AssetDatabase.CreateAsset(previewConfig.generatedConfig, newPath);
                EditorUtility.SetDirty(previewConfig.generatedConfig);
                AssetDatabase.SaveAssets();
                
                QuantumUnityDB.Global.AddAsset(previewConfig.generatedConfig);
            }
            
            configuration.runtimeConfig.SystemsConfig = previewConfig.generatedConfig;
            
            QuantumRunnerUnityFactory.Init();
            QuantumUnityDB.UpdateGlobal();
            
            configuration.quantumSettings.Initialize();
            
            int gameFlags = 0;
            var sessionRunnerArgs = new SessionRunner.Arguments
            {
                RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
                GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
                ClientId = "a", // TODO: Actual client secret.
                RuntimeConfig = configuration.runtimeConfig,
                SessionConfig = QuantumDeterministicSessionConfigAsset.DefaultConfig,
                GameMode = DeterministicGameMode.Local,
                PlayerCount = 1,
                StartGameTimeoutInSeconds = 0.0f,
                GameFlags = gameFlags,
                RecordingFlags = RecordingFlags.None,
                DeltaTimeType = SimulationUpdateTime.EngineDeltaTime,
            };
            Debug.Log("Starting Runner.");
            runner = (QuantumRunner)SessionRunner.Start(sessionRunnerArgs);
            runner.Service(1.0f / 60.0f);
            runner.Service(1.0f / 60.0f);
        }

        public void TickSimulation()
        {
            if (runner)
            {
                runner.Service(1.0f / 60.0f);
                evu.LateUpdate();
            }
        }
        
        private void TickView()
        {
            foreach (var cu in rootGameObject.GetComponentsInChildren<ParticleSystem>())
            {
                cu.Simulate(1.0f / 60.0f, true, false, false);
            }

            foreach (var cc in rootGameObject.GetComponentsInChildren<VisualEffectBase>())
            {
                cc.FixedUpdate();
            }
        }
        
        public void ToggleAutoPlay()
        {
            autoPlay = !autoPlay;
            EditorDeltaTime.Reset();
        }

        private double timer = 0;
        protected virtual void Update()
        {
            Repaint();

            if (autoPlay && runner)
            {
                EditorDeltaTime.SetEditorDeltaTime();

                timer += EditorDeltaTime.editorDeltaTime;
                if (timer >= 1.0 / 60.0)
                {
                    timer -= (1.0 / 60.0);
                    TickSimulation();
                    TickView();
                }
            }
        }

        public void SetStateForPlayback(HNSFState state)
        {
            stateAsset = state;
            SetStateForPlaybackUser(state);
        }
        
        private void OnGUI()
        {
            var pos = position;
            pos.x = 0;
            pos.y = 0;
            //pos.height /= 2.75f;
            //pos.height = Mathf.Min(250, pos.height);
            
            HandleControlsUser(pos);

            RenderingMain(pos);
        }

        private void HandleControlsDefault(Rect pos)
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.R && runner != null)
                    {
                        TickSimulation();
                    }
                    break;
                case EventType.MouseDown:
                    if (pos.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.button == 0)
                        {
                            mousePos = Event.current.mousePosition;
                            moveMode = true;
                        } else if (Event.current.button == 1)
                        {
                            mousePos = Event.current.mousePosition;
                            rotationMode = true;
                        }
                    }
                    break;
                case EventType.MouseUp:
                    if(Event.current.button == 0)
                    {
                        moveMode = false;
                    }
                    if (Event.current.button == 1)
                    {
                        rotationMode = false;
                    }
                    break;
                case EventType.MouseDrag:
                    if (rotationMode || moveMode)
                    {
                        diff = Event.current.mousePosition - mousePos;
                        mousePos = Event.current.mousePosition;
                    }
                    break;
                case EventType.ScrollWheel:
                    if (moveMode || rotationMode)
                    {
                        scrollWheel = Event.current.delta.y;
                    }
                    break;
            }
            
            if (scrollWheel != 0)
            {
                renderUtils.camera.transform.position += renderUtils.camera.transform.forward * (scrollWheel * scrollSpeed);
                scrollWheel = 0;
            }
            
            if (diff.magnitude > 0)
            {
                if (moveMode)
                {
                    renderUtils.camera.transform.position += new Vector3(diff.x * -moveSpeed * Time.deltaTime, diff.y * moveSpeed * Time.deltaTime, 0);
                }
                /*
                if (rotationMode)
                {
                    renderUtils.camera.transform.RotateAround(new Vector3(0, renderUtils.camera.transform.position.y, 0), Vector3.up, diff.x * rotSpeed);
                }*/
                diff = Vector2.zero;
            }
        }

        private void RenderingMain(Rect pos)
        {
            renderUtils.BeginPreview(pos, EditorStyles.helpBox);
            DrawGround();
            //DrawHurtboxes();
            //DrawHitboxes();
            RenderSub();
            renderUtils.Render(allowScriptableRenderPipeline: true, false);
            renderUtils.EndAndDrawPreview(pos);
        }

        private void RenderSub()
        {
        }

        protected virtual void DrawGround()
        {
            Handles.SetCamera(renderUtils.camera);
            Handles.color = Color.grey;
            for (int i = -10; i <= 10; i++)
            {
                Handles.color = i == 0 ? Color.red : Color.grey;
                Handles.DrawLine(new Vector3(i, 0, -10), new Vector3(i, 0, 10));
                Handles.color = i == 0 ? Color.green : Color.grey;
                Handles.DrawLine(new Vector3(-10, 0, i), new Vector3(10, 0, i));
            }
            Handles.color = Color.cyan;
            Handles.DrawLine(Vector3.zero, new Vector3(0, 10, 0));
        }
        
        protected virtual void CreateFighter()
        {
            /*
            if(visualFighterSceneReference != null)
            {
                DestroyImmediate(visualFighterSceneReference);
                visualFighterSceneReference = null;
            }
            visualFighterPrefab = tempFighter;
            visualFighterSceneReference = renderUtils.InstantiatePrefabInScene(visualFighterPrefab.gameObject);
            ResetFighterVariables();*/
        }
    }
}