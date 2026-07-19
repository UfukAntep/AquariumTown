using UnityEngine;

// A Resources-hosted reference catalog keeps selected Asset Store content in
// player builds even though the original packs are outside Resources folders.
public class RuntimeAssetCatalog : ScriptableObject
{
    public AudioClip calmMusic;
    public AudioClip menuMusic;
    public AudioClip dangerMusic;
    public AudioClip[] sounds;
    public Sprite[] tabIcons;
    public Sprite[] decorIcons;
    public Sprite[] itemIcons;
    public GameObject[] fishPrefabs;
    public Material skyMaterial;
    public GameObject arrowPrefab;
}

public static class GameAssets
{
    static RuntimeAssetCatalog catalog;
    public static RuntimeAssetCatalog Catalog
    {
        get
        {
            if (catalog == null) catalog = Resources.Load<RuntimeAssetCatalog>("RuntimeAssetCatalog");
            return catalog;
        }
    }

    public static AudioClip Sound(Snd sound)
    {
        RuntimeAssetCatalog c = Catalog;
        int i = (int)sound;
        return c != null && c.sounds != null && i >= 0 && i < c.sounds.Length ? c.sounds[i] : null;
    }

    public static Sprite TabIcon(int i)
    {
        RuntimeAssetCatalog c = Catalog;
        return c != null && c.tabIcons != null && i >= 0 && i < c.tabIcons.Length ? c.tabIcons[i] : null;
    }

    public static Sprite DecorIcon(int i)
    {
        RuntimeAssetCatalog c = Catalog;
        return c != null && c.decorIcons != null && i >= 0 && i < c.decorIcons.Length ? c.decorIcons[i] : null;
    }

    public static Sprite ItemIcon(int i)
    {
        RuntimeAssetCatalog c = Catalog;
        return c != null && c.itemIcons != null && i >= 0 && i < c.itemIcons.Length ? c.itemIcons[i] : null;
    }

    public static GameObject FishPrefab(int species)
    {
        RuntimeAssetCatalog c = Catalog;
        return c != null && c.fishPrefabs != null && c.fishPrefabs.Length > 0
            ? c.fishPrefabs[Mathf.Abs(species) % c.fishPrefabs.Length] : null;
    }

    public static Sprite FishPortrait(int species) { return FishPortraitCache.Get(species); }
    public static Material SkyMaterial { get { return Catalog != null ? Catalog.skyMaterial : null; } }

    public static void Clear() { FishPortraitCache.Clear(); catalog = null; }
}

// Generates a cached UI portrait from the very same 3D prefab used by the fish.
// This avoids generic coloured dots and remains build-safe through the catalog.
static class FishPortraitCache
{
    static readonly System.Collections.Generic.Dictionary<int, Sprite> portraits =
        new System.Collections.Generic.Dictionary<int, Sprite>();

    public static Sprite Get(int species)
    {
        Sprite cached;
        if (portraits.TryGetValue(species, out cached)) return cached;

        const int portraitLayer = 30;
        GameObject stage = new GameObject("FishPortraitStage");
        GameObject model = AssetLib.SpawnSeaAnimal(species, stage.transform, 1.7f);
        if (model == null)
        {
            Transform fallback = SpeciesInfo.Build(species, stage.transform, 1.1f);
            model = fallback.gameObject;
        }
        SetLayer(model.transform, portraitLayer);
        model.transform.rotation = Quaternion.Euler(12f, 145f, 0f);

        Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        }

        GameObject cameraGo = new GameObject("FishPortraitCamera");
        Camera camera = cameraGo.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        camera.cullingMask = 1 << portraitLayer;
        camera.orthographic = true;
        camera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z) * 1.35f;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 20f;
        camera.transform.position = bounds.center + new Vector3(3f, 1.6f, -4f);
        camera.transform.LookAt(bounds.center);

        GameObject lightGo = new GameObject("FishPortraitLight");
        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.35f;
        light.cullingMask = 1 << portraitLayer;
        light.transform.rotation = Quaternion.Euler(35f, -35f, 0f);

        RenderTexture rt = new RenderTexture(256, 192, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 2;
        camera.targetTexture = rt;
        RenderTexture previous = RenderTexture.active;
        camera.Render();
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(256, 192, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, 256, 192), 0, 0);
        texture.Apply();
        texture.name = "FishPortrait_" + species;
        texture.filterMode = FilterMode.Bilinear;
        RenderTexture.active = previous;
        camera.targetTexture = null;
        rt.Release();

        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(cameraGo);
        Object.DestroyImmediate(lightGo);
        Object.DestroyImmediate(stage);

        Sprite portrait = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        portrait.name = "FishPortrait_" + species;
        portraits[species] = portrait;
        return portrait;
    }

    static void SetLayer(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++) SetLayer(root.GetChild(i), layer);
    }

    public static void Clear()
    {
        foreach (Sprite portrait in portraits.Values)
        {
            if (portrait == null) continue;
            Texture2D texture = portrait.texture;
            Object.Destroy(portrait);
            if (texture != null) Object.Destroy(texture);
        }
        portraits.Clear();
    }
}
