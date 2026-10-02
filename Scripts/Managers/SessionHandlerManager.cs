using System.Collections.Generic;
using HnSF.sessionhandling.handlers;
using UnityEngine;

namespace HnSF.sessionhandling
{
    public class SessionHandlerManager : MonoBehaviour
    {
        protected Dictionary<string, SessionHandlerBase> sessionHandlers = new Dictionary<string, SessionHandlerBase>();

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
        
        public bool SessionHandlerExists(string sessionID)
        {
            return sessionHandlers.ContainsKey(sessionID);
        }

        public T GetSessionHandler<T>(string sessionID) where T : SessionHandlerBase
        {
            return sessionHandlers.TryGetValue(sessionID, out var handler) ? (T)handler : null;
        }

        
        public bool TryGetSessionHandler<T>(string sessionID, out T sessionHandler) where T : SessionHandlerBase
        {
            sessionHandler = null;
            if (sessionHandlers.TryGetValue(sessionID, out var handler))
            {
                sessionHandler = (T)handler;
                return true;
            }
            return false;
        }
    }
}