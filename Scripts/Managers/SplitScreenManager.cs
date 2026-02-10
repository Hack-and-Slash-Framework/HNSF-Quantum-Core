using System.Collections.Generic;
using UnityEngine;

namespace HnSF
{
    public class SplitScreenManager : MonoBehaviour
    {
        public delegate void ScreenDelegate(int index);

        public ScreenDelegate OnScreenRegistered;

        [System.Serializable]
        public class RectDefinition
        {
            public Rect[] rects = new Rect[1];
        }

        public bool separateDisplaysPerPlayer;

        [SerializeField] private List<Camera> cameras = new List<Camera>();
        public bool shouldUpdateView;

        public Camera backgroundCamera;

        public List<RectDefinition> splitscreenRects = new();

        public void Init()
        {
            backgroundCamera?.gameObject.SetActive(false);
        }

        public void Activate()
        {
            shouldUpdateView = true;
            UpdateView();
        }

        public void Deactivate()
        {
            shouldUpdateView = false;
        }

        public void RegisterCamera(Camera cam)
        {
            cameras.Add(cam);
            UpdateView();
            OnScreenRegistered?.Invoke(cameras.Count - 1);
        }

        public void UnregisterCamera(Camera cam)
        {
            cameras.Remove(cam);
            UpdateView();
        }

        public void Clear()
        {
            cameras.Clear();
            UpdateView();
        }

        public Camera GetView(int ind)
        {
            if (cameras.Count <= ind) return null;
            return cameras[ind];
        }

        public bool SetView(int index, Camera camera)
        {
            while(index >= cameras.Count) cameras.Add(null);
            cameras[index] = camera;
            UpdateView();
            return true;
        }

        public void UpdateView()
        {
            if (!shouldUpdateView) return;

            if (separateDisplaysPerPlayer && GetDisplayCount() > 1)
            {
                backgroundCamera.gameObject.SetActive(false);
                if (cameras.Count == 0) return;

                for (int i = 0; i < cameras.Count; i++)
                {
                    if (i >= GetDisplayCount())
                    {
                        Debug.LogError("Player is outside of display count. Disabling camera.");
                        SetCameraTargetDisplay(cameras[i], 0);
                        cameras[i].gameObject.SetActive(false);
                        continue;
                    }

#if UNITY_EDITOR

#else
                if(!Display.displays[i].active) Display.displays[i].Activate();
#endif
                    SetCameraTargetDisplay(cameras[i], i);
                }
            }
            else
            {
                backgroundCamera.gameObject.SetActive(false);
                if (cameras.Count == 0) return;
                if (cameras.Count == 3)
                {
                    backgroundCamera.gameObject.SetActive(true);
                }

                for (int i = 0; i < cameras.Count; i++)
                {
                    SetCameraRect(cameras[i], splitscreenRects[cameras.Count - 1].rects[i]);
                }
            }
        }

        public void SetCameraTargetDisplay(Camera cam, int displayIndex)
        {
            if (cam == null) return;
            cam.targetDisplay = displayIndex;

#if HNSF_URP
            if (!cam.gameObject.TryGetComponent<UniversalAdditionalCameraData>(out var ucd)) return;

            foreach (var extraCam in ucd.cameraStack)
            {
                extraCam.targetDisplay = displayIndex;
            }
#endif
        }

        public void SetCameraRect(Camera cam, Rect rect)
        {
            if (cam == null) return;
            cam.rect = rect;

#if HNSF_URP
            if (!cam.gameObject.TryGetComponent<UniversalAdditionalCameraData>(out var ucd)) return;

            foreach (var extraCam in ucd.cameraStack)
            {
                extraCam.rect = rect;
            }
#endif
        }

        int GetDisplayCount()
        {
#if UNITY_EDITOR
            return 8;
#else
        return Display.displays.Length;
#endif
        }
    }
}