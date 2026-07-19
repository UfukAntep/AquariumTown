using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Loads the user's imported "ithappy" packs (City Characters + Casual Food).
// Falls back to primitive visuals when assets are unavailable.
public static class AssetLib
{
    static bool inited;
    static List<GameObject> characters = new List<GameObject>();
    static List<GameObject> foods = new List<GameObject>();
    static List<GameObject> seaAnimals = new List<GameObject>();
    static List<string> seaAnimalNames = new List<string>();
    static AnimationClip walkClip, idleClip;
    static RuntimeAnimatorController moveController;
    static GameObject cashPrefab;
    static Material waterMaterial;
    static Dictionary<Material, Material> matFix = new Dictionary<Material, Material>();
    static Dictionary<Material, Material> seaMatFix = new Dictionary<Material, Material>();

    public static void Clear()
    {
        inited = false;
        characters.Clear();
        foods.Clear();
        seaAnimals.Clear();
        seaAnimalNames.Clear();
        walkClip = null; idleClip = null; moveController = null;
        cashPrefab = null; waterMaterial = null;
        matFix.Clear();
        seaMatFix.Clear();
    }

    static void Init()
    {
        if (inited) return;
        inited = true;
#if UNITY_EDITOR
        LoadFolder("Assets/ithappy/City_Characters/Prefabs/Characters", characters);
        LoadFolder("Assets/ithappy/Casual_Food/Prefabs", foods, false, new string[] { "cocktail_005" });
        walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/ithappy/City_Characters/Animations/Adult/Adult_Walk.anim");
        idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/ithappy/City_Characters/Animations/Adult/Adult_IdleLookAround.anim");
        moveController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/ithappy/City_Characters/Animations/Animation_Controllers/Character_Movement.controller");

        // Quirky Series water animals: WHITELIST only catchable aquatic species
        // (no sea lions, birds, penguins etc.)
        string[] aquatic = { "Clownfish", "Crab", "Dolphin", "Lobster", "Orca", "SeaHorse", "Seahorse",
            "Arowana", "Carp", "Crocodile", "Frog", "Manatee", "Turtle", "Shark", "Ray", "Salmon",
            "Squid", "Octopus", "Puffer", "Betta", "Koi", "Piranha", "Catfish", "Angelfish" };
        LoadFolderWhitelist("Assets/Quirky Series/Ultimate Pack Vol.1/Mega Pack Vol.2/Sea Vol.1/Prefabs", seaAnimals, aquatic);
        LoadFolderWhitelist("Assets/Quirky Series/Ultimate Pack Vol.1/Mega Pack Vol.2/River Vol.1/Prefabs", seaAnimals, aquatic);

        // PinkTea cash + IgniteCoders water
        cashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PinkTea/3D Cartoon Safe Pack/Prefabs/Cash.prefab");
        waterMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/IgniteCoders/Simple Water Shader/Resources/Water_mat_01.mat");
        if (waterMaterial == null)
            waterMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/IgniteCoders/Simple Water Shader/Resources/Water_mat_02.mat");
#endif
    }

#if UNITY_EDITOR
    static void LoadFolder(string folder, List<GameObject> into, bool skipLOD = false, string[] blacklist = null)
    {
        try
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { folder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (skipLOD && (path.Contains("LOD") || path.Contains("Single LODs"))) continue;
                if (blacklist != null)
                {
                    bool bad = false;
                    for (int b = 0; b < blacklist.Length; b++)
                        if (path.Contains(blacklist[b])) { bad = true; break; }
                    if (bad) continue;
                }
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null) into.Add(go);
            }
        }
        catch { }
    }

    static void LoadFolderWhitelist(string folder, List<GameObject> into, string[] allow)
    {
        try
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { folder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.Contains("LOD")) continue;
                string file = System.IO.Path.GetFileNameWithoutExtension(path);
                bool ok = false;
                for (int a = 0; a < allow.Length; a++)
                    if (file.IndexOf(allow[a], System.StringComparison.OrdinalIgnoreCase) >= 0) { ok = true; break; }
                if (!ok) continue;
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go != null) { into.Add(go); seaAnimalNames.Add(NiceName(file)); }
            }
        }
        catch { }
    }

    // "SeaHorse" -> "Denizati" style friendly Turkish names for the known animals
    static string NiceName(string file)
    {
        string f = file.ToLower();
        if (f.Contains("clownfish")) return "Palyaco Baligi";
        if (f.Contains("seahorse")) return "Denizati";
        if (f.Contains("turtle")) return "Kaplumbaga";
        if (f.Contains("crab")) return "Yengec";
        if (f.Contains("lobster")) return "Istakoz";
        if (f.Contains("dolphin")) return "Yunus";
        if (f.Contains("orca")) return "Katil Balina";
        if (f.Contains("arowana")) return "Arowana";
        if (f.Contains("carp")) return "Sazan";
        if (f.Contains("crocodile")) return "Timsah";
        if (f.Contains("frog")) return "Kurbaga";
        if (f.Contains("manatee")) return "Deniz Ineği";
        if (f.Contains("squid")) return "Kalamar";
        if (f.Contains("octopus")) return "Ahtapot";
        if (f.Contains("puffer")) return "Balon Baligi";
        if (f.Contains("shark")) return "Kopekbaligi";
        if (f.Contains("ray")) return "Vatoz";
        if (f.Contains("salmon")) return "Somon";
        if (f.Contains("koi")) return "Koi";
        if (f.Contains("piranha")) return "Pirana";
        if (f.Contains("catfish")) return "Yayin";
        if (f.Contains("angel")) return "Melek Baligi";
        if (f.Contains("betta")) return "Beta";
        return file;
    }
