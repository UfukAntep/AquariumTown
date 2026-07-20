using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameBootstrap : MonoBehaviour
{
    static bool booted;
    static bool hooked;
    static GameBootstrap instance;
    static Light sun;
    static Material skyMaterial;
    static GameObject gateBarrier;
    static TextMesh gateNameText;
    // Keep the manual open/close sign inside the starting shop boundary.
    static readonly Vector3 GateSignPos = new Vector3(6.5f, 0f, 28.5f);
    static Bounds displayBounds = new Bounds(new Vector3(-25f, 1.9f, 187.5f), new Vector3(58f, 1.4f, 3.5f));
    static List<Fish> displayFish = new List<Fish>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        booted = false; hooked = false; instance = null; sun = null; skyMaterial = null; gateBarrier = null; gateNameText = null;
        displayFish = new List<Fish>();
        Game.Clear();
        MatLib.Clear();
        Sfx.Clear();
        UIKit.Clear();
        AssetLib.Clear();
        QuestSystem.Clear();
        Reviews.Clear();
        Loc.Clear();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (!hooked)
        {
            hooked = true;
            SceneManager.sceneLoaded += delegate { if (!booted) BootNow(); };
        }
        if (!booted) BootNow();
    }

    public static void PrepareRestart()
    {
        booted = false;
        instance = null; sun = null; skyMaterial = null; gateBarrier = null; gateNameText = null;
        displayFish = new List<Fish>();
        Game.Clear();
        MatLib.Clear();
        Sfx.Clear();
        UIKit.Clear();
        AssetLib.Clear();
        Reviews.Clear();
    }

    static void BootNow()
    {
        booted = true;
        Scene s = SceneManager.GetActiveScene();
        GameObject[] roots = s.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++) roots[i].SetActive(false);
        new GameObject("AquariumTown").AddComponent<GameBootstrap>();
    }

    // Scene-aware navigation: uses the real "Game"/"Menu" scenes when they are
    // in Build Settings, otherwise falls back to the in-place soft restart.
    public static void LaunchGame()
    {
        GameManager.SkipMenuOnce = true;
        LoadingScreen.BeginGameLoad();
    }

    public static System.Collections.IEnumerator LoadGameRoutine()
    {
        LoadingScreen.Report(0.02f, "Kayit hazirlaniyor...");
        yield return null; // ensure the loading canvas is rendered before work starts
        if (Application.CanStreamedLevelBeLoaded("Game"))
        {
            PrepareRestart();
            Time.timeScale = 1f;
            AsyncOperation operation = SceneManager.LoadSceneAsync("Game");
            while (!operation.isDone)
            {
                float sceneProgress = Mathf.Clamp01(operation.progress / 0.9f);
                LoadingScreen.Report(0.03f + sceneProgress * 0.17f, "Oyun sahnesi yukleniyor...");
                yield return null;
            }
        }
        else
        {
            LoadingScreen.Report(0.18f, "Oyun dunyasi hazirlaniyor...");
            yield return null;
            SoftRestart();
        }
    }

    public static void GoToMenu()
    {
        GameManager.SkipMenuOnce = false;
        if (Application.CanStreamedLevelBeLoaded("Menu"))
        {
            PrepareRestart();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("Menu");
        }
        else SoftRestart();
    }

    // In-place restart (no scene reload needed, works even if the scene is not
    // in Build Settings): destroy everything, rebuild next frame.
    public static void SoftRestart()
    {
        PrepareRestart();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Scene s = SceneManager.GetActiveScene();
        GameObject[] roots = s.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++) Destroy(roots[i]);
        new GameObject("RestartRunner").AddComponent<RestartRunner>();
    }

    public static void RebuildNow()
    {
        booted = true;
        new GameObject("AquariumTown").AddComponent<GameBootstrap>();
    }

    // Uniform grid: horizontal gap == vertical gap. 5 columns x 2 rows per band
    // = 10 tanks; 8 bands = 80 tanks. A purchasable zone = 2 bands (20 tanks).
    const float Grid = 11f;       // equal spacing in x and z
    const float ColX0 = -8f;      // x of the first (rightmost) column
    const float RowZ0 = 12f;      // z of the bottom row
    const int Cols = 5;
    public const float MapTop = 190f;

    // The opening section follows the player's marked zig-zag: #1 uses the old
    // "Golden Crab" plot, #2 is directly below, then the order snakes left.
    // Near the toilet annex the lower row is skipped and progression continues
    // from the safe upper row. Later groups use the regular 5x2 grid.
    public static Vector3 PlotPos(int i)
    {
        if (i < 10)
        {
            switch (i)
            {
                case 0: return new Vector3(-8f, 0f, 12f);  // first: old Golden Crab plot
                case 1: return new Vector3(-8f, 0f, 1f);   // second: directly below
                case 2: return new Vector3(-19f, 0f, 1f);  // then left
                case 3: return new Vector3(-19f, 0f, 12f); // and up
                case 4: return new Vector3(-30f, 0f, 12f);
                case 5: return new Vector3(-30f, 0f, 1f);
                case 6: return new Vector3(-41f, 0f, 12f); // skip toilet's lower row
                case 7: return new Vector3(-52f, 0f, 12f);
                case 8: return new Vector3(-41f, 0f, 23f);
                default: return new Vector3(-52f, 0f, 23f);
            }
        }

        int band = i / 10;
        int k = i % 10;
        int col = k / 2;
        int row = k % 2;                       // 0 = top, 1 = bottom
        float x = ColX0 - col * Grid;
        float z = RowZ0 + band * (2f * Grid) + (row == 0 ? Grid : 0f);
        return new Vector3(x, 0f, z);
    }

    public static int ZoneOf(int sp) { return sp / 20; }

    // expansion zone z-slabs (zone z = 2 bands, x -58..-2)
    public static float ZoneZ0(int z) { return z == 0 ? -4f : 6f + 44f * z; }
    public static float ZoneZ1(int z) { return 50f + 44f * z; }

    static readonly Rect SeaRect = new Rect(26f, -60f, 114f, 150f);

    System.Collections.IEnumerator Start()
    {
        instance = this;
        Game.world = transform;

        // MENU phase vs GAME phase: the "Game" scene (or SkipMenuOnce) builds the
        // world; the "Menu" scene (or default) builds only the lightweight menu.
        bool gamePhase = GameManager.SkipMenuOnce || SceneManager.GetActiveScene().name == "Game";
        if (!gamePhase)
        {
            BuildMenuScene();
            yield break;
        }
        GameManager.SkipMenuOnce = false;
        Time.timeScale = 1f;

        gameObject.AddComponent<GameManager>();
        gameObject.AddComponent<DevCheats>();
        GameManager gm = Game.gm;
        LoadingScreen.Report(0.23f, "Kayit ve sirket bilgileri okunuyor...");
        yield return null;

        // brand-new save: shop starts CLOSED at 05:00 with a messy floor
        if (gm.freshStart)
        {
            gm.shopOpen = false;
            gm.clockMinutes = 5f * 60f;
        }

        BuildGround();
        LoadingScreen.Report(0.26f, "Zemin hazirlaniyor...");
        yield return null;
        BuildShop();
        LoadingScreen.Report(0.30f, "Dukkan kuruluyor...");
        yield return null;
        BuildGateAndArch();
        BuildDockAndRamp();
        LoadingScreen.Report(0.34f, "Giris ve iskele kuruluyor...");
        yield return null;
        BuildDisplayAquarium();
        SeaDecor();
        BuildFoodProps();
        LoadingScreen.Report(0.38f, "Dukkan ve sahil kuruluyor...");
        yield return null;

        CameraRig rig = CameraRig.Create();
        BuildSun();
        Sfx.Init(gameObject, false);
        UIManager.Create();
        PCScreen.Create();
        LoadingScreen.Report(0.49f, "Kamera ve arayuz hazirlaniyor...");
        yield return null;

        Sea.Create(SeaRect, transform);
        CashRegister.Create(new Vector3(1f, 0f, 20f), transform);
        ManagerDesk.Create(new Vector3(0f, 0f, 7f), transform);
        TrashSystem.Create(new Vector3(11.5f, 0f, 18.5f), transform);
        gameObject.AddComponent<CustomerManager>();
        EventManager.Create(transform);
        BeachManager.Create(transform);
        LoadingScreen.Report(0.57f, "Deniz canlilari ve olaylar hazirlaniyor...");
        yield return null;

        // toilets sit in the far BOTTOM-LEFT corner; the AREA is bought on the spot
        Vector3 toiletPos = new Vector3(-49f, 0f, 2f);
        if (gm.toiletAreaOpen)
            Toilets.Create(toiletPos, transform);
        else
        {
            GameObject toiletLock = new GameObject("LockedToiletArea");
            toiletLock.transform.SetParent(transform, false);
            toiletLock.transform.position = toiletPos;
            Toilets.BuildAreaShell(toiletLock.transform, true);

            // Purchase pad stays just outside the locked room's east wall.
            BuyZone.CreateGeneric("TUVALET ALANI (Sv " + GameManager.ToiletAreaLevel + ")", GameManager.ToiletAreaCost, new Vector3(-36f, 0f, 2f), new Color(0.4f, 0.75f, 0.95f),
                delegate () { return Game.gm.Level >= GameManager.ToiletAreaLevel; },
                delegate
                {
                    Game.gm.toiletAreaOpen = true;
                    Destroy(toiletLock);
                    Toilets.Create(toiletPos, Game.world);
                    Game.ui.Toast("Tuvalet alani acildi! PC'den klozet ve lavabo ekleyebilirsin.");
                });
            // A small fixture preview sits inside the locked room.
            Material white = MatLib.Get(new Color(0.9f, 0.92f, 0.95f));
            Material wall = MatLib.Get(new Color(0.55f, 0.75f, 0.85f));
            B.Prim(PrimitiveType.Cube, "PrevBack", toiletLock.transform, new Vector3(3f, 1.1f, 1f), Vector3.zero, new Vector3(1.6f, 2.2f, 0.14f), wall);
            B.Prim(PrimitiveType.Cube, "PrevTank", toiletLock.transform, new Vector3(3f, 0.9f, 0.7f), Vector3.zero, new Vector3(0.6f, 0.55f, 0.28f), white);
            B.Prim(PrimitiveType.Cylinder, "PrevBowl", toiletLock.transform, new Vector3(3f, 0.4f, 0.3f), Vector3.zero, new Vector3(0.55f, 0.32f, 0.55f), white);
        }

        BuildStarterBackArea();
        LoadingScreen.Report(0.64f, "Dukkan bolumleri kontrol ediliyor...");
        yield return null;

        for (int i = 0; i < SpeciesInfo.Count; i++)
        {
            if (gm.IsUnlocked(i))
            {
                Tank t = Tank.Create(i, PlotPos(i), transform);
                t.AddSaved(gm.LoadTankCount(i));
            }
            else BuyZone.CreatePlot(i, PlotPos(i));
            if (i % 8 == 7)
            {
                LoadingScreen.Report(0.64f + 0.18f * (i + 1f) / SpeciesInfo.Count, "Akvaryumlar yerlestiriliyor... " + (i + 1) + "/" + SpeciesInfo.Count);
                yield return null;
            }
        }
        BuildZones();

        if (gm.LoadHasDepot())
        {
            Depot.Create(new Vector3(14f, 0f, 4f), transform);
            Game.depot.LoadSaved();
        }
        else
        {
            BuyZone.CreateGeneric("DEPO", GameManager.DepotCost, new Vector3(14f, 0f, 4f), new Color(0.9f, 0.7f, 0.3f),
                delegate () { return Game.gm.Level >= 3; },
                delegate
                {
                    Depot.Create(new Vector3(14f, 0f, 4f), Game.world);
                    Game.ui.Toast("Depo acildi! Her turu buraya birakabilirsin.");
                });
        }

        for (int role = 0; role < StaffInfo.RoleCount; role++)
            for (int n = 0; n < gm.staffCounts[role]; n++)
                Staff.Create(role, Customer.DoorPos + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f)));
        LoadingScreen.Report(0.87f, "Personel ise geliyor...");
        yield return null;

        for (int i = 0; i < DecorInfo.Count; i++)
            if (gm.decorOwned[i]) ApplyDecor(i);
        ApplyPaint();

        int shown = Mathf.Min(12, gm.unlockedCount);
        for (int i = gm.unlockedCount - shown; i < gm.unlockedCount; i++) AddDisplayFish(i);
        LoadingScreen.Report(0.93f, "Baliklar akvaryumlara birakiliyor...");
        yield return null;

        // player spawns right next to the first aquarium
        PlayerController.Create(new Vector3(-1.5f, 0.2f, 13f));
        rig.target = Game.player.transform;

        UpdateGateBarrier();

        int offline = gm.ComputeOfflineEarnings();
        if (offline > 0)
        {
            gm.AddMoney(offline);
            Game.ui.Toast("Sen yokken personel calisti: +$" + B.Money(offline));
        }
        Game.ui.OnMoneyChanged();
        Game.ui.RefreshLevel();
        ApplyShopName();
        Game.ui.CloseTransient(); // item 14: no stray panels on game entry
        // species already stocked count as discovered (VERITABANI)
        for (int i = 0; i < Game.tanks.Count; i++)
            if (Game.tanks[i].Count > 0) gm.MarkDiscovered(Game.tanks[i].species);
        QuestSystem.GenerateDaily();
        LoadingScreen.Report(0.98f, "Son kontroller yapiliyor...");
        yield return null;

        // tutorial: starting mess + welcome message
        if (gm.freshStart)
        {
            gm.freshStart = false;
            for (int i = 0; i < 5; i++)
                Game.trash.SpawnLandTrash(new Vector3(Random.Range(-20f, 2f), 0f, Random.Range(4f, 18f)), true);
            Game.ui.ShowInfo("DUKKANINA HOS GELDIN!",
                "Saat daha 05:00 ve dukkanin su an KAPALI.\n" +
                "Once yerdeki copleri toplayip disaridaki kutuya at,\n" +
                "sonra hemen denize kos ve radarla balik yakala!\n" +
                "Hazir olunca kapidaki tabeladan dukkani ac.");
        }
        LoadingScreen.Report(1f, "Hazir! Akvaryum kasabana hos geldin.");
        yield return null;
        LoadingScreen.Hide();
        if (Game.ui != null) Game.ui.ScheduleLevel5QuakeTutorial();
    }

    public static void ApplyShopName()
    {
        if (gateNameText != null && Game.gm != null && !string.IsNullOrEmpty(Game.gm.shopName))
            gateNameText.text = Game.gm.shopName.ToUpper() + "\nDUKKAN GIRISI";
    }

    // ---------- lightweight menu scene ----------
    void BuildMenuScene()
    {
        Sfx.Init(gameObject, true);
        Material seaMat = AssetLib.WaterMaterial();
        if (seaMat == null) seaMat = MatLib.Glass(new Color(0.3f, 0.7f, 0.95f, 0.85f));
        B.Prim(PrimitiveType.Plane, "MenuSea", transform, Vector3.zero, Vector3.zero, new Vector3(30f, 1f, 30f), seaMat);
        B.Prim(PrimitiveType.Cube, "MenuBed", transform, new Vector3(0f, -2.5f, 0f), Vector3.zero, new Vector3(300f, 0.1f, 300f), MatLib.Get(new Color(0.15f, 0.4f, 0.6f)));
        Bounds swim = new Bounds(new Vector3(0f, -0.7f, 0f), new Vector3(56f, 0.5f, 56f));
        for (int i = 0; i < 14; i++)
        {
            Fish f = Fish.Create(Random.Range(0, 24), new Vector3(Random.Range(-25f, 25f), -0.7f, Random.Range(-25f, 25f)), 1.5f);
            f.SetWild(swim, true);
        }
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.backgroundColor = new Color(0.55f, 0.8f, 0.95f);
        cam.fieldOfView = 50f;
        camGo.AddComponent<AudioListener>();
        camGo.transform.position = new Vector3(0f, 11f, -16f);
        camGo.transform.LookAt(new Vector3(0f, -1f, 0f));
        camGo.AddComponent<MenuCamDrift>();
        BuildSun();
        UIManager.Create();
        Game.ui.ShowMainMenu();
    }

    // ---------- expansion zones (locked gray areas with price labels) ----------
    void BuildStarterBackArea()
    {
        if (Game.gm.starterBackAreaOpen) return;

        const float z0 = 29.5f;
        const float z1 = 49.5f;
        float zc = (z0 + z1) * 0.5f;
        GameObject overlay = new GameObject("StarterBackAreaLock");
        overlay.transform.SetParent(transform, false);

        Material gray = MatLib.Glass(new Color(0.22f, 0.24f, 0.28f, 0.58f));
        Material barrier = MatLib.Get(new Color(0.32f, 0.38f, 0.45f));
        B.Prim(PrimitiveType.Cube, "LockedFloor", overlay.transform, new Vector3(-25f, 0.055f, zc), Vector3.zero,
            new Vector3(66f, 0.055f, z1 - z0), gray);
        B.Prim(PrimitiveType.Cube, "Divider", overlay.transform, new Vector3(-25f, 0.8f, z0), Vector3.zero,
            new Vector3(66f, 1.6f, 0.65f), barrier, true);
        B.Text3D("KILITLI EK BOLUM", overlay.transform, new Vector3(-25f, 2.6f, zc), 0.16f, Color.white);
        B.Text3D("$" + B.Money(GameManager.StarterBackAreaCost) + "   (Sv " + GameManager.StarterBackAreaLevel + ")",
            overlay.transform, new Vector3(-25f, 1.6f, zc), 0.14f, new Color(1f, 0.9f, 0.3f));

        BuyZone.CreateGeneric("EK BOLUM (Sv " + GameManager.StarterBackAreaLevel + ")", GameManager.StarterBackAreaCost,
            new Vector3(-25f, 0f, 26f), new Color(0.4f, 0.8f, 0.65f),
            delegate () { return Game.gm.Level >= GameManager.StarterBackAreaLevel; },
            delegate
            {
                Game.gm.starterBackAreaOpen = true;
                Destroy(overlay);
                if (Game.ui != null) Game.ui.Toast("Dukkanin ek bolumu acildi!");
            });
    }

    void BuildZones()
    {
        for (int b = 1; b < 4; b++) // zones 1,2,3 (zone 0 free); each = 2 bands
        {
            if (Game.gm.ZoneOpen(b)) continue;
            int zone = b;
            float z0 = ZoneZ0(b), z1 = ZoneZ1(b);
            float zc = (z0 + z1) * 0.5f;
            int req = 20 * b - 2;          // level gate: 18 / 38 / 58
            int cost = Game.gm.ZoneCost(b);

            GameObject ov = new GameObject("ZoneLock" + b);
            ov.transform.SetParent(transform, false);
            Material gray = MatLib.Glass(new Color(0.25f, 0.25f, 0.28f, 0.55f));
            B.Prim(PrimitiveType.Cube, "Plate", ov.transform, new Vector3(-30f, 0.05f, zc), Vector3.zero,
                new Vector3(56f, 0.06f, z1 - z0 - 0.4f), gray);
            Material post = MatLib.Get(new Color(0.45f, 0.45f, 0.5f));
            for (int px = 0; px < 5; px++)
                B.Prim(PrimitiveType.Cylinder, "Post", ov.transform, new Vector3(-56f + px * 13f, 0.7f, z0 + 0.3f), Vector3.zero, new Vector3(0.25f, 0.7f, 0.25f), post);
            // clear price + level label right on the locked area
            B.Text3D("KILITLI ALAN", ov.transform, new Vector3(-30f, 2.4f, zc), 0.16f, new Color(0.95f, 0.95f, 1f));
            B.Text3D("$" + B.Money(cost) + "   (Sv " + req + ")", ov.transform, new Vector3(-30f, 1.5f, zc), 0.15f, new Color(1f, 0.9f, 0.35f));

            GameObject overlay = ov;
            BuyZone.CreateGeneric("YENI ALAN", cost, new Vector3(-6f, 0f, z0 + 2.5f), new Color(0.95f, 0.8f, 0.25f),
                delegate () { return Game.gm.ZoneOpen(zone - 1) && Game.gm.Level >= req; },
                delegate
                {
                    Game.gm.zoneOpen[zone] = true;
                    Destroy(overlay);
                    if (instance != null) instance.RebuildPerimeter();
                    Game.ui.Toast("Yeni alan acildi! Duvarlar genisletildi.");
                });
        }
    }

    // ---------- static hooks ----------
    public static void OnSpeciesUnlocked(int sp)
    {
        if (Game.TankOf(sp) == null && instance != null)
            Tank.Create(sp, PlotPos(sp), instance.transform);
        AddDisplayFish(sp);
        if (Game.toilets != null) Game.toilets.OnLevelChanged();
        if (Game.pc != null && Game.pc.IsOpen) Game.pc.RefreshAll();
    }

    static void AddDisplayFish(int sp)
    {
        if (instance == null) return;
        Fish f = Fish.Create(sp, displayBounds.center + new Vector3(Random.Range(-10f, 10f), 0f, 0f), 0.8f);
        f.SetWild(displayBounds, true);
        displayFish.Add(f);
        while (displayFish.Count > 12)
        {
            Fish old = displayFish[0];
            displayFish.RemoveAt(0);
            if (old != null) Destroy(old.gameObject);
        }
    }

    public static bool NearGateSign(Vector3 pos)
    {
        pos.y = 0f;
        return Vector3.Distance(pos, GateSignPos) < 2.5f;
    }

    public static void UpdateGateBarrier()
    {
        if (instance == null) return;
        bool closed = !Game.gm.shopOpen;
        if (closed && gateBarrier == null)
        {
            gateBarrier = new GameObject("GateBarrier");
            gateBarrier.transform.SetParent(instance.transform, false);
            Material red = MatLib.Get(new Color(0.85f, 0.2f, 0.2f));
            B.Prim(PrimitiveType.Cube, "Bar1", gateBarrier.transform, new Vector3(8f, 1f, 26f), new Vector3(0f, 90f, 15f), new Vector3(8f, 0.25f, 0.25f), red);
            B.Prim(PrimitiveType.Cube, "Bar2", gateBarrier.transform, new Vector3(8f, 1.6f, 26f), new Vector3(0f, 90f, -15f), new Vector3(8f, 0.25f, 0.25f), red);
            B.Text3D("KAPALI", gateBarrier.transform, new Vector3(8f, 2.6f, 26f), 0.12f, new Color(1f, 0.4f, 0.35f));
        }
        else if (!closed && gateBarrier != null)
        {
            Destroy(gateBarrier);
            gateBarrier = null;
        }
    }

    public static void ApplyPaint()
    {
        MatLib.Floor().color = MatLib.FloorStyles[Mathf.Clamp(Game.gm.floorStyle, 0, MatLib.FloorStyles.Length - 1)];
        MatLib.Wall().color = MatLib.WallStyles[Mathf.Clamp(Game.gm.wallStyle, 0, MatLib.WallStyles.Length - 1)];
    }

    public static void PreviewPaint(int floorStyle, int wallStyle)
    {
        MatLib.Floor().color = MatLib.FloorStyles[Mathf.Clamp(floorStyle, 0, MatLib.FloorStyles.Length - 1)];
        MatLib.Wall().color = MatLib.WallStyles[Mathf.Clamp(wallStyle, 0, MatLib.WallStyles.Length - 1)];
    }

    public static void ApplyDecor(int idx)
    {
        if (instance == null) return;
        Transform t = instance.transform;
        switch (idx)
        {
            case 0: // palms
                Palm(t, new Vector3(14f, 0f, 33f)); Palm(t, new Vector3(14f, 0f, 19f)); Palm(t, new Vector3(12f, 0f, 5f));
                break;
            case 1: // fountain on the grass walkway
                Material stone = MatLib.Get(new Color(0.75f, 0.78f, 0.82f));
                B.Prim(PrimitiveType.Cylinder, "FountainPool", t, new Vector3(14f, 0.3f, 36f), Vector3.zero, new Vector3(4f, 0.3f, 4f), stone, true);
                B.Prim(PrimitiveType.Cylinder, "FountainWaterD", t, new Vector3(14f, 0.62f, 36f), Vector3.zero, new Vector3(3.5f, 0.05f, 3.5f), MatLib.Glass(new Color(0.4f, 0.75f, 1f, 0.6f)));
                B.Prim(PrimitiveType.Cylinder, "FountainCol", t, new Vector3(14f, 1.2f, 36f), Vector3.zero, new Vector3(0.5f, 0.9f, 0.5f), stone);
                B.Prim(PrimitiveType.Sphere, "FountainTop", t, new Vector3(14f, 2.4f, 36f), Vector3.zero, Vector3.one * 0.8f, MatLib.Glass(new Color(0.5f, 0.8f, 1f, 0.7f))).AddComponent<Bobber>();
                break;
            case 2: // balloon arch over the gate
                for (int i = 0; i < 9; i++)
                {
                    float a = Mathf.PI * i / 8f;
                    Vector3 p = new Vector3(8f, 2.5f + Mathf.Sin(a) * 2.2f, 26f + Mathf.Cos(a) * 4.5f);
                    Color c = Color.HSVToRGB(i / 9f, 0.6f, 1f);
                    B.Prim(PrimitiveType.Sphere, "Balloon", t, p, Vector3.zero, Vector3.one * 0.7f, MatLib.Get(c)).AddComponent<Bobber>().amp = 0.06f;
                }
                break;
            case 3: // golden fish statue near the entrance
                Material gold = MatLib.Get(new Color(0.95f, 0.8f, 0.3f));
                B.Prim(PrimitiveType.Cylinder, "Pedestal", t, new Vector3(5f, 0.6f, 32f), Vector3.zero, new Vector3(1.8f, 0.6f, 1.8f), MatLib.Get(new Color(0.7f, 0.72f, 0.78f)), true);
                GameObject statue = new GameObject("Statue");
                statue.transform.SetParent(t, false);
                statue.transform.position = new Vector3(5f, 2.1f, 32f);
                Transform model = SpeciesInfo.Build(0, statue.transform, 1.6f);
                model.localEulerAngles = new Vector3(-30f, 0f, 0f);
                foreach (MeshRenderer mr in statue.GetComponentsInChildren<MeshRenderer>()) mr.sharedMaterial = gold;
                statue.AddComponent<Spinner>().speed = 30f;
                break;
            case 4: // red carpet from the gate
                B.Prim(PrimitiveType.Cube, "Carpet", t, new Vector3(3.5f, 0.03f, 26f), Vector3.zero, new Vector3(9f, 0.03f, 3f), MatLib.Get(new Color(0.8f, 0.15f, 0.15f)));
                break;
            case 6: // diving boards + jump ramp on the dock
                Material board = MatLib.Get(new Color(0.85f, 0.95f, 1f));
                B.Prim(PrimitiveType.Cube, "Board1", t, new Vector3(30.6f, 1.1f, 31.2f), Vector3.zero, new Vector3(2.4f, 0.12f, 1f), board, true);
                B.Prim(PrimitiveType.Cube, "Board2", t, new Vector3(30.6f, 1.1f, 32.8f), Vector3.zero, new Vector3(2.4f, 0.12f, 1f), board, true);
                RampZone.Create(new Vector3(30.8f, 1f, 32f), new Vector3(38f, 0f, 32f), t);
                break;
            case 7: // jetski parked by the dock
                Jetski.Create(new Vector3(30f, 0.55f, 22f), t);
                break;
            case 5: // lamp posts
                Vector3[] lamps = { new Vector3(5.5f, 0f, 5f), new Vector3(5.5f, 0f, 32f), new Vector3(-57.6f, 0f, 5f), new Vector3(-57.6f, 0f, 30f) };
                Material dark = MatLib.Get(new Color(0.2f, 0.22f, 0.28f));
                for (int i = 0; i < lamps.Length; i++)
                {
                    B.Prim(PrimitiveType.Cylinder, "LampPost", t, lamps[i] + Vector3.up * 1.6f, Vector3.zero, new Vector3(0.18f, 1.6f, 0.18f), dark);
                    B.Prim(PrimitiveType.Sphere, "LampBulb", t, lamps[i] + Vector3.up * 3.4f, Vector3.zero, Vector3.one * 0.7f, MatLib.Get(new Color(1f, 0.95f, 0.6f)));
                }
                break;
            case 8: // iskele
                Material wood = MatLib.Get(new Color(0.7f, 0.5f, 0.3f));
                Material woodD = MatLib.Get(new Color(0.55f, 0.4f, 0.25f));
                B.Prim(PrimitiveType.Cube, "Dock", t, new Vector3(25f, 0.9f, 32f), Vector3.zero, new Vector3(10f, 0.3f, 3.4f), wood, true);
                for (int i = 0; i < 4; i++)
                    B.Prim(PrimitiveType.Cube, "DockLeg", t, new Vector3(21f + i * 2.6f, 0.4f, 32f + ((i % 2) * 2 - 1) * 1.4f), Vector3.zero, new Vector3(0.3f, 1f, 0.3f), woodD);
                for (int i = 0; i < 3; i++)
                    B.Prim(PrimitiveType.Cube, "DockStep", t, new Vector3(19.6f - i * 0.7f, 0.65f - i * 0.25f, 32f), Vector3.zero, new Vector3(0.7f, 0.22f, 3f), woodD, true);
                break;
        }
        Sfx.Play(Snd.Drop, 0.3f);
    }

    static void Palm(Transform t, Vector3 pos)
    {
        Material trunk = MatLib.Get(new Color(0.55f, 0.4f, 0.25f));
        Material leaf = MatLib.Get(new Color(0.3f, 0.7f, 0.35f));
        B.Prim(PrimitiveType.Cylinder, "Trunk", t, pos + Vector3.up * 1.4f, new Vector3(0f, 0f, 6f), new Vector3(0.35f, 1.4f, 0.35f), trunk);
        for (int i = 0; i < 5; i++)
        {
            float a = i * 72f;
            B.Prim(PrimitiveType.Cube, "Leaf", t, pos + new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * 1f, 2.9f, Mathf.Cos(a * Mathf.Deg2Rad) * 1f),
                new Vector3(20f, a, 0f), new Vector3(0.5f, 0.08f, 2f), leaf);
        }
    }

    // ---------- environment ----------
    void BuildGround()
    {
        B.Prim(PrimitiveType.Plane, "Ground", transform, new Vector3(27f, -0.02f, 22f), Vector3.zero, new Vector3(25f, 1f, 20f), MatLib.Beach(), true);

        // shop floor (paintable) spans the full expandable area: x -58..8, z -4..135
        B.Prim(PrimitiveType.Cube, "ShopFloor", transform, new Vector3(-25f, 0.01f, (MapTop - 4f) * 0.5f), Vector3.zero, new Vector3(66f, 0.02f, MapTop + 4f), MatLib.Floor());
        B.Prim(PrimitiveType.Cube, "GrassStrip", transform, new Vector3(14f, 0.012f, 40f), Vector3.zero, new Vector3(12f, 0.02f, 200f), MatLib.Grass());
        // Soft lime-to-sand shoreline like a friendly mobile aquarium game.
        B.Prim(PrimitiveType.Cube, "ShoreLightGrass", transform, new Vector3(20.2f, 0.015f, 40f), Vector3.zero,
            new Vector3(2.4f, 0.025f, 200f), MatLib.Get(new Color(0.68f, 0.94f, 0.45f)));
        B.Prim(PrimitiveType.Cube, "ShorePaleSand", transform, new Vector3(23.3f, 0.018f, 40f), Vector3.zero,
            new Vector3(4.2f, 0.03f, 200f), MatLib.Get(new Color(1f, 0.96f, 0.68f)));
        BuildCuteShoreDetails();
        // DEEP water: transparent surface above, dark seabed far below, the
        // character sinks into it while swimming (real depth feel)
        B.Prim(PrimitiveType.Plane, "SeaSurf", transform, new Vector3(SeaRect.center.x, 0.55f, SeaRect.center.y), Vector3.zero,
            new Vector3(SeaRect.width / 10f, 1f, SeaRect.height / 10f), CuteWater());
        // bright turquoise seabed keeps the whole sea CUTE, not dark
        B.Prim(PrimitiveType.Cube, "SeaBed", transform, new Vector3(SeaRect.center.x, -1.6f, SeaRect.center.y), Vector3.zero,
            new Vector3(SeaRect.width, 0.05f, SeaRect.height), MatLib.Get(new Color(0.55f, 0.9f, 1f)));
        // shoreline: water edge skirt + underwater slope
        B.Prim(PrimitiveType.Cube, "ShoreSkirt", transform, new Vector3(SeaRect.xMin, 0.27f, SeaRect.center.y), Vector3.zero,
            new Vector3(0.15f, 0.56f, SeaRect.height), MatLib.Glass(new Color(0.4f, 0.78f, 0.98f, 0.6f)));
        B.Prim(PrimitiveType.Cube, "ShoreSlope", transform, new Vector3(SeaRect.xMin + 2.5f, -1.1f, SeaRect.center.y), new Vector3(0f, 0f, 35f),
            new Vector3(6f, 0.1f, SeaRect.height), MatLib.Get(new Color(0.85f, 0.8f, 0.6f)));


        MakeWall(new Vector3(27f, 2f, MapTop + 6f), new Vector3(260f, 4f, 1f));
        MakeWall(new Vector3(27f, 2f, -66f), new Vector3(260f, 4f, 1f));
        MakeWall(new Vector3(-62f, 2f, 60f), new Vector3(1f, 4f, 260f));
        MakeWall(new Vector3(142f, 2f, 60f), new Vector3(1f, 4f, 260f));
    }

    // light, cheerful water: copy the asset material and brighten its colors
    static Material CuteWater()
    {
        Material src = AssetLib.WaterMaterial();
        if (src == null) return MatLib.Glass(new Color(0.42f, 0.82f, 1f, 0.75f));
        return new Material(src); // preserve Water_mat_04's authored blue palette
    }

    void BuildCuteShoreDetails()
    {
        Material stone = MatLib.Get(new Color(0.55f, 0.85f, 0.43f));
        Material shell = MatLib.Get(new Color(0.55f, 0.8f, 1f));
        Material star = MatLib.Get(new Color(0.93f, 0.55f, 0.68f));
        for (float z = SeaRect.yMin + 5f; z < SeaRect.yMax; z += 13f)
        {
            for (int i = 0; i < 3; i++)
                B.Prim(PrimitiveType.Sphere, "ShoreStone", transform,
                    new Vector3(Random.Range(18.7f, 21f), 0.07f, z + Random.Range(-2.5f, 2.5f)), Vector3.zero,
                    new Vector3(Random.Range(0.5f, 1.15f), 0.09f, Random.Range(0.45f, 0.9f)), stone);
            B.Prim(PrimitiveType.Sphere, "Shell", transform,
                new Vector3(Random.Range(21.5f, 24.5f), 0.08f, z + 3.5f), Vector3.zero,
                new Vector3(0.32f, 0.08f, 0.24f), shell);
            GameObject s = new GameObject("Starfish");
            s.transform.SetParent(transform, false);
            s.transform.position = new Vector3(Random.Range(21.5f, 24.5f), 0.07f, z - 3f);
            for (int arm = 0; arm < 5; arm++)
                B.Prim(PrimitiveType.Cube, "Arm", s.transform, Vector3.forward * 0.22f,
                    new Vector3(0f, arm * 72f, 0f), new Vector3(0.1f, 0.04f, 0.42f), star);
        }
    }

    void MakeWall(Vector3 pos, Vector3 size)
    {
        GameObject w = new GameObject("Wall");
        w.transform.SetParent(transform, false);
        w.transform.position = pos;
        BoxCollider bc = w.AddComponent<BoxCollider>();
        bc.size = size;
    }

    void BuildShop()
    {
        RebuildPerimeter();

        // rope queue barriers inside the gate
        Material post = MatLib.Get(new Color(0.55f, 0.65f, 0.8f));
        Material rope = MatLib.Get(new Color(0.85f, 0.2f, 0.2f));
        for (int i = 0; i < 5; i++)
        {
            Vector3 p = new Vector3(4f - i * 2f, 0f, 22.5f);
            B.Prim(PrimitiveType.Cylinder, "Post", transform, p + Vector3.up * 0.6f, Vector3.zero, new Vector3(0.18f, 0.6f, 0.18f), post);
            B.Prim(PrimitiveType.Sphere, "PostTop", transform, p + Vector3.up * 1.25f, Vector3.zero, Vector3.one * 0.28f, post);
            if (i < 4)
                B.Prim(PrimitiveType.Cube, "Rope", transform, p + new Vector3(-1f, 1f, 0f), Vector3.zero, new Vector3(1.8f, 0.08f, 0.08f), rope);
        }
    }

    // Walls hug EXACTLY the currently owned rectangle; locked gray zones lie
    // beyond the north wall and become part of the shop when bought.
    GameObject wallsGo;
    public void RebuildPerimeter()
    {
        if (wallsGo != null) Destroy(wallsGo);
        wallsGo = new GameObject("ShopWalls");
        wallsGo.transform.SetParent(transform, false);
        Material wall = MatLib.Wall();
        int maxOpen = 0;
        for (int b = 0; b < 5; b++) { if (Game.gm.ZoneOpen(b)) maxOpen = b; else break; }
        float zN = ZoneZ1(maxOpen);
        float zS = -4f;
        float mid = (zS + zN) * 0.5f;

        B.Prim(PrimitiveType.Cube, "WallS", wallsGo.transform, new Vector3(-25f, 0.8f, zS), Vector3.zero, new Vector3(66f, 1.6f, 1f), wall, true);
        B.Prim(PrimitiveType.Cube, "WallW", wallsGo.transform, new Vector3(-58f, 0.8f, mid), Vector3.zero, new Vector3(1f, 1.6f, zN - zS), wall, true);
        B.Prim(PrimitiveType.Cube, "WallN", wallsGo.transform, new Vector3(-25f, 0.8f, zN), Vector3.zero, new Vector3(66f, 1.6f, 1f), wall, true);
        // east wall: openings for the exit arch (z 8-16) and customer gate (z 22-30)
        // Leave a clean half-metre seat around each arch post. The first segment
        // now reaches the south wall, removing the stray gap at the bottom.
        B.Prim(PrimitiveType.Cube, "WallE1", wallsGo.transform, new Vector3(8f, 0.8f, 1.75f), Vector3.zero, new Vector3(1f, 1.6f, 11.5f), wall, true);
        B.Prim(PrimitiveType.Cube, "WallE2", wallsGo.transform, new Vector3(8f, 0.8f, 19f), Vector3.zero, new Vector3(1f, 1.6f, 5f), wall, true);
        B.Prim(PrimitiveType.Cube, "WallE3", wallsGo.transform, new Vector3(8f, 0.8f, (30.5f + zN) * 0.5f), Vector3.zero, new Vector3(1f, 1.6f, zN - 30.5f), wall, true);
    }

    void BuildGateAndArch()
    {
        Material blueD = MatLib.Get(new Color(0.25f, 0.6f, 0.8f));
        B.Prim(PrimitiveType.Cube, "ArchL", transform, new Vector3(8f, 1.6f, 8f), Vector3.zero, new Vector3(0.9f, 3.2f, 0.9f), blueD);
        B.Prim(PrimitiveType.Cube, "ArchR", transform, new Vector3(8f, 1.6f, 16f), Vector3.zero, new Vector3(0.9f, 3.2f, 0.9f), blueD);
        B.Prim(PrimitiveType.Cube, "ArchTop", transform, new Vector3(8f, 3.4f, 12f), Vector3.zero, new Vector3(0.9f, 0.8f, 9.5f), blueD);
        B.Text3D("DENIZ YOLU", transform, new Vector3(8f, 4.3f, 12f), 0.1f, Color.white);

        Material gate = MatLib.Get(new Color(0.9f, 0.75f, 0.3f));
        B.Prim(PrimitiveType.Cube, "GateL", transform, new Vector3(8f, 1.8f, 22f), Vector3.zero, new Vector3(1f, 3.6f, 1f), gate);
        B.Prim(PrimitiveType.Cube, "GateR", transform, new Vector3(8f, 1.8f, 30f), Vector3.zero, new Vector3(1f, 3.6f, 1f), gate);
        B.Prim(PrimitiveType.Cube, "GateTop", transform, new Vector3(8f, 3.9f, 26f), Vector3.zero, new Vector3(1f, 1f, 9.5f), gate);
        gateNameText = B.Text3D("DUKKAN GIRISI", transform, new Vector3(8f, 5f, 26f), 0.11f, new Color(1f, 0.95f, 0.5f));
        Material fence = MatLib.Get(new Color(0.8f, 0.8f, 0.85f));
        B.Prim(PrimitiveType.Cube, "PathFence1", transform, new Vector3(13f, 0.5f, 22.5f), Vector3.zero, new Vector3(9f, 1f, 0.25f), fence);
        B.Prim(PrimitiveType.Cube, "PathFence2", transform, new Vector3(13f, 0.5f, 29.5f), Vector3.zero, new Vector3(9f, 1f, 0.25f), fence);
        B.Prim(PrimitiveType.Cube, "SignPost", transform, GateSignPos + Vector3.up * 1f, Vector3.zero, new Vector3(0.15f, 1f, 0.15f), fence);
        B.Prim(PrimitiveType.Cube, "SignBoard", transform, GateSignPos + Vector3.up * 2.2f, Vector3.zero, new Vector3(1.6f, 0.9f, 0.12f), MatLib.Get(new Color(0.35f, 0.25f, 0.18f)));
        B.Text3D("AC / KAPAT", transform, GateSignPos + new Vector3(0f, 2.2f, -0.12f), 0.07f, Color.white);
    }

    void BuildDockAndRamp()
    {
        // Dock (Iskele) is now a decor item (index 8).
        // Ramp (Ziplama Rampasi) is decor item (index 6).
    }

    void BuildDisplayAquarium()
    {
        Material glass = MatLib.Glass(new Color(0.45f, 0.8f, 0.95f, 0.45f));
        Material bed = MatLib.Get(new Color(0.5f, 0.8f, 0.55f));
        Material blueD = MatLib.Get(new Color(0.25f, 0.6f, 0.8f));
        Material plant = MatLib.Get(new Color(0.3f, 0.65f, 0.4f));
        Material coral = MatLib.Get(new Color(0.9f, 0.55f, 0.45f));
        Material pearl = MatLib.Get(new Color(0.95f, 0.95f, 1f));

        float dz = MapTop - 2.5f; // top back of the shop
        B.Prim(PrimitiveType.Cube, "DispBed", transform, new Vector3(-25f, 0.5f, dz), Vector3.zero, new Vector3(60f, 1f, 5f), bed);
        B.Prim(PrimitiveType.Cube, "DispGlass", transform, new Vector3(-25f, 2.1f, dz - 2.6f), Vector3.zero, new Vector3(60f, 3f, 0.4f), glass, true);
        for (int i = 0; i < 6; i++)
        {
            float x = -52f + i * 11f;
            B.Prim(PrimitiveType.Cube, "DispArchL", transform, new Vector3(x, 1.8f, dz - 2.3f), Vector3.zero, new Vector3(0.6f, 3.6f, 0.6f), blueD);
            B.Prim(PrimitiveType.Cube, "DispArchTop", transform, new Vector3(x + 5.5f, 3.8f, dz - 2.3f), new Vector3(0f, 0f, 8f), new Vector3(12f, 0.5f, 0.6f), blueD);
        }
        for (int i = 0; i < 12; i++)
        {
            float x = -53f + i * 4.8f;
            B.Prim(PrimitiveType.Capsule, "DispPlant", transform, new Vector3(x, 1.6f, dz + Random.Range(-1f, 1f)), Vector3.zero, new Vector3(0.25f, 0.7f, 0.25f), plant);
            if (i % 2 == 0)
                B.Prim(PrimitiveType.Cube, "DispCoral", transform, new Vector3(x + 2f, 1.3f, dz), new Vector3(0f, 25f, 0f), new Vector3(0.4f, 0.8f, 0.4f), coral);
        }
        B.Prim(PrimitiveType.Sphere, "DispPearl", transform, new Vector3(-25f, 1.4f, dz + 0.5f), Vector3.zero, Vector3.one * 0.8f, pearl).AddComponent<Bobber>();
        B.Text3D("VITRIN AKVARYUMU", transform, new Vector3(-25f, 5f, dz - 1f), 0.14f, new Color(0.7f, 0.95f, 1f));
    }

    // snack tables built from the Casual Food pack
    void BuildFoodProps()
    {
        MakeFoodTable(new Vector3(5f, 0f, 14f), 3);   // by the register
        MakeFoodTable(new Vector3(22.5f, 0f, 14f), 2); // beach picnic
    }

    void MakeFoodTable(Vector3 pos, int foodCount)
    {
        Material wood = MatLib.Get(new Color(0.72f, 0.52f, 0.32f));
        Material woodD = MatLib.Get(new Color(0.55f, 0.4f, 0.25f));
        B.Prim(PrimitiveType.Cube, "TableTop", transform, pos + Vector3.up * 0.85f, Vector3.zero, new Vector3(2.6f, 0.12f, 1.4f), wood, true);
        B.Prim(PrimitiveType.Cube, "LegA", transform, pos + new Vector3(-1.1f, 0.4f, 0f), Vector3.zero, new Vector3(0.15f, 0.8f, 1.2f), woodD);
        B.Prim(PrimitiveType.Cube, "LegB", transform, pos + new Vector3(1.1f, 0.4f, 0f), Vector3.zero, new Vector3(0.15f, 0.8f, 1.2f), woodD);
        for (int i = 0; i < foodCount; i++)
        {
            GameObject holder = new GameObject("FoodProp");
            holder.transform.SetParent(transform, false);
            holder.transform.position = pos + new Vector3(-0.8f + i * 0.8f, 0.95f, Random.Range(-0.3f, 0.3f));
            holder.transform.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
            if (AssetLib.SpawnFood(holder.transform, 1.1f) == null)
                B.Prim(PrimitiveType.Sphere, "Snack", holder.transform, Vector3.up * 0.15f, Vector3.zero, Vector3.one * 0.35f, MatLib.Get(new Color(1f, 0.6f, 0.3f)));
        }
    }

    void SeaDecor()
    {
        Material weed = MatLib.Get(new Color(0.3f, 0.7f, 0.5f));
        Material star = MatLib.Get(new Color(0.55f, 0.6f, 0.9f));
        Material rock = MatLib.Get(new Color(0.55f, 0.58f, 0.62f));
        for (int i = 0; i < 30; i++)
        {
            Vector3 p = new Vector3(Random.Range(SeaRect.xMin + 3f, SeaRect.xMax - 3f), 0.1f, Random.Range(SeaRect.yMin + 3f, SeaRect.yMax - 3f));
            if (i % 3 == 0)
                B.Prim(PrimitiveType.Capsule, "Weed", transform, p + Vector3.up * 0.4f, new Vector3(0f, 0f, Random.Range(-12f, 12f)), new Vector3(0.18f, 0.5f, 0.18f), weed);
            else if (i % 3 == 1)
                B.Prim(PrimitiveType.Sphere, "Star", transform, p, Vector3.zero, new Vector3(0.5f, 0.12f, 0.5f), star);
            else
                B.Prim(PrimitiveType.Sphere, "Rock", transform, p, Vector3.zero, new Vector3(Random.Range(0.8f, 1.6f), 0.5f, Random.Range(0.8f, 1.4f)), rock);
        }
    }

    void BuildSun()
    {
        GameObject lightGo = new GameObject("Sun");
        sun = lightGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.15f;
        sun.color = new Color(1f, 0.97f, 0.9f);
        sun.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
        RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.7f);
        if (GameAssets.SkyMaterial != null)
        {
            skyMaterial = new Material(GameAssets.SkyMaterial);
            RenderSettings.skybox = skyMaterial;
        }
    }

    void Update()
    {
        if (sun != null && Game.gm != null)
        {
            float hour = Game.gm.clockMinutes / 60f;
            float daylight;
            if (hour >= 7f && hour < 20f) daylight = 1f;
            else if (hour >= 20f && hour < 21f) daylight = Mathf.Lerp(1f, 0.18f, hour - 20f);
            else if (hour >= 5f && hour < 7f) daylight = Mathf.Lerp(0.18f, 1f, (hour - 5f) / 2f);
            else daylight = 0.18f;
            float targetIntensity = Mathf.Lerp(0.2f, 1.15f, daylight);
            sun.intensity = Mathf.Lerp(sun.intensity, targetIntensity, Time.deltaTime * 0.5f);
            sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(15f, 65f, daylight), -35f + hour * 3f, 0f);
            Color targetAmb = Color.Lerp(new Color(0.12f, 0.16f, 0.3f), new Color(0.6f, 0.65f, 0.7f), daylight);
            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, targetAmb, Time.deltaTime * 0.5f);
            if (skyMaterial != null)
            {
                if (skyMaterial.HasProperty("_Exposure")) skyMaterial.SetFloat("_Exposure", Mathf.Lerp(0.28f, 1.08f, daylight));
                if (skyMaterial.HasProperty("_Tint")) skyMaterial.SetColor("_Tint", Color.Lerp(new Color(0.16f, 0.2f, 0.42f, 1f), Color.white, daylight));
            }
        }
    }
}

