using UnityEngine;

// Camera modes: top-down follow, TPS, first-person and PC zoom.
public class CameraRig : MonoBehaviour
{
    public enum Mode { TopDown, TPS, FPS, PC }
    public Mode mode = Mode.TopDown;

    public Transform target;
    float tpsYaw;
    float tpsPitch = 16f;
    float zoom = 1f; // saved between sessions
    float shakeAmp, shakeTime;
    Vector3 pcFrom;
    Quaternion pcFromRot;
    float pcT;
    bool zoomingIn, zoomingOut;
    System.Action onZoomDone;
    Mode returnMode = Mode.TopDown;

    public static CameraRig Create()
    {
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.backgroundColor = new Color(0.55f, 0.8f, 0.95f);
        cam.fieldOfView = 48f;
        cam.nearClipPlane = 0.2f;
        cam.farClipPlane = 400f;
        camGo.AddComponent<AudioListener>();
        CameraRig rig = camGo.AddComponent<CameraRig>();
        rig.zoom = PlayerPrefs.GetFloat("AT3_Zoom", 1f);
        Game.cam = rig;
        return rig;
    }

    // Existing movement/UI callers use IsTPS to mean a mouse-look,
    // camera-relative player mode. FPS follows the same control rules.
    public bool IsTPS { get { return mode == Mode.TPS || mode == Mode.FPS; } }
    public bool IsFPS { get { return mode == Mode.FPS; } }
    public float TPSYaw { get { return tpsYaw; } }

    public void TogglePlayerMode()
    {
        if (mode == Mode.PC) return;
        Mode previous = mode;
        mode = mode == Mode.TopDown ? Mode.TPS : mode == Mode.TPS ? Mode.FPS : Mode.TopDown;
        if (mode == Mode.TPS)
        {
            if (previous == Mode.TopDown)
                tpsYaw = Game.player != null && Game.player.visual != null ? Game.player.visual.eulerAngles.y : 0f;
            tpsPitch = 16f;
        }
        else if (mode == Mode.FPS)
        {
            tpsPitch = Mathf.Clamp(tpsPitch, -75f, 75f);
        }
        bool mouseLook = mode == Mode.TPS || mode == Mode.FPS;
        Cursor.lockState = mouseLook ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !mouseLook;
        ApplyPlayerVisibility();
    }

    void ApplyPlayerVisibility()
    {
        if (Game.player != null && Game.player.visual != null)
            Game.player.visual.gameObject.SetActive(mode != Mode.FPS);
    }

    public void Shake(float amp, float time) { shakeAmp = amp; shakeTime = time; }

