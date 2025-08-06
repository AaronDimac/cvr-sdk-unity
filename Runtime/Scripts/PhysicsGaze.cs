using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cognitive3D;

//physics raycast from camera
//adds gazepoint at hit.point

namespace Cognitive3D
{
    [AddComponentMenu("Cognitive3D/Internal/Physics Gaze")]
    public class PhysicsGaze : GazeBase
    {
        public delegate void onGazeTick();
        /// <summary>
        /// Called on a 0.1 second interval
        /// </summary>
        public static event onGazeTick OnGazeTick;
        private static void InvokeGazeTickEvent() { if (OnGazeTick != null) { OnGazeTick(); } }

        public bool experimentalCanvasGaze;

        public override void Initialize()
        {
            base.Initialize();
            if (GameplayReferences.HMD == null) { Cognitive3D.Util.logWarning("HMD is null! Physics Gaze needs a camera to function"); }
            StartCoroutine(Tick());
            Cognitive3D_Manager.OnPreSessionEnd += OnEndSessionEvent;
        }

        IEnumerator Tick()
        {
            if (GameplayReferences.HMD == null) { yield return null; }

            while (Cognitive3D_Manager.IsInitialized)
            {
                yield return Cognitive3D_Manager.PlayerSnapshotInverval;
                
                try
                {
                    InvokeGazeTickEvent();
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }

                Ray ray = GazeHelper.GetCurrentWorldGazeRay();

                //do this once here, so we don't need to iterate over canvases twice (once for world, once for dynamics)
                float canvasDistance = 0;
                Canvas canvasHit = null;
                Vector3 canvasHitWorldPosition = Vector3.zero;
                bool didHitCanvas = false;
                DynamicObject canvasDynamic = null;

                if (experimentalCanvasGaze)
                {
                    didHitCanvas = RaycastToCanvas(ray.origin, ray.direction, out canvasDistance, out canvasHit, out canvasHitWorldPosition);
                    if (didHitCanvas)
                    {
                        canvasDynamic = canvasHit.GetComponent<DynamicObject>();
                    }
                }

                if (Cognitive3D_Preferences.Instance.EnableGaze == true && GameplayReferences.HMDCameraComponent && DynamicRaycast(ray.origin, ray.direction, GameplayReferences.HMDCameraComponent.farClipPlane, 0.05f, out var hitDistance, out var hitDynamic, out var hitWorld, out var hitLocal, out var hitcoord)) //hit dynamic
                {
                    if (didHitCanvas && hitDistance > canvasDistance)
                    {
                        //hit a dynamic, but also hit a canvas nearer than this dynamic object

                        if (canvasDynamic != null) //dynamic canvas
                        {
                            var canvasLocal = canvasHit.transform.InverseTransformPoint(canvasHitWorldPosition);
                            GazeCore.RecordGazePoint(Util.Timestamp(Time.frameCount), canvasDynamic.GetId(), canvasLocal, ray.origin, GameplayReferences.HMD.rotation);
                        }
                        else //world canvas that is closer
                        {
                            GazeCore.RecordGazePoint(Util.Timestamp(Time.frameCount), canvasHitWorldPosition, GameplayReferences.HMD.position, GameplayReferences.HMD.rotation);
                        }
                    }
                    else
                    {
                        string ObjectId = hitDynamic.GetId();
                        var mediacomponent = hitDynamic.GetComponent<MediaComponent>();
                        if (mediacomponent != null)
                        {
                            var mediatime = mediacomponent.IsVideo ? (int)((mediacomponent.VideoPlayerFrame / mediacomponent.VideoPlayerFrameRate) * 1000) : 0;
                            var mediauvs = hitcoord;
                            GazeCore.RecordGazePoint(Util.Timestamp(Time.frameCount), ObjectId, hitLocal, GameplayReferences.HMD.position, GameplayReferences.HMD.rotation, mediacomponent.MediaId, mediatime, mediauvs);
                        }
                        else
                        {
                            GazeCore.RecordGazePoint(Util.Timestamp(Time.frameCount), ObjectId, hitLocal, ray.origin, GameplayReferences.HMD.rotation);
                        }

                        //debugging
                        DrawGazePoint(GameplayReferences.HMD.position, hitWorld, new Color(1, 0, 1, 0.5f));

                        //active session view
                        AddGazeToDisplay(hitWorld, hitLocal, hitDynamic);
                    }
                }
                else if (Cognitive3D_Preferences.Instance.EnableGaze == true && GameplayReferences.HMDCameraComponent && Physics.Raycast(ray, out var hit, GameplayReferences.HMDCameraComponent.farClipPlane, Cognitive3D_Preferences.Instance.GazeLayerMask, Cognitive3D_Preferences.Instance.TriggerInteraction))
                {
                    if (didHitCanvas && hit.distance > canvasDistance)
                    {
                        if (canvasDynamic != null) //dynamic canvas
                        {
                            var canvasLocal = canvasHit.transform.InverseTransformPoint(canvasHitWorldPosition);
                            GazeCore.RecordGazePoint(Util.Timestamp(Time.frameCount), canvasDynamic.GetId(), canvasLocal, ray.origin, GameplayReferences.HMD.rotation);
                        }
                        else //world canvas that is closer
                        {
                            GazeCore.RecordGazePoint(Util.Timestamp(Time.frameCount), canvasHitWorldPosition, GameplayReferences.HMD.position, GameplayReferences.HMD.rotation);
                        }
                    }
                    else
                    {
                        Vector3 pos = GameplayReferences.HMD.position;
                        Vector3 gazepoint = hit.point;
                        Quaternion rot = GameplayReferences.HMD.rotation;

                        //hit world
                        GazeCore.RecordGazePoint(Util.Timestamp(Time.frameCount), gazepoint, pos, rot);

                        //debugging
                        DrawGazePoint(pos,gazepoint,Color.red);

                        //active session view
                        AddGazeToDisplay(hit.point);
                    }
                }
                else if (GameplayReferences.HMD) //hit sky / farclip / gaze disabled. record HMD position and rotation
                {
                    if (didHitCanvas)
                    {
                        if (canvasDynamic != null) //dynamic canvas
                        {
                            var canvasLocal = canvasHit.transform.InverseTransformPoint(canvasHitWorldPosition);
                            GazeCore.RecordGazePoint(Util.Timestamp(Time.frameCount), canvasDynamic.GetId(), canvasLocal, ray.origin, GameplayReferences.HMD.rotation);
                        }
                        else //world canvas that is closer
                        {
                            GazeCore.RecordGazePoint(Util.Timestamp(Time.frameCount), canvasHitWorldPosition, GameplayReferences.HMD.position, GameplayReferences.HMD.rotation);
                        }
                    }
                    else
                    {

                        Vector3 pos = GameplayReferences.HMD.position;
                        Quaternion rot = GameplayReferences.HMD.rotation;
                        Vector3 displayPosition = GameplayReferences.HMD.forward * GameplayReferences.HMDCameraComponent.farClipPlane;
                        GazeCore.RecordGazePoint(Util.Timestamp(Time.frameCount), pos, rot);

                        //debugging
                        if (DrawLines)
                            Debug.DrawRay(pos, displayPosition, Color.cyan, 0.1f);

                        //active session view
                        AddGazeToDisplay(displayPosition);

                    }
                }
            }
        }

