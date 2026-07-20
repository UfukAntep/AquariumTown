using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

// Purchased from Technology. During an outage the player can approach from
// any direction and press E; higher levels start faster, level five can also
// recover automatically after a short delay.
public class GeneratorUnit : MonoBehaviour
{
    Transform visual;
    TextMesh label;
    float startTimer;
    bool starting;
    float autoTimer;

    public Vector3 InteractionSpot { get { return transform.position + new Vector3(0f, 0f, 1.8f); } }
    public bool PlayerNear(Vector3 position) { return Vector3.Distance(position, transform.position) < 3.2f; }
    public bool CanUse { get { return Game.gm != null && Game.gm.generatorLevel > 0 && Game.events != null && Game.events.PowerOutageActive; } }
    public bool Starting { get { return starting; } }

    public static void Ensure()
    {
        if (Game.generator != null || Game.world == null) return;
        GameObject go = new GameObject("GeneratorUnit");
        go.transform.SetParent(Game.world, false);
        go.transform.position = new Vector3(7f, 0f, -1.5f); // shop's lower-right corner
        Game.generator = go.AddComponent<GeneratorUnit>();
        Game.generator.Build();
    }

    void Build()
    {
        visual = new GameObject("GeneratorVisual").transform;
        visual.SetParent(transform, false);
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (visual == null) return;
        for (int i = visual.childCount - 1; i >= 0; i--) Destroy(visual.GetChild(i).gameObject);
        int level = Game.gm != null ? Game.gm.generatorLevel : 0;
        Material baseMat = MatLib.Get(level > 0 ? new Color(0.95f, 0.67f, 0.14f) : new Color(0.42f, 0.43f, 0.46f));
        Material dark = MatLib.Get(new Color(0.12f, 0.15f, 0.18f));
        B.Prim(PrimitiveType.Cube, "ConcretePad", visual, new Vector3(0f, 0.12f, 0f), Vector3.zero, new Vector3(3.6f, 0.22f, 2.8f), MatLib.Get(new Color(0.52f, 0.55f, 0.58f)), true);
        if (level > 0)
        {
            B.Prim(PrimitiveType.Cube, "GeneratorBody", visual, new Vector3(0f, 0.85f, 0f), Vector3.zero, new Vector3(2.5f, 1.45f, 1.65f), baseMat, true);
            B.Prim(PrimitiveType.Cube, "ControlPanel", visual, new Vector3(0f, 1.02f, 0.86f), Vector3.zero, new Vector3(1.25f, 0.65f, 0.08f), dark);
            for (int i = 0; i < Mathf.Min(level, 3); i++)
                B.Prim(PrimitiveType.Cylinder, "StatusLight", visual, new Vector3(-0.38f + i * 0.38f, 1.15f, 0.94f), new Vector3(90f, 0f, 0f), new Vector3(0.12f, 0.05f, 0.12f), MatLib.Get(i == 0 ? UIKit.Green : UIKit.Blue));
            B.Prim(PrimitiveType.Cylinder, "Exhaust", visual, new Vector3(-0.85f, 1.85f, -0.35f), Vector3.zero, new Vector3(0.22f, 0.55f, 0.22f), dark);
        }
        label = B.Text3D(level > 0 ? "JENERATOR  Sv" + level : "JENERATOR YOK\nPC > TEKNOLOJI", visual, new Vector3(0f, 2.45f, 0f), 0.075f, level > 0 ? Color.white : new Color(1f, 0.7f, 0.25f));
    }

    public void StartByPlayer()
    {
        if (!CanUse || starting) return;
        BeginStart(Mathf.Lerp(6f, 1.4f, (Game.gm.generatorLevel - 1) / 4f));
    }

    public void StartByTechnician(int technicianLevel)
    {
        if (!CanUse || starting) return;
        BeginStart(Mathf.Lerp(4f, 0.8f, (Mathf.Clamp(technicianLevel, 1, 5) - 1) / 4f));
    }

    void BeginStart(float duration)
    {
        starting = true;
        startTimer = duration;
        if (Game.ui != null) Game.ui.Toast("Jenerator calistiriliyor...", 2f);
        Sfx.Play(Snd.Generator, 0.8f);
    }