// Slow orbit for the menu backdrop camera.
public class MenuCamDrift : MonoBehaviour
{
    void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.up, 2f * Time.deltaTime);
        transform.LookAt(new Vector3(0f, -1f, 0f));
    }
}

// Persistent aquarium-themed loading overlay. Progress is driven by real scene
// loading and world-construction milestones rather than a fake timer.
public class LoadingScreen : MonoBehaviour
{
    static LoadingScreen current;
    Text percentText, statusText, activityText;
    RectTransform fill, shine;
    Transform fish;
    readonly List<RectTransform> bubbles = new List<RectTransform>();
    float progress;
    float startedAt;

    public static void BeginGameLoad()
    {
        if (current != null) return;
        GameObject go = new GameObject("AquariumLoadingScreen");
        DontDestroyOnLoad(go);
        current = go.AddComponent<LoadingScreen>();
        current.Build();
        current.StartCoroutine(GameBootstrap.LoadGameRoutine());
    }

    void Build()
    {
        startedAt = Time.unscaledTime;
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(4000f, 4000f), new Color(0.08f, 0.52f, 0.78f), false, false);
        GameObject glow = UIKit.Icon(transform, UIKit.Circle(), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f),
            new Vector2(760f, 760f), new Color(0.22f, 0.8f, 0.95f, 0.28f));

        GameObject titleArea = UIKit.Panel(transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -90f),
            new Vector2(900f, 90f), new Color(0f, 0f, 0f, 0.001f), false, false);
        UIKit.Label(titleArea.transform, "AQUARIUM TOWN", 46, Color.white, TextAnchor.MiddleCenter, true);

        GameObject fishGo = UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f),
            new Vector2(260f, 150f), new Color(0f, 0f, 0f, 0.001f), false, false);
        fish = fishGo.transform;
        UIKit.Icon(fish, UIKit.Circle(), new Vector2(0.5f, 0.5f), new Vector2(18f, 0f), new Vector2(150f, 88f), UIKit.Orange);
        UIKit.Icon(fish, UIKit.Star(), new Vector2(0.5f, 0.5f), new Vector2(-78f, 0f), new Vector2(72f, 72f), new Color(1f, 0.48f, 0.15f));
        UIKit.Icon(fish, UIKit.Circle(), new Vector2(0.5f, 0.5f), new Vector2(60f, 13f), new Vector2(17f, 17f), Color.white);
        UIKit.Icon(fish, UIKit.Circle(), new Vector2(0.5f, 0.5f), new Vector2(62f, 13f), new Vector2(7f, 7f), UIKit.BlueDark);

        for (int i = 0; i < 9; i++)
        {
            GameObject bubble = UIKit.Icon(transform, UIKit.Circle(), new Vector2(0.5f, 0.5f),
                new Vector2(-430f + i * 105f, -80f + (i % 3) * 55f), new Vector2(18f + (i % 3) * 8f, 18f + (i % 3) * 8f),
                new Color(0.78f, 0.96f, 1f, 0.55f));
            bubbles.Add(bubble.GetComponent<RectTransform>());
        }

        GameObject statusArea = UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -105f),
            new Vector2(900f, 55f), new Color(0f, 0f, 0f, 0.001f), false, false);
        statusText = UIKit.Label(statusArea.transform, "Hazirlaniyor...", 23, Color.white, TextAnchor.MiddleCenter, true);
        GameObject track = UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -175f),
            new Vector2(780f, 42f), new Color(0.08f, 0.28f, 0.48f, 0.9f), true, true);
        GameObject fillGo = UIKit.Panel(track.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f),
            new Vector2(1f, 28f), UIKit.Green, true, false);
        fill = fillGo.GetComponent<RectTransform>();
        GameObject shineGo = UIKit.Panel(track.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f),
            new Vector2(70f, 24f), new Color(0.85f, 1f, 0.72f, 0.8f), true, false);
        shine = shineGo.GetComponent<RectTransform>();
        GameObject percentArea = UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -230f),
            new Vector2(300f, 58f), new Color(0f, 0f, 0f, 0.001f), false, false);
        percentText = UIKit.Label(percentArea.transform, "%0", 30, Color.white, TextAnchor.MiddleCenter, true);
        GameObject activityArea = UIKit.Panel(transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -280f),
            new Vector2(760f, 45f), new Color(0f, 0f, 0f, 0.001f), false, false);
        activityText = UIKit.Label(activityArea.transform, "", 18, new Color(0.82f, 0.96f, 1f), TextAnchor.MiddleCenter);
        Report(0f, "Hazirlaniyor...");
    }

    public static void Report(float value, string status)
    {
        if (current == null) return;
        current.progress = Mathf.Clamp01(value);
        if (current.fill != null) current.fill.sizeDelta = new Vector2(764f * current.progress, 28f);
        if (current.percentText != null) current.percentText.text = "%" + Mathf.RoundToInt(current.progress * 100f);
        if (current.statusText != null) current.statusText.text = status;
    }

    public static void Hide()
    {
        if (current == null) return;
        Destroy(current.gameObject);
        current = null;
    }

    void Update()
    {
        float t = Time.unscaledTime;
        if (activityText != null)
        {
            string[] spinner = { "●○○", "○●○", "○○●", "○●○" };
            int frame = Mathf.FloorToInt(t * 5f) % spinner.Length;
            activityText.text = spinner[frame] + "  YUKLENIYOR  •  " + (t - startedAt).ToString("0.0") + " sn  •  OYUN CALISIYOR";
        }
        if (shine != null)
        {
            float x = 10f + Mathf.Repeat(t * 260f, 690f);
            shine.anchoredPosition = new Vector2(x, 0f);
        }
        if (fish != null)
        {
            Vector2 p = fish.GetComponent<RectTransform>().anchoredPosition;
            p.y = 80f + Mathf.Sin(t * 2.2f) * 12f;
            p.x = Mathf.Sin(t * 0.8f) * 25f;
            fish.GetComponent<RectTransform>().anchoredPosition = p;
        }
        for (int i = 0; i < bubbles.Count; i++)
        {
            RectTransform b = bubbles[i];
            if (b == null) continue;
            Vector2 p = b.anchoredPosition;
            p.y += Time.unscaledDeltaTime * (22f + i * 3f);
            p.x += Mathf.Sin(t * 1.7f + i) * Time.unscaledDeltaTime * 8f;
            if (p.y > 250f) p.y = -160f;
            b.anchoredPosition = p;
        }
    }
}