#endif

    public static Material WaterMaterial() { Init(); return waterMaterial; }
    public static int SeaAnimalCount { get { Init(); return seaAnimals.Count; } }

    // consistent name for the model a species maps to (null if pack unavailable)
    public static string AnimalName(int id)
    {
        Init();
        if (seaAnimalNames.Count == 0) return null;
        return seaAnimalNames[id % seaAnimalNames.Count];
    }

    // Quirky animal by species id (deterministic) with normalized size + swim animation.
    public static GameObject SpawnSeaAnimal(int id, Transform parent, float targetSize)
    {
        Init();
        if (seaAnimals.Count == 0) return null;
        GameObject go = Object.Instantiate(seaAnimals[id % seaAnimals.Count], parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        FixMaterials(go);
        FixSeaAnimalMaterials(go);
        StripColliders(go);
        NormalizeSize(go, targetSize);
        go.AddComponent<QuirkyMotion>();
        return go;
    }

    public static GameObject SpawnRandomSeaAnimal(Transform parent, float targetSize)
    {
        Init();
        if (seaAnimals.Count == 0) return null;
        return SpawnSeaAnimal(Random.Range(0, seaAnimals.Count), parent, targetSize);
    }

    // Cash bill from the safe pack, laid flat and normalized to bill size.
    public static GameObject SpawnMoneyBill(Transform parent)
    {
        Init();
        if (cashPrefab == null) return null;
        GameObject go = Object.Instantiate(cashPrefab, parent, false);
        FixMaterials(go);
        StripColliders(go);
        NormalizeSize(go, 0.95f);
        return go;
    }

    static void NormalizeSize(GameObject go, float target)
    {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return;
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        float maxDim = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
        if (maxDim > 0.001f)
            go.transform.localScale = go.transform.localScale * (target / maxDim);
    }

    public static bool HasCharacters { get { Init(); return characters.Count > 0; } }

    // Spawns a character under parent. seed >= 0 gives a deterministic pick.
    public static GameObject SpawnCharacter(Transform parent, int seed = -1)
    {
        Init();
        if (characters.Count == 0) return null;
        int idx = seed >= 0 ? seed % characters.Count : Random.Range(0, characters.Count);
        GameObject go = Object.Instantiate(characters[idx], parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        FixMaterials(go);
        StripColliders(go);
        CharMotion cm = go.AddComponent<CharMotion>();
        cm.Setup(idleClip, walkClip, moveController);
        return go;
    }

    public static GameObject SpawnFood(Transform parent, float scale = 1f)
    {
        Init();
        if (foods.Count == 0) return null;
        GameObject go = Object.Instantiate(foods[Random.Range(0, foods.Count)], parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one * scale;
        FixMaterials(go);
        StripColliders(go);
        return go;
    }

    // Built-in Standard materials render pink in URP -> clone into URP Lit.
    public static void FixMaterials(GameObject go)
    {
        Shader urp = Shader.Find("Universal Render Pipeline/Lit");
        if (urp == null) return;
        Renderer[] rends = go.GetComponentsInChildren<Renderer>(true);
        for (int r = 0; r < rends.Length; r++)
        {
            Material[] mats = rends[r].sharedMaterials;
            bool changed = false;
            for (int m = 0; m < mats.Length; m++)
            {
                Material src = mats[m];
                if (src == null || src.shader == null) continue;
                string sn = src.shader.name;
                // anything that isn't URP-native renders pink -> rebuild as URP Lit,
                // carrying the texture (_MainTex/_BaseMap) and color over
                bool urpOk = sn.Contains("Universal") || sn.Contains("URP") || sn.Contains("Shader Graphs");
                if (urpOk) continue;
                Material fixedMat;
                if (!matFix.TryGetValue(src, out fixedMat) || fixedMat == null)
                {
                    fixedMat = new Material(urp);
                    Texture tex = null;
                    if (src.HasProperty("_MainTex")) tex = src.GetTexture("_MainTex");
                    if (tex == null && src.HasProperty("_BaseMap")) tex = src.GetTexture("_BaseMap");
                    if (tex == null && src.HasProperty("_BaseColorMap")) tex = src.GetTexture("_BaseColorMap");
                    if (tex != null) fixedMat.mainTexture = tex;
                    if (src.HasProperty("_Color")) fixedMat.color = src.color;
                    else if (src.HasProperty("_BaseColor")) fixedMat.color = src.GetColor("_BaseColor");
                    fixedMat.SetFloat("_Smoothness", 0.2f);
                    matFix[src] = fixedMat;
                }
                mats[m] = fixedMat;
                changed = true;
            }
            if (changed) rends[r].sharedMaterials = mats;
        }
    }

    // Some pack shaders cast shadows but fail to draw their surface in URP.
    // Sea animals use a predictable opaque URP/Lit material with white tint so
    // their texture colours stay bright both in the lake and in aquariums.
    static void FixSeaAnimalMaterials(GameObject go)
    {
        Shader urp = Shader.Find("Universal Render Pipeline/Lit");
        if (urp == null) return;
        Renderer[] rends = go.GetComponentsInChildren<Renderer>(true);
        for (int r = 0; r < rends.Length; r++)
        {
            Material[] mats = rends[r].sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material src = mats[i];
                if (src == null) continue;
                Material fixedMat;
                if (!seaMatFix.TryGetValue(src, out fixedMat) || fixedMat == null)
                {
                    fixedMat = new Material(urp);
                    Texture tex = null;
                    if (src.HasProperty("_BaseMap")) tex = src.GetTexture("_BaseMap");
                    if (tex == null && src.HasProperty("_MainTex")) tex = src.GetTexture("_MainTex");
                    if (tex == null && src.HasProperty("_BaseColorMap")) tex = src.GetTexture("_BaseColorMap");
                    if (fixedMat.HasProperty("_BaseMap")) fixedMat.SetTexture("_BaseMap", tex);
                    fixedMat.SetColor("_BaseColor", Color.white);
                    fixedMat.SetFloat("_Surface", 0f);
                    fixedMat.SetFloat("_Smoothness", 0.25f);
                    fixedMat.SetFloat("_ZWrite", 1f);
                    fixedMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                    seaMatFix[src] = fixedMat;
                }
                mats[i] = fixedMat;
            }
            rends[r].sharedMaterials = mats;
            rends[r].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            rends[r].receiveShadows = true;
        }
    }

    static void StripColliders(GameObject go)
    {
        Collider[] cols = go.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++) Object.Destroy(cols[i]);
    }
}

