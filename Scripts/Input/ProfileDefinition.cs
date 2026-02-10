using System;
using System.Collections.Generic;
using UnityEngine;

namespace HnSF.Input
{
    [System.Serializable]
    public struct ProfileDefinition
    {
        [System.Serializable]
        public struct CameraVariables
        {
            public float deadzoneHoz;
            public float deadzoneVert;
            public float speedHoz;
            public float speedVert;
            public float speedLockOnHoz;
            public float speedLockOnVert;
        }

        [SerializeField] public bool undeletable;
        [SerializeField] public byte version;
        [SerializeField] public string profileName;
        [SerializeField] public string overrides;
        [SerializeField] public int lockOnType;
        [SerializeField] public CameraVariables controllerCam;
        [SerializeField] public CameraVariables keyboardCam;

        public bool IsValid()
        {
            if (String.IsNullOrEmpty(profileName)) return false;
            return true;
        }
    }
}