        void AddGazeToDisplay(Vector3 worldPoint)
        {
            if (DisplayGazePoints[DisplayGazePoints.Count] == null)
                DisplayGazePoints[DisplayGazePoints.Count] = new ThreadGazePoint();
            DisplayGazePoints[DisplayGazePoints.Count].WorldPoint = worldPoint;
            DisplayGazePoints[DisplayGazePoints.Count].LocalPoint = Vector3.zero;
            DisplayGazePoints[DisplayGazePoints.Count].Transform = null;
            DisplayGazePoints[DisplayGazePoints.Count].IsLocal = false;
            DisplayGazePoints.Update();
        }

        void AddGazeToDisplay(Vector3 worldPoint, Vector3 localPoint, DynamicObject hitDynamic)
        {
            if (DisplayGazePoints[DisplayGazePoints.Count] == null)
                DisplayGazePoints[DisplayGazePoints.Count] = new ThreadGazePoint();
            DisplayGazePoints[DisplayGazePoints.Count].WorldPoint = worldPoint;
            DisplayGazePoints[DisplayGazePoints.Count].LocalPoint = localPoint;
            DisplayGazePoints[DisplayGazePoints.Count].Transform = hitDynamic.transform;
            DisplayGazePoints[DisplayGazePoints.Count].IsLocal = true;
            DisplayGazePoints.Update();
        }