// Quirky Series animals: play their built-in swim/walk animation state on loop.
public class QuirkyMotion : MonoBehaviour
{
    void Start()
    {
        Animator anim = GetComponentInChildren<Animator>();
        if (anim == null || anim.runtimeAnimatorController == null) return;
        string[] states = { "Swim", "Fly", "Walk", "Move", "Idle_A", "Idle" };
        try
        {
            for (int i = 0; i < states.Length; i++)
            {
                int hash = Animator.StringToHash(states[i]);
                if (anim.HasState(0, hash))
                {
                    anim.Play(hash, 0, Random.value);
                    anim.speed = Random.Range(0.85f, 1.15f);
                    break;
                }
            }
        }
        catch { }
    }
}

// Blends idle/walk clips based on how fast the object is actually moving.
// Falls back to a light procedural bob when clips are unavailable.
public class CharMotion : MonoBehaviour
{
    PlayableGraph graph;
    AnimationMixerPlayable mixer;
    bool hasGraph;
    Animator anim;
    bool hasController, hasVertParam;
    Vector3 lastPos;
    float smoothSpeed;
    float bobPhase;

    public void Setup(AnimationClip idle, AnimationClip walk, RuntimeAnimatorController controller)
    {
        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = gameObject.AddComponent<Animator>();

        // preferred: the pack's own movement controller ("Vert" blends idle->walk->run)
        if (controller != null)
        {
            try
            {
                anim.runtimeAnimatorController = controller;
                hasController = true;
                for (int i = 0; i < anim.parameters.Length; i++)
                    if (anim.parameters[i].name == "Vert") { hasVertParam = true; break; }
                return;
            }
            catch { hasController = false; }
        }

        if (idle == null && walk == null) return;
        try
        {
            graph = PlayableGraph.Create("CharMotion");
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "out", anim);
            mixer = AnimationMixerPlayable.Create(graph, 2);
            if (idle != null)
            {
                AnimationClipPlayable p = AnimationClipPlayable.Create(graph, idle);
                graph.Connect(p, 0, mixer, 0);
            }
            if (walk != null)
            {
                AnimationClipPlayable p = AnimationClipPlayable.Create(graph, walk);
                graph.Connect(p, 0, mixer, 1);
            }
            mixer.SetInputWeight(0, 1f);
            output.SetSourcePlayable(mixer);
            graph.Play();
            hasGraph = true;
        }
        catch { hasGraph = false; }
    }

    void Start() { lastPos = transform.position; bobPhase = Random.value * 6f; }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        Vector3 delta = transform.position - lastPos;
        delta.y = 0f;
        lastPos = transform.position;
        smoothSpeed = Mathf.Lerp(smoothSpeed, delta.magnitude / dt, 8f * dt);
        float w = Mathf.Clamp01(smoothSpeed / 2.8f);

        if (hasController)
        {
            if (hasVertParam) anim.SetFloat("Vert", Mathf.Clamp01(smoothSpeed / 6f));
        }
        else if (hasGraph)
        {
            mixer.SetInputWeight(0, 1f - w);
            mixer.SetInputWeight(1, w);
        }
        else
        {
            float bob = w * Mathf.Abs(Mathf.Sin(Time.time * 9f + bobPhase)) * 0.08f;
            Vector3 lp = transform.localPosition;
            lp.y = bob;
            transform.localPosition = lp;
        }
    }

    void OnDestroy()
    {
        if (hasGraph && graph.IsValid()) graph.Destroy();
    }
}