    void Update()
    {
        if (Game.events == null || !Game.events.PowerOutageActive)
        {
            starting = false;
            autoTimer = 0f;
            return;
        }
        if (Game.gm != null && Game.gm.generatorLevel >= 5 && !starting)
        {
            autoTimer += Time.deltaTime;
            if (autoTimer >= 4f) BeginStart(0.7f);
        }
        if (!starting) return;
        startTimer -= Time.deltaTime;
        if (label != null) label.text = "JENERATOR BASLIYOR  " + Mathf.CeilToInt(Mathf.Max(0f, startTimer)) + "s";
        if (startTimer <= 0f)
        {
            starting = false;
            if (label != null) label.text = "JENERATOR  Sv" + Game.gm.generatorLevel;
            Game.events.RestorePower(true);
        }
    }
}

public static class SchoolBusVisual
{
    public static void Spawn()
    {
        if (Game.world == null) return;
        GameObject bus = new GameObject("SchoolBus");
        bus.transform.SetParent(Game.world, false);
        bus.transform.position = Customer.DoorPos + new Vector3(8f, 0f, 0f);
        Material yellow = MatLib.Get(new Color(1f, 0.72f, 0.08f));
        Material dark = MatLib.Get(new Color(0.08f, 0.12f, 0.18f));
        B.Prim(PrimitiveType.Cube, "BusBody", bus.transform, new Vector3(0f, 1.25f, 0f), Vector3.zero, new Vector3(5.8f, 2.2f, 2.25f), yellow);
        B.Prim(PrimitiveType.Cube, "Windows", bus.transform, new Vector3(0f, 1.65f, -1.14f), Vector3.zero, new Vector3(4.4f, 0.72f, 0.05f), dark);
        for (int i = -1; i <= 1; i += 2)
            B.Prim(PrimitiveType.Cylinder, "Wheel", bus.transform, new Vector3(i * 1.85f, 0.45f, -1.12f), new Vector3(90f, 0f, 0f), new Vector3(0.6f, 0.2f, 0.6f), dark);
        B.Text3D("OKUL GEZISI", bus.transform, new Vector3(0f, 2.75f, 0f), 0.085f, Color.white);
        Object.Destroy(bus, 16f);
    }
}

public class StormWave : MonoBehaviour
{
    float life;

    public static void Spawn(float z)
    {
        if (Game.world == null) return;
        GameObject go = B.Prim(PrimitiveType.Cube, "StormWave", Game.world,
            new Vector3(75f, 0.62f, z), Vector3.zero, new Vector3(18f, 0.12f, 1.15f),
            MatLib.Glass(new Color(0.72f, 0.93f, 1f, 0.72f)));
        go.AddComponent<StormWave>();
    }

    void Update()
    {
        life += Time.deltaTime;
        transform.position += Vector3.left * 12f * Time.deltaTime;
        transform.localScale = new Vector3(transform.localScale.x, 0.12f + Mathf.Sin(life * 8f) * 0.08f, transform.localScale.z);
        if (life > 4.2f) Destroy(gameObject);
    }
}

public static class ManagementRoomSystem
{
    static GameObject root;