// Waits a frame so destroyed objects are fully gone, then rebuilds the game.
public class RestartRunner : MonoBehaviour
{
    int frames;
    void Update()
    {
        if (++frames >= 2)
        {
            GameBootstrap.RebuildNow();
            Destroy(gameObject);
        }
    }
}

// Rideable jetski (bought on the PC): hop on while swimming for a big speed boost,
// it parks itself when you climb back onto land.
public class Jetski : MonoBehaviour
{
    bool mounted;
    bool broken;
    Vector3 homePosition;
    TextMesh statusText;
    GameObject hull;

    public bool Broken { get { return broken; } }
    public int RepairCost { get { return Mathf.RoundToInt(DecorInfo.Costs[7] * 0.9f); } }

    public static Jetski Create(Vector3 pos, Transform parent)
    {
        GameObject go = new GameObject("Jetski");
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.position = pos;
        Jetski j = go.AddComponent<Jetski>();
        Game.jetski = j;
        j.homePosition = pos;
        Material bodyM = MatLib.Get(new Color(0.95f, 0.3f, 0.2f));
        Material white = MatLib.Get(Color.white);
        Material dark = MatLib.Get(new Color(0.15f, 0.15f, 0.2f));
        j.hull = B.Prim(PrimitiveType.Capsule, "Hull", go.transform, new Vector3(0f, 0.25f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.8f, 1.1f, 0.5f), bodyM);
        B.Prim(PrimitiveType.Cube, "Seat", go.transform, new Vector3(0f, 0.55f, -0.35f), Vector3.zero, new Vector3(0.5f, 0.25f, 0.9f), white);
        B.Prim(PrimitiveType.Cube, "Stem", go.transform, new Vector3(0f, 0.6f, 0.55f), new Vector3(-20f, 0f, 0f), new Vector3(0.1f, 0.5f, 0.1f), dark);
        B.Prim(PrimitiveType.Cube, "Handle", go.transform, new Vector3(0f, 0.85f, 0.62f), Vector3.zero, new Vector3(0.65f, 0.08f, 0.08f), dark);
        go.AddComponent<Bobber>().amp = 0.05f;
        j.statusText = B.Text3D("", go.transform, new Vector3(0f, 1.6f, 0f), 0.08f, Color.white);
        j.ApplyLevelVisual();
        return j;
    }