        void DrawGazePoint(Vector3 start, Vector3 worldPoint,Color color)
        {
            if (!DrawLines) { return; }
            Debug.DrawLine(start, worldPoint, color, 0.1f);
            Debug.DrawRay(worldPoint, Vector3.right, Color.red, 10);
            Debug.DrawRay(worldPoint, Vector3.forward, Color.blue, 10);
            Debug.DrawRay(worldPoint, Vector3.up, Color.green, 10);
        }

        private void OnEndSessionEvent()
        {
            Cognitive3D_Manager.OnPreSessionEnd -= OnEndSessionEvent;
            Destroy(this);
        }

        public bool DrawLines = false;

        RectTransform[] rts = new RectTransform[0];

        //option to check against pre-set canvases rather than finding them every frame
        public bool useOverrideCanvases;
        public List<Canvas> overrideTargetCanvases;

        bool RaycastToCanvas(Vector3 position, Vector3 forward, out float distance, out Canvas hit, out Vector3 worldPosition)
        {
            if (useOverrideCanvases)
            {
                //get canvases every frame, but cache rect transform references
                if (overrideTargetCanvases.Count != rts.Length)
                {
                    rts = new RectTransform[overrideTargetCanvases.Count];
                    for (int i = 0; i < overrideTargetCanvases.Count; i++)
                    {
                        //TODO handle null canvases in list
                        rts[i] = overrideTargetCanvases[i].GetComponent<RectTransform>();
                    }
                }

                Canvas hitCanvas = null;
                float hitDistance = 99999;
                for (int i = 0; i < rts.Length; i++)
                {
                    float tempDistance;
                    bool didHitCanvas = DrawDebugRect(position, forward, rts[i], out tempDistance);
                    if (didHitCanvas && tempDistance < hitDistance)
                    {
                        hitDistance = tempDistance;
                        hitCanvas = overrideTargetCanvases[i];
                    }
                }

                if (hitCanvas != null)
                {
                    if (DrawLines)
                    {
                        Debug.DrawLine(position, position + forward * hitDistance, Color.green);
                    }
                    worldPosition = position + forward * hitDistance;
                    hit = hitCanvas;
                    distance = hitDistance;
                    return true;
                }
            }
            else
            {
                //get canvases every tick, but cache rect transform references
                var canvases = FindObjectsOfType<Canvas>();
                if (canvases.Length != rts.Length)
                {
                    rts = new RectTransform[canvases.Length];
                    for (int i = 0; i < canvases.Length; i++)
                        rts[i] = canvases[i].GetComponent<RectTransform>();
                }

                Canvas hitCanvas = null;
                float hitDistance = 99999;
                for (int i = 0; i < rts.Length; i++)
                {
                    float tempDistance;
                    bool didHitCanvas = DrawDebugRect(position, forward, rts[i], out tempDistance);
                    if (didHitCanvas && tempDistance < hitDistance)
                    {
                        hitDistance = tempDistance;
                        hitCanvas = canvases[i];
                    }
                }

                if (hitCanvas != null)
                {
                    if (DrawLines)
                    {
                        Debug.DrawLine(position, position + forward * hitDistance, Color.green);
                    }
                    worldPosition = position + forward * hitDistance;
                    hit = hitCanvas;
                    distance = hitDistance;
                    return true;
                }
            }

            worldPosition = Vector3.zero;
            hit = null;
            distance = 0;
            return false;
        }

