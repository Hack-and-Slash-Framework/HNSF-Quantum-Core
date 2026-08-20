using Quantum;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HnSF.sessionhandling.handlers
{
    public class SessionHandlerMatch : SessionHandlerBase
    {
        public UnityEvent<int> OnQuitMatch = new();
        
        public bool inProgress;
        public QuantumRunner runner;

        public MatchHandlerBase matchHandlerInstance;
        
        public LoadedAssetHandleWrapper selectedMapDefinition;
        
        public override bool Initialize()
        {
            return base.Initialize();
        }

        public override void Teardown()
        {
            if (TornDown)
                return;
            
            if (matchHandlerInstance)
            {
                matchHandlerInstance.Teardown();
                Destroy(matchHandlerInstance);
            }
            
            base.Teardown();
        }
        
        public void InitMatch()
        {
            inProgress = true;
            matchHandlerInstance.gameRunner = runner;
            matchHandlerInstance.OnQuitMatch.AddListener(WhenQuitMatch);
        }

        private void WhenQuitMatch(int quantumClientId)
        {
            matchHandlerInstance.OnQuitMatch.RemoveListener(WhenQuitMatch);
            OnQuitMatch.Invoke(quantumClientId);
        }
    }
}