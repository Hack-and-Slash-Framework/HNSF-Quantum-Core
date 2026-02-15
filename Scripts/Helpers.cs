#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Quantum;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace HnSF
{
    public static class GeneralHelpers
    {
#if UNITY_EDITOR
        [MenuItem("CONTEXT/AudioSource/Realistic Setup")]
        public static void RalisticRolloff(MenuCommand command)
        {
            Undo.RecordObject(command.context, "AudioSource Realistic Setup");
            RealisticRolloff(((AudioSource)command.context));
            SFXSpatialBlend(((AudioSource)command.context));
            EditorUtility.SetDirty(command.context);
        }

        [MenuItem("CONTEXT/AudioSource/Linear Rolloff")]
        public static void LinearRolloff(MenuCommand command)
        {
            Undo.RecordObject(command.context, "AudioSource Linear Setup");
            LinearRolloff((AudioSource)command.context);
            SFXSpatialBlend(((AudioSource)command.context));
            EditorUtility.SetDirty(command.context);
        }
#endif
        
        public static readonly Dictionary<string, string> RegionToDisplayName = new Dictionary<string, string>()
        {
            { "us", "US East (Washington D.C.)" },
            { "usw", "US West (San Jose)" },
            { "ussc", "US South Central (Dallas)" },
            { "cae", "Canada East (Montreal)" },
            { "sa", "South America (Sao Paulo)" },
            { "eu", "Europe (Amsterdam)" },
            { "asia", "Asia (Singapore)" },
            { "jp", "Japan (Tokyo)" },
            { "in", "India (Chennai)" },
            { "za", "South Africa (Johannesburg)" },
            { "hk", "Hong Kong" },
            { "tr", "Turkey (Istanbul)" },
            { "kr", "South Korea (Seoul)" },
            { "uae", "United Arab Emirates (Dubai)" },
            { "au", "Australia (Sydney)" },
            { "cn", "China Mainland (Shanghai)" },
            { "ru", "Russia" },
            { "rue", "Russia East" }
        };

        public static readonly List<(int, string)> PingToDisplayColor = new List<(int, string)>()
        {
            (200, "#e80909"),
            (150, "#e86609"),
            (100, "#9e9405"),
            (83, "#005500")
        };

        public static string GetPingColor(int ping)
        {
            var pingColor = PingToDisplayColor[0].Item2;
            foreach (var pingTier in PingToDisplayColor)
            {
                if (ping > pingTier.Item1) continue;
                pingColor = pingTier.Item2;
            }

            return pingColor;
        }

        public static string TryConvertQuantumRegionCodeToDisplayName(string regionCode)
        {
            return RegionToDisplayName.GetValueOrDefault(regionCode, regionCode);
        }

        public static float Eerp(float a, float b, float t)
        {
            return a * Mathf.Exp(t * Mathf.Log(b / a));
        }

        public static bool TryGetChild(this Transform t, int index, out Transform transform)
        {
            transform = null;
            try
            {
                transform = t.GetChild(index);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void RealisticRolloff(AudioSource AS)
        {
            var animCurve = new AnimationCurve(
                new Keyframe(AS.minDistance, 1f),
                new Keyframe(AS.minDistance + (AS.maxDistance - AS.minDistance) / 4f, .35f),
                new Keyframe(AS.maxDistance, 0f));

            AS.rolloffMode = AudioRolloffMode.Custom;
            animCurve.SmoothTangents(1, .025f);
            AS.SetCustomCurve(AudioSourceCurveType.CustomRolloff, animCurve);

            AS.dopplerLevel = 0f;

            SFXSpatialBlend(AS);
        }

        public static void LinearRolloff(AudioSource AS)
        {
            var animCurve = new AnimationCurve(
                new Keyframe(AS.minDistance, 1f),
                new Keyframe(AS.maxDistance, 0f));

            AS.rolloffMode = AudioRolloffMode.Linear;
            AS.SetCustomCurve(AudioSourceCurveType.CustomRolloff, animCurve);

            AS.dopplerLevel = 0f;

            SFXSpatialBlend(AS);
        }

        public static void SFXSpatialBlend(AudioSource AS)
        {
            var spatialBlendCurve = new AnimationCurve(
                new Keyframe(AS.minDistance / 2.0f, 0f),
                new Keyframe(AS.maxDistance, 1f));

            var spreadCurve = new AnimationCurve(
                new Keyframe(AS.minDistance, 0.3f),
                new Keyframe(AS.maxDistance, 0.0f));

            AS.SetCustomCurve(AudioSourceCurveType.SpatialBlend, spatialBlendCurve);
            AS.SetCustomCurve(AudioSourceCurveType.Spread, spreadCurve);
        }

        public static async UniTask<string> GetAssetRefID(AssetReference assetReference)
        {

            var loc = Addressables.LoadResourceLocationsAsync(assetReference);
            await loc;
            var key = loc.Result.First()?.PrimaryKey;

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError($"Couldn't find ID for reference ({assetReference}).");
            }

            return key;
        }

        public static Vector3 WorldToScreenSpace(Vector3 worldPos, Camera cam, RectTransform area)
        {
            Vector3 screenPoint = cam.WorldToScreenPoint(worldPos);
            screenPoint.z = 0;

            Vector2 screenPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screenPoint, cam, out screenPos))
            {
                return screenPos;
            }

            return screenPoint;
        }

        public static Vector2 WorldToCanvasPosition(Canvas canvas, Camera worldCamera, Vector3 worldPosition)
        {
            //Vector position (percentage from 0 to 1) considering camera size.
            //For example (0,0) is lower left, middle is (0.5,0.5)
            Vector2 viewportPoint = worldCamera.WorldToViewportPoint(worldPosition);

            var rootCanvasTransform =
                (canvas.isRootCanvas ? canvas.transform : canvas.rootCanvas.transform) as RectTransform;
            var rootCanvasSize = rootCanvasTransform!.rect.size;
            //Calculate position considering our percentage, using our canvas size
            //So if canvas size is (1100,500), and percentage is (0.5,0.5), current value will be (550,250)
            var rootCoord = (viewportPoint - rootCanvasTransform.pivot) * rootCanvasSize;
            if (canvas.isRootCanvas)
                return rootCoord;

            var rootToWorldPos = rootCanvasTransform.TransformPoint(rootCoord);
            return canvas.transform.InverseTransformPoint(rootToWorldPos);
        }

        public static Vector2 GetSnapToPositionToBringChildIntoView(this ScrollRect instance, RectTransform child)
        {
            Canvas.ForceUpdateCanvases();
            Vector2 viewportLocalPosition = instance.viewport.localPosition;
            Vector2 childLocalPosition = child.localPosition;
            Vector2 result = new Vector2(
                0 - (viewportLocalPosition.x + childLocalPosition.x),
                0 - (viewportLocalPosition.y + childLocalPosition.y)
            );
            return result;
        }
        
        public static int? GetPlayerSlot(QuantumGame game, PlayerRef playerRef)
        {
            if (game.PlayerIsLocal(playerRef))
            {
                return game.GetLocalPlayerSlots()[game.GetLocalPlayers().IndexOf(playerRef)];
            }
            else
            {
                return null;
            }
        }
        
        public static T FindNextSelectableFromDirection<T>(T currentSelection, Vector3 dir, T[] allUiItems, bool selectionWrapAround = true) where T : MonoBehaviour
        {
            dir = dir.normalized;
            Vector3 localDir = Quaternion.Inverse(currentSelection.transform.rotation) * dir;
            Vector3 pos = currentSelection.transform.TransformPoint(GetPointOnRectEdge(currentSelection.transform as RectTransform, localDir));
            float maxScore = Mathf.NegativeInfinity;
            float maxFurthestScore = Mathf.NegativeInfinity;
            float score = 0;

            bool wantsWrapAround = selectionWrapAround;

            T bestPick = null;
            T bestFurthestPick = null;

            for (int i = 0; i < allUiItems.Length; ++i)
            {
                T sel = allUiItems[i];

                if (sel == currentSelection)
                    continue;

                //if (!sel.IsInteractable() || sel.navigation.mode == Navigation.Mode.None)
                //    continue;
                
                var selRect = sel.transform as RectTransform;
                Vector3 selCenter = selRect != null ? (Vector3)selRect.rect.center : Vector3.zero;
                Vector3 myVector = sel.transform.TransformPoint(selCenter) - pos;

                // Value that is the distance out along the direction.
                float dot = Vector3.Dot(dir, myVector);

                // If element is in wrong direction and we have wrapAround enabled check and cache it if furthest away.
                if (wantsWrapAround && dot < 0)
                {
                    score = -dot * myVector.sqrMagnitude;

                    if (score > maxFurthestScore)
                    {
                        maxFurthestScore = score;
                        bestFurthestPick = sel;
                    }

                    continue;
                }

                // Skip elements that are in the wrong direction or which have zero distance.
                // This also ensures that the scoring formula below will not have a division by zero error.
                if (dot <= 0)
                    continue;

                // This scoring function has two priorities:
                // - Score higher for positions that are closer.
                // - Score higher for positions that are located in the right direction.
                // This scoring function combines both of these criteria.
                // It can be seen as this:
                //   Dot (dir, myVector.normalized) / myVector.magnitude
                // The first part equals 1 if the direction of myVector is the same as dir, and 0 if it's orthogonal.
                // The second part scores lower the greater the distance is by dividing by the distance.
                // The formula below is equivalent but more optimized.
                //
                // If a given score is chosen, the positions that evaluate to that score will form a circle
                // that touches pos and whose center is located along dir. A way to visualize the resulting functionality is this:
                // From the position pos, blow up a circular balloon so it grows in the direction of dir.
                // The first Selectable whose center the circular balloon touches is the one that's chosen.
                score = dot / myVector.sqrMagnitude;

                if (score > maxScore)
                {
                    maxScore = score;
                    bestPick = sel;
                }
            }

            if (wantsWrapAround && null == bestPick) return bestFurthestPick;

            return bestPick;
        }
        
        private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
        {
            if (rect == null)
                return Vector3.zero;
            if (dir != Vector2.zero)
                dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);
            return dir;
        }
    }

    public static class RectTransformExtensions
    {
        public static RectTransform Left(this RectTransform rt, float x)
        {
            rt.offsetMin = new Vector2(x, rt.offsetMin.y);
            return rt;
        }

        public static RectTransform Right(this RectTransform rt, float x)
        {
            rt.offsetMax = new Vector2(-x, rt.offsetMax.y);
            return rt;
        }

        public static RectTransform Bottom(this RectTransform rt, float y)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, y);
            return rt;
        }

        public static RectTransform Top(this RectTransform rt, float y)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -y);
            return rt;
        }
    }
}