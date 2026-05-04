using System;
using System.Collections.Generic;
using Quantum;
using UnityEngine;

namespace HnSF
{
    public class StatePreviewEventHandler : MonoBehaviour, IEditorAwake, IEditorOnDisable
    {
        [NonSerialized] protected List<DispatcherSubscription> quantumSubscriptions = new();

        public GameObject rootObject;
        
        public virtual void Awake()
        {
        }

        public virtual void OnDisable()
        {
            
        }

        public virtual void Initialize()
        {
            
        }

        public virtual void Teardown()
        {
            foreach (var s in quantumSubscriptions)
            {
                QuantumCallback.Unsubscribe(s);
            }
            quantumSubscriptions.Clear();
            Debug.Log("DIsabled EventHandler");
        }
    }
}