    public static void Refresh()
    {
        if (root != null) Object.Destroy(root);
        if (Game.world == null || Game.gm == null || Game.gm.shopUpg.Length <= 4 || Game.gm.shopUpg[4] <= 0) return;
        root = new GameObject("ManagementRoomUpgrades");
        root.transform.SetParent(Game.world, false);
        int level = Game.gm.shopUpg[4];
        float height = level == 1 ? 1.05f : Mathf.Lerp(1.6f, 3.6f, (level - 2) / 3f);
        Material wall = MatLib.Get(level == 1 ? new Color(0.48f, 0.72f, 0.78f) :
            Color.Lerp(new Color(0.32f, 0.55f, 0.72f), new Color(0.22f, 0.3f, 0.5f), (level - 2) / 3f));
        // Level one is visibly a cheap fence; later levels become increasingly
        // tall solid walls. Level five matches the 3.6m shop wall.
        if (level == 1) BuildFence(root.transform, wall);
        else
        {
            B.Prim(PrimitiveType.Cube, "RoomBottom", root.transform, new Vector3(1.7f, height * 0.5f, -3f), Vector3.zero, new Vector3(10.5f, height, 0.16f), wall, true);
            B.Prim(PrimitiveType.Cube, "RoomLeft", root.transform, new Vector3(-3.5f, height * 0.5f, 2.5f), Vector3.zero, new Vector3(0.16f, height, 11f), wall, true);
            B.Prim(PrimitiveType.Cube, "RoomTopA", root.transform, new Vector3(-1.5f, height * 0.5f, 8f), Vector3.zero, new Vector3(4f, height, 0.16f), wall, true);
            B.Prim(PrimitiveType.Cube, "RoomTopB", root.transform, new Vector3(5.2f, height * 0.5f, 8f), Vector3.zero, new Vector3(3.5f, height, 0.16f), wall, true);
        }
        if (Game.gm.shopUpg.Length > 5 && Game.gm.shopUpg[5] > 0)
        {
            SecurityCameraSystem.BeginMonitorLayout();
            Material desk = MatLib.Get(new Color(0.24f, 0.3f, 0.4f));
            GameObject cameraDesk = new GameObject("CameraDeskStation");
            cameraDesk.transform.SetParent(root.transform, false);
            cameraDesk.transform.localPosition = new Vector3(-1.35f, 0f, -1.45f); // left-side marked slot, aligned with the generator
            B.Prim(PrimitiveType.Cube, "CameraDesk", cameraDesk.transform, new Vector3(0f, 0.72f, 0f), Vector3.zero, new Vector3(2.8f, 1.25f, 1.1f), desk, true);
            int monitors = Mathf.Clamp(Game.gm.shopUpg[5], 1, 5);
            for (int i = 0; i < monitors; i++)
            {
                GameObject monitor = B.Prim(PrimitiveType.Cube, "CameraMonitor", cameraDesk.transform,
                    new Vector3(-0.9f + (i % 3) * 0.9f, 1.65f + (i / 3) * 0.62f, 0.05f), Vector3.zero,
                    new Vector3(0.72f, 0.5f, 0.08f), MatLib.Get(new Color(0.18f, 0.65f, 0.85f)));
                SecurityCameraSystem.RegisterMonitor(monitor.GetComponent<Renderer>());
            }
            B.Text3D("KAMERA IZLEME", cameraDesk.transform, new Vector3(0f, 2.55f, 0f), 0.065f, Color.white);
            Game.cameraDesk = cameraDesk.AddComponent<CameraDeskUnit>();
        }
        SecurityCameraSystem.Refresh();
    }

    static void BuildFence(Transform parent, Material material)
    {
        for (float x = -3.5f; x <= 7f; x += 1.5f)
            B.Prim(PrimitiveType.Cube, "FencePost", parent, new Vector3(x, 0.55f, -3f), Vector3.zero, new Vector3(0.14f, 1.1f, 0.14f), material, true);
        for (float z = -3f; z <= 8f; z += 1.5f)
            B.Prim(PrimitiveType.Cube, "FencePost", parent, new Vector3(-3.5f, 0.55f, z), Vector3.zero, new Vector3(0.14f, 1.1f, 0.14f), material, true);
        B.Prim(PrimitiveType.Cube, "FenceRail", parent, new Vector3(1.7f, 0.38f, -3f), Vector3.zero, new Vector3(10.5f, 0.12f, 0.12f), material, true);
        B.Prim(PrimitiveType.Cube, "FenceRail", parent, new Vector3(1.7f, 0.88f, -3f), Vector3.zero, new Vector3(10.5f, 0.12f, 0.12f), material, true);
        B.Prim(PrimitiveType.Cube, "FenceRail", parent, new Vector3(-3.5f, 0.38f, 2.5f), Vector3.zero, new Vector3(0.12f, 0.12f, 11f), material, true);
        B.Prim(PrimitiveType.Cube, "FenceRail", parent, new Vector3(-3.5f, 0.88f, 2.5f), Vector3.zero, new Vector3(0.12f, 0.12f, 11f), material, true);
        AddFenceSegment(parent, material, -1.5f, 4f);
        AddFenceSegment(parent, material, 5.2f, 3.5f);
    }