    public void ZoomToPC(System.Action done)
    {
        if (mode == Mode.PC)
        {
            if (zoomingOut)
            {
                zoomingOut = false;
                zoomingIn = true;
                pcT = 1f - Mathf.Clamp01(pcT);
                onZoomDone = done;
            }
            else if (!zoomingIn && done != null)
            {
                done();
            }
            return;
        }
        returnMode = mode;
        mode = Mode.PC;
        pcFrom = transform.position;
        pcFromRot = transform.rotation;
        pcT = 0f;
        zoomingIn = true;
        zoomingOut = false;
        onZoomDone = done;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReturnFromPC()
    {
        if (mode != Mode.PC) return;
        if (zoomingOut) return;
        if (zoomingIn) pcT = 1f - Mathf.Clamp01(pcT);
        else pcT = 0f;
        zoomingIn = false;
        zoomingOut = true;
        onZoomDone = null;
    }

    void Update()
    {
        if (mode != Mode.PC && ControlBindings.Down(ControlAction.Camera) &&
            (Game.ui == null || !Game.ui.AnyMenuOpen))
        {
            if (Game.ui != null) Game.ui.HideCameraTutorial();
            TogglePlayerMode();
        }

        // Mouse controls both third-person orbit and first-person look.
        if ((mode == Mode.TPS || mode == Mode.FPS) && (Game.ui == null || !Game.ui.AnyMenuOpen))
        {
            tpsYaw += Input.GetAxis("Mouse X") * 3.2f;
            float minPitch = mode == Mode.FPS ? -80f : -8f;
            float maxPitch = mode == Mode.FPS ? 80f : 65f;
            tpsPitch = Mathf.Clamp(tpsPitch - Input.GetAxis("Mouse Y") * 2.4f, minPitch, maxPitch);
        }

        // scroll wheel zoom (both camera modes), persisted
        if (mode != Mode.PC && (Game.ui == null || !Game.ui.AnyMenuOpen))
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.005f)
            {
                zoom = Mathf.Clamp(zoom - scroll * 0.6f, 0.5f, 2f);
                PlayerPrefs.SetFloat("AT3_Zoom", zoom);
            }
        }
    }

    void LateUpdate()
    {
        float dt = Time.unscaledDeltaTime;
        Vector3 shake = Vector3.zero;
        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            shake = Random.insideUnitSphere * shakeAmp * (shakeTime > 0f ? 1f : 0f);
        }

        if (mode == Mode.PC && Game.managerDesk != null && Game.managerDesk.monitor != null)
        {
            Transform mon = Game.managerDesk.monitor;
            Vector3 pcPos = mon.position + mon.forward * -1.6f + Vector3.up * 0.1f;
            Quaternion pcRot = Quaternion.LookRotation(mon.position - pcPos);
            pcT += dt * 1.6f;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(pcT));
            if (zoomingIn)
            {
                transform.position = Vector3.Lerp(pcFrom, pcPos, k);
                transform.rotation = Quaternion.Slerp(pcFromRot, pcRot, k);
                if (pcT >= 1f)
                {
                    zoomingIn = false;
                    System.Action cb = onZoomDone;
                    onZoomDone = null;
                    if (cb != null) cb();
                }
            }
            else if (zoomingOut)
            {
                transform.position = Vector3.Lerp(pcPos, pcFrom, k);
                transform.rotation = Quaternion.Slerp(pcRot, pcFromRot, k);
                if (pcT >= 1f)
                {
                    zoomingOut = false;
                    mode = returnMode;
                    bool mouseLook = mode == Mode.TPS || mode == Mode.FPS;
                    Cursor.lockState = mouseLook ? CursorLockMode.Locked : CursorLockMode.None;
                    Cursor.visible = !mouseLook;
                    ApplyPlayerVisibility();
                }
            }
            return;
        }

        if (target == null) return;

        if (mode == Mode.TopDown)
        {
            // closer default view (like the reference game); scroll to adjust
            Vector3 want = target.position + new Vector3(0f, 12f, -7.5f) * zoom;
            transform.position = Vector3.Lerp(transform.position, want, 5f * Time.deltaTime) + shake;
            Quaternion look = Quaternion.LookRotation(target.position + Vector3.up * 1f - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 6f * Time.deltaTime);
        }
        else if (mode == Mode.TPS)
        {
            // Free third-person orbit around the character (yaw + pitch).
            Quaternion orbit = Quaternion.Euler(tpsPitch, tpsYaw, 0f);
            Vector3 pivot = target.position + Vector3.up * 1.7f;
            Vector3 want = pivot - orbit * Vector3.forward * 5.5f * zoom;
            if (want.y < 0.5f) want.y = 0.5f;
            transform.position = Vector3.Lerp(transform.position, want, 14f * Time.deltaTime) + shake;
            Quaternion look = Quaternion.LookRotation(pivot - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 16f * Time.deltaTime);
        }
        else if (mode == Mode.FPS)
        {
            Quaternion look = Quaternion.Euler(tpsPitch, tpsYaw, 0f);
            Vector3 eye = target.position + Vector3.up * 1.65f;
            transform.position = Vector3.Lerp(transform.position, eye, 22f * Time.deltaTime) + shake;
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 22f * Time.deltaTime);
        }
    }
}