    public void ApplyLevelVisual()
    {
        int level = Game.gm != null ? Game.gm.jetskiLevel : 1;
        if (hull != null)
        {
            Renderer r = hull.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = MatLib.Get(Color.HSVToRGB(Mathf.Lerp(0.02f, 0.55f, (level - 1) / 4f), 0.72f, 0.95f));
        }
        if (statusText != null) statusText.text = broken ? "KIRIK!  $" + B.Money(RepairCost) : "JETSKI  Sv" + level;
    }

    public bool PlayerNear(Vector3 playerPos) { return Vector3.Distance(playerPos, transform.position) < 3.2f; }

    public void BreakFromShark()
    {
        mounted = false;
        broken = true;
        if (Game.player != null && Game.player.jetski == this) Game.player.jetski = null;
        transform.SetParent(Game.world, true);
        transform.position = homePosition;
        transform.rotation = Quaternion.Euler(0f, 0f, 72f);
        Bobber bb = GetComponent<Bobber>();
        if (bb == null) gameObject.AddComponent<Bobber>().amp = 0.05f;
        ApplyLevelVisual();
        if (Game.ui != null) Game.ui.Toast("Jetski kirildi! Iskelenin yaninda $" + B.Money(RepairCost) + " odeyerek tamir et.", 6f);
    }