    static void AddFenceSegment(Transform parent, Material material, float centerX, float length)
    {
        int posts = Mathf.Max(2, Mathf.CeilToInt(length / 1.4f) + 1);
        for (int i = 0; i < posts; i++)
        {
            float x = centerX - length * 0.5f + length * i / (posts - 1f);
            B.Prim(PrimitiveType.Cube, "FencePost", parent, new Vector3(x, 0.55f, 8f), Vector3.zero, new Vector3(0.14f, 1.1f, 0.14f), material, true);
        }
        B.Prim(PrimitiveType.Cube, "FenceRail", parent, new Vector3(centerX, 0.38f, 8f), Vector3.zero, new Vector3(length, 0.12f, 0.12f), material, true);
        B.Prim(PrimitiveType.Cube, "FenceRail", parent, new Vector3(centerX, 0.88f, 8f), Vector3.zero, new Vector3(length, 0.12f, 0.12f), material, true);
    }
}

public class CameraDeskUnit : MonoBehaviour
{
    GameObject viewer;
    RawImage feedImage;
    Text title;
    int feedIndex;
    float openedAt;

    public bool ViewerOpen { get { return viewer != null && viewer.activeSelf; } }
    public bool PlayerNear(Vector3 position) { return Vector3.Distance(position, transform.position) < 3.2f; }

    public void OpenViewer()
    {
        if (SecurityCameraSystem.FeedCount == 0)
        {
            if (Game.ui != null) Game.ui.Toast("Canli yayin icin once Teknoloji bolumunden guvenlik kamerasi satin al.", 4f);
            return;
        }
        EnsureViewer();
        feedIndex = Mathf.Clamp(feedIndex, 0, SecurityCameraSystem.FeedCount - 1);
        RefreshFeed();
        viewer.SetActive(true);
        openedAt = Time.unscaledTime;
        Sfx.Play(Snd.Tick, 0.45f);
    }

    void EnsureViewer()
    {
        if (viewer != null) return;
        viewer = new GameObject("CameraLiveViewer");
        Canvas canvas = viewer.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 45;
        CanvasScaler scaler = viewer.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;
        viewer.AddComponent<GraphicRaycaster>();
        GameObject shade = UIKit.Panel(viewer.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1600f, 900f), new Color(0.025f, 0.04f, 0.07f, 0.97f), false, false);
        GameObject header = UIKit.Panel(shade.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(1160f, 72f), UIKit.BlueDark, true, false);
        title = UIKit.Label(header.transform, "", 24, Color.white, TextAnchor.MiddleCenter);
        GameObject screen = UIKit.Panel(shade.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(1160f, 650f), Color.black, true, true);
        GameObject raw = new GameObject("LiveFeed");
        raw.transform.SetParent(screen.transform, false);
        RectTransform rr = raw.AddComponent<RectTransform>();
        rr.anchorMin = Vector2.zero; rr.anchorMax = Vector2.one; rr.offsetMin = new Vector2(12f, 12f); rr.offsetMax = new Vector2(-12f, -12f);
        feedImage = raw.AddComponent<RawImage>();
        feedImage.color = Color.white;
        AspectRatioFitter fitter = raw.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 16f / 9f;
        UIKit.Btn(shade.transform, new Vector2(-330f, -390f), new Vector2(230f, 58f), UIKit.Blue, "< ONCEKI KAMERA", 17, Previous);
        UIKit.Btn(shade.transform, new Vector2(0f, -390f), new Vector2(230f, 58f), UIKit.Red, "KAPAT (" + ControlBindings.KeyName(ControlAction.Interact) + ")", 17, CloseViewer);
        UIKit.Btn(shade.transform, new Vector2(330f, -390f), new Vector2(230f, 58f), UIKit.Blue, "SONRAKI KAMERA >", 17, Next);
        viewer.SetActive(false);
    }

    void Update()
    {
        if (!ViewerOpen || Time.unscaledTime - openedAt < 0.2f) return;
        if (ControlBindings.Down(ControlAction.Interact) || Input.GetKeyDown(KeyCode.Escape)) CloseViewer();
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Previous();
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Next();
    }

    void Previous() { if (SecurityCameraSystem.FeedCount > 0) { feedIndex = (feedIndex - 1 + SecurityCameraSystem.FeedCount) % SecurityCameraSystem.FeedCount; RefreshFeed(); } }
    void Next() { if (SecurityCameraSystem.FeedCount > 0) { feedIndex = (feedIndex + 1) % SecurityCameraSystem.FeedCount; RefreshFeed(); } }
    void RefreshFeed()
    {
        if (feedImage != null) feedImage.texture = SecurityCameraSystem.GetFeed(feedIndex);
        if (title != null) title.text = "CANLI GUVENLIK YAYINI  -  KAMERA " + (feedIndex + 1) + "/" + SecurityCameraSystem.FeedCount;
    }
    public void CloseViewer() { if (viewer != null) viewer.SetActive(false); Sfx.Play(Snd.Tick, 0.35f); }
    void OnDestroy() { if (viewer != null) Destroy(viewer); if (Game.cameraDesk == this) Game.cameraDesk = null; }
}