        bool DrawDebugRect(Vector3 pos, Vector3 forward, RectTransform rt, out float hitDistance)
        {
            var halfsize0 = rt.sizeDelta[0] / 2;
            var halfsize1 = rt.sizeDelta[1] / 2;

            Vector3 bottomLeft = new Vector3(-halfsize0, -halfsize1, 0);
            Vector3 bottomRight = new Vector3(halfsize0, -halfsize1, 0);
            Vector3 topLeft = new Vector3(-halfsize0, halfsize1, 0);
            Vector3 topRight = new Vector3(halfsize0, halfsize1, 0);

            //transform matrix for getting the normal
            Matrix4x4 m4 = rt.localToWorldMatrix;
            var wbottomLeft = m4.MultiplyPoint(bottomLeft);
            var wbottomRight = m4.MultiplyPoint(bottomRight);
            var wtopLeft = m4.MultiplyPoint(topLeft);

            //raycast to the surface of the canvas. need to get the distance
            float distance;
            var m_Normal = Vector3.Normalize(Vector3.Cross(wbottomRight - wbottomLeft, wtopLeft - wbottomLeft));
            var m_Distance = 0f - Vector3.Dot(m_Normal, wbottomLeft);
            bool hit = FastRaycast(pos, forward, m_Normal, m_Distance, out distance);

            Vector3 worldHitPosition;
            //hitting the plane in world space, need to convert the local hit position
            if (hit)
            {
                //world hit point to local hit point
                worldHitPosition = pos + forward * distance;
                Vector3 twoDPoint = rt.InverseTransformPoint(worldHitPosition);
                bool inPolygon = IsPointInPolygon4(bottomLeft, bottomRight, topRight, topLeft, twoDPoint);

                if (DrawLines)
                {
                    Debug.DrawRay(twoDPoint, Vector3.forward, Color.blue);
                    Debug.DrawLine(bottomLeft, bottomRight, inPolygon ? Color.green : Color.red);
                    Debug.DrawLine(bottomLeft, topLeft, inPolygon ? Color.green : Color.red);
                    Debug.DrawLine(bottomRight, topRight, inPolygon ? Color.green : Color.red);
                    Debug.DrawLine(topLeft, topRight, inPolygon ? Color.green : Color.red);
                }

                if (inPolygon)
                {
                    hitDistance = distance;
                    return true;
                }
            }

            hitDistance = 0;
            return false;
        }

        //only marginally faster than constructing a plane and using that
        public bool FastRaycast(Vector3 pos, Vector3 forward, Vector3 normal, float distance, out float enter)
        {
            float num = Vector3.Dot(forward, normal);
            float num2 = 0f - Vector3.Dot(pos, normal) - distance;
            enter = num2 / num;
            return enter > 0f;
        }

        //could be a loop, but unwrapped to avoid vector3[] garbage
        //known to always be 4 points
        private static bool IsPointInPolygon4(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 testPoint)
        {
            bool result = false;

            // Only using x and z coordinates because Unity is "y-up" and boundary is infinitely high
            if (a.y < testPoint.y && d.y >= testPoint.y || d.y < testPoint.y && a.y >= testPoint.y)
            {
                if (a.x + (testPoint.y - a.y) / (d.y - a.y) * (d.x - a.x) < testPoint.x)
                {
                    result = !result;
                }
            }

            // Only using x and z coordinates because Unity is "y-up" and boundary is infinitely high
            if (b.y < testPoint.y && a.y >= testPoint.y || a.y < testPoint.y && b.y >= testPoint.y)
            {
                if (b.x + (testPoint.y - b.y) / (a.y - b.y) * (a.x - b.x) < testPoint.x)
                {
                    result = !result;
                }
            }

            // Only using x and z coordinates because Unity is "y-up" and boundary is infinitely high
            if (c.y < testPoint.y && b.y >= testPoint.y || b.y < testPoint.y && c.y >= testPoint.y)
            {
                if (c.x + (testPoint.y - c.y) / (b.y - c.y) * (b.x - c.x) < testPoint.x)
                {
                    result = !result;
                }
            }

            // Only using x and z coordinates because Unity is "y-up" and boundary is infinitely high
            if (d.y < testPoint.y && c.y >= testPoint.y || c.y < testPoint.y && d.y >= testPoint.y)
            {
                if (d.x + (testPoint.y - d.y) / (c.y - d.y) * (c.x - d.x) < testPoint.x)
                {
                    result = !result;
                }
            }
            return result;
        }
    }
}