    public bool TryRepair()
    {
        if (!broken) return false;
        if (!Game.gm.TrySpend(RepairCost))
        {
            if (Game.ui != null) Game.ui.Toast("Jetski tamiri icin yeterli paran yok!");
            return false;
        }
        broken = false;
        transform.position = homePosition;
        transform.rotation = Quaternion.identity;
        ApplyLevelVisual();
        Sfx.Play(Snd.Repair, 0.9f);
        if (Game.ui != null) Game.ui.Toast("Jetski tamir edildi!");
        return true;
    }

    void Update()
    {
        if (Game.player == null) return;
        if (broken) return;
        if (!mounted)
        {
            if (Game.player.Swimming && Game.player.jetski == null &&
                Vector3.Distance(Game.player.transform.position, transform.position) < 2.2f)
            {
                mounted = true;
                Game.player.jetski = this;
                Bobber bb = GetComponent<Bobber>();
                if (bb != null) Destroy(bb);
                transform.SetParent(Game.player.visual, false);
                transform.localPosition = new Vector3(0f, 0.5f, 0f);
                transform.localRotation = Quaternion.identity;
                Sfx.Play(Snd.Splash, 0.6f);
                if (Game.ui != null) Game.ui.Toast("Jetski! Su ustunde cok hizlisin.");
            }
        }
        else if (!Game.player.Swimming)
        {
            // dismount: park at the water's edge
            mounted = false;
            Game.player.jetski = null;
            transform.SetParent(Game.world, true);
            Vector3 p = Game.player.transform.position + new Vector3(3f, 0f, 0f);
            p.y = 0.55f;
            if (Game.sea != null && !Game.sea.Contains(p))
                p = new Vector3(28f, 0.55f, Mathf.Clamp(p.z, -50f, 80f));
            transform.position = p;
            transform.rotation = Quaternion.identity;
            gameObject.AddComponent<Bobber>().amp = 0.05f;
        }
    }
}

public class RampZone : MonoBehaviour
{
    Vector3 landing;
    float cooldown;
    GameObject rampVisual;

    public static RampZone Create(Vector3 pos, Vector3 landing, Transform parent)
    {
        GameObject go = new GameObject("RampZone");
        if (parent != null) go.transform.SetParent(parent);
        go.transform.position = pos;
        RampZone r = go.AddComponent<RampZone>();
        Game.ramp = r;
        r.landing = landing;
        r.rampVisual = B.Prim(PrimitiveType.Cube, "UpgradeRamp", go.transform, new Vector3(0f, 0.25f, 0f), new Vector3(0f, 0f, -15f), new Vector3(2.3f, 0.3f, 2.8f), MatLib.Get(UIKit.Orange), true);
        r.ApplyLevelVisual();
        return r;
    }

    public void ApplyLevelVisual()
    {
        int level = Game.gm != null ? Game.gm.rampLevel : 1;
        if (rampVisual != null)
        {
            Renderer renderer = rampVisual.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = MatLib.Get(Color.HSVToRGB(Mathf.Lerp(0.08f, 0.82f, (level - 1) / 4f), 0.65f, 1f));
        }
    }

    void Update()
    {
        if (Game.player == null) return;
        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;
        Vector3 p = Game.player.transform.position;
        if (Mathf.Abs(p.x - transform.position.x) < 1.2f && Mathf.Abs(p.z - transform.position.z) < 1.6f)
        {
            cooldown = 2f;
            int level = Game.gm != null ? Game.gm.rampLevel : 1;
            float[] distances = { 8f, 14f, 23f, 36f, 54f };
            Vector3 target = new Vector3(transform.position.x + distances[level - 1], 0f, transform.position.z + Random.Range(-1.2f, 1.2f));
            if (Game.sea != null) target.x = Mathf.Min(target.x, Game.sea.area.xMax - 12f);
            Game.player.LaunchTo(target, 4f + level * 1.4f);
        }
    }
}