public static class SecurityCameraSystem
{
    static GameObject root;
    static List<Renderer> monitors = new List<Renderer>();
    static List<RenderTexture> feeds = new List<RenderTexture>();
    static readonly Vector3[] positions = {
        new Vector3(7f, 3.5f, 25f), new Vector3(3f, 3.5f, 18f), new Vector3(-36f, 3.4f, 3f),
        new Vector3(18f, 3.5f, 22f), new Vector3(-2.8f, 3.2f, 6.7f) };
    static readonly Vector3[] targets = {
        Customer.GateInside, new Vector3(1f, 0f, 20f), new Vector3(-49f, 0f, 2f),
        new Vector3(42f, 0f, 22f), new Vector3(2f, 0f, 5f) };

    public static void BeginMonitorLayout() { monitors.Clear(); }
    public static void RegisterMonitor(Renderer renderer) { if (renderer != null) monitors.Add(renderer); }
    public static int FeedCount { get { return feeds.Count; } }
    public static RenderTexture GetFeed(int index) { return index >= 0 && index < feeds.Count ? feeds[index] : null; }

    public static void Refresh()
    {
        if (root != null) Object.Destroy(root);
        for (int i = 0; i < feeds.Count; i++) if (feeds[i] != null) Object.Destroy(feeds[i]);
        feeds.Clear();
        if (Game.world == null || Game.gm == null || Game.gm.cameraLevel <= 0) return;
        root = new GameObject("SecurityCameras");
        root.transform.SetParent(Game.world, false);
        for (int i = 0; i < Mathf.Min(Game.gm.cameraLevel, positions.Length); i++)
        {
            GameObject cam = new GameObject("SecurityCamera_" + (i + 1));
            cam.transform.SetParent(root.transform, false);
            cam.transform.position = positions[i];
            Material white = MatLib.Get(new Color(0.88f, 0.9f, 0.93f));
            Material lens = MatLib.Get(new Color(0.08f, 0.12f, 0.18f));
            B.Prim(PrimitiveType.Cube, "Mount", cam.transform, Vector3.zero, Vector3.zero, new Vector3(0.25f, 0.6f, 0.25f), white);
            B.Prim(PrimitiveType.Cube, "Camera", cam.transform, new Vector3(0f, -0.35f, 0.35f), new Vector3(18f, i * 37f, 0f), new Vector3(0.55f, 0.38f, 0.8f), white);
            B.Prim(PrimitiveType.Sphere, "Lens", cam.transform, new Vector3(0f, -0.35f, 0.76f), Vector3.zero, Vector3.one * 0.18f, lens);

            GameObject feedObject = new GameObject("LiveFeedCamera");
            feedObject.transform.SetParent(cam.transform, false);
            feedObject.transform.position = positions[i] + Vector3.up * 6.5f;
            feedObject.transform.rotation = Quaternion.LookRotation((targets[i] + Vector3.up * 0.8f) - feedObject.transform.position);
            Camera feedCamera = feedObject.AddComponent<Camera>();
            feedCamera.fieldOfView = 56f;
            feedCamera.nearClipPlane = 0.25f;
            feedCamera.farClipPlane = 180f;
            feedCamera.depth = -20f - i;
            feedCamera.allowHDR = false;
            feedCamera.allowMSAA = true;
            RenderTexture texture = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            texture.name = "SecurityFeed_" + (i + 1);
            texture.antiAliasing = 4;
            texture.filterMode = FilterMode.Bilinear;
            texture.anisoLevel = 4;
            texture.Create();
            feedCamera.targetTexture = texture;
            feeds.Add(texture);
            if (i < monitors.Count && monitors[i] != null)
            {
                Material screen = new Material(monitors[i].sharedMaterial);
                screen.mainTexture = texture;
                if (screen.HasProperty("_BaseMap")) screen.SetTexture("_BaseMap", texture);
                monitors[i].sharedMaterial = screen;
            }
        }
    }
}
