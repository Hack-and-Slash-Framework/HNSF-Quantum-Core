using System.Collections.Generic;
using HnSF.sessionhandling.handlers;
using UnityEngine;

namespace HnSF.sessionhandling
{
    public class SessionHandlerManager : MonoBehaviour
    {
        public Dictionary<string, SessionHandlerBase> sessionHandlers = new Dictionary<string, SessionHandlerBase>();

        [Header("Prefabs")]
        public SessionHandlerLocalMatch localMatchSessionHandlerPrefab;
        
        public T CreateSessionHandler<T>(string sessionID, T sessionHandlerPrefab) where T : SessionHandlerBase
        {
            if (sessionHandlerPrefab == null) return null;
            if (sessionHandlers.ContainsKey(sessionID)) return null;
            SessionHandlerBase g = GameObject.Instantiate(sessionHandlerPrefab, transform, false);
            if (!g.Initialize()) return null;
            g.id = sessionID;
            sessionHandlers.Add(sessionID, g);
            return g as T;
        }

        public void DestroySessionHandler(string sessionID, bool teardown = true)
        {
            if (!sessionHandlers.TryGetValue(sessionID, out var handler)) return;
            if(teardown) handler.Teardown();
            GameObject.Destroy(handler.gameObject);
            sessionHandlers.Remove(sessionID);
        }

        public bool TryGetSessionHandler(string sessionID, out SessionHandlerBase sessionHandler)
        {
            return sessionHandlers.TryGetValue(sessionID, out sessionHandler);
        }
    }
}