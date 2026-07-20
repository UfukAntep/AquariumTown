using System.Collections.Generic;
using UnityEngine;

// Huge open sea. Wildlife spawns in a window around the player's level:
// recently unlocked species + a few locked previews (with level badges).
public class Sea : MonoBehaviour
{
    public Rect area;

    List<Fish> fishes = new List<Fish>();
    float respawnTimer;
    const int MaxPopulation = 180; // 20% denser sea population

    // At low levels only a few species are eligible. A fixed per-species cap
    // used to stop the sea at roughly 25 fish even though the global target is
    // 180. Share the target across the currently available species instead.
    int SpeciesCap(int sp)
    {
        int availableSpecies = Mathf.Max(1, MaxSpecies + 1);
        return Mathf.CeilToInt(MaxPopulation / (float)availableSpecies) + (sp % 5 == 0 ? 1 : 0);
    }

    public static Sea Create(Rect area, Transform parent)
    {
        GameObject go = new GameObject("Sea");
        if (parent != null) go.transform.SetParent(parent, false);
        Sea s = go.AddComponent<Sea>();
        s.area = area;
        Game.sea = s;
        // life EVERYWHERE: every species lives in the sea from the start,
        // rare (high level) ones live farther from the shore
        for (int i = 0; i < 132; i++) s.SpawnOne();
        return s;
    }

    // sea only contains species up to (player level + 5)
    int MaxSpecies { get { return Mathf.Min(SpeciesInfo.Count - 1, Game.gm.unlockedCount + 4); } }

    // Position by species LEVEL: low-level fish hug the shore, high-level fish
    // roam farther out. The band is wide so it's random, with a small chance of
    // an out-of-place fish.
    Rect BandFor(int sp)
    {
        float t = Mathf.Clamp01(sp / (float)Mathf.Max(1, MaxSpecies));
        // 15% chance to appear anywhere (rare exceptions)
        if (Random.value < 0.15f)
            return new Rect(area.xMin + 2f, area.yMin + 2f, area.width - 4f, area.height - 4f);
        float centerX = Mathf.Lerp(area.xMin + 6f, area.xMax - 8f, t);
        float half = area.width * 0.22f;
        float x0 = Mathf.Max(area.xMin + 2f, centerX - half);
        float x1 = Mathf.Min(area.xMax - 2f, centerX + half);
        return new Rect(x0, area.yMin + 2f, x1 - x0, area.height - 4f);
    }

    Bounds SwimBounds(Rect r)
    {
        // Keep wildlife just under the surface. Deep swimmers were only
        // readable as black silhouettes through the water material.
        return new Bounds(
            new Vector3(r.x + r.width * 0.5f, 0.41f, r.y + r.height * 0.5f),
            new Vector3(r.width, 0.14f, r.height));
    }

    void SpawnOne()
    {
        // species only up to (level + 5); respect the per-species cap
        int[] counts = new int[SpeciesInfo.Count];
        for (int i = 0; i < fishes.Count; i++) counts[fishes[i].species]++;
        int max = MaxSpecies;
        for (int attempt = 0; attempt < 20; attempt++)
        {
            int sp = Random.Range(0, max + 1);
            if (counts[sp] < SpeciesCap(sp)) { Spawn(sp); return; }
        }
    }

    void Spawn(int sp)
    {
        Rect band = BandFor(sp);
        Vector3 pos = new Vector3(Random.Range(band.xMin, band.xMax), 0.41f, Random.Range(band.yMin, band.yMax));
        Fish f = Fish.Create(sp, pos);
        f.SetWild(SwimBounds(band));
        fishes.Add(f);
    }

    public Fish SpawnGolden()
    {
        Vector3 pos = new Vector3(area.xMin + 8f, 0.43f, Random.Range(area.yMin + 10f, area.yMax - 10f));
        Fish f = Fish.Create(Mathf.Max(0, Game.gm.unlockedCount - 1), pos);
        f.SetWild(SwimBounds(new Rect(area.xMin, area.yMin, 40f, area.height)));
        f.MakeGolden();
        fishes.Add(f);
        Destroy(f.gameObject, 25f); // disappears if not caught
        return f;
    }

    // after midnight (00:00 - 05:00) the sea turns deadly: only sharks, they attack
    public bool SharkNight { get { return Game.gm != null && Game.gm.clockMinutes < 5f * 60f; } }

    bool nightHidden;
    float sharkTimer;

    void Update()
    {
        if (Game.gm == null) return; // scene restarting
        fishes.RemoveAll(f => f == null);

        if (SharkNight)
        {
            // hide all normal fish once
            if (!nightHidden)
            {
                nightHidden = true;
                for (int i = 0; i < fishes.Count; i++)
                    if (fishes[i] != null && !fishes[i].golden) fishes[i].gameObject.SetActive(false);
            }
            // spawn hunting sharks while the player is in the water
            if (Game.player != null && Game.player.Swimming)
            {
                sharkTimer -= Time.deltaTime;
                if (sharkTimer <= 0f)
                {
                    sharkTimer = 5f;
                    int existing = FindObjectsByType<Shark>(FindObjectsSortMode.None).Length;
                    if (existing < 3) Shark.Spawn(Game.player.transform.position, false);
                }
            }
            return;
        }

        // daytime: restore hidden fish
        if (nightHidden)
        {
            nightHidden = false;
            for (int i = 0; i < fishes.Count; i++)
                if (fishes[i] != null) fishes[i].gameObject.SetActive(true);
        }

        respawnTimer -= Time.deltaTime;
        if (respawnTimer <= 0f)
        {
            respawnTimer = 0.9f;
            if (fishes.Count < MaxPopulation) SpawnOne();
        }
    }

    // Compass / advanced navigation target: nearest (or highest-value) catchable fish.
    public Fish CompassTarget(Vector3 pos, bool best)
    {
        Fish result = null;
        float bd = float.MaxValue;
        int bs = -1;
        for (int i = 0; i < fishes.Count; i++)
        {
            Fish f = fishes[i];
            if (f == null || f.state != Fish.State.Wild || f.decorative) continue;
            if (!f.golden && !Game.gm.IsUnlocked(f.species)) continue;
            float d = Vector3.Distance(pos, f.transform.position);
            if (best)
            {
                int score = f.golden ? 999 : f.species;
                if (score > bs || (score == bs && d < bd)) { bs = score; bd = d; result = f; }
            }
            else if (d < bd) { bd = d; result = f; }
        }
        return result;
    }

    public bool Contains(Vector3 pos)
    {
        return pos.x > area.xMin && pos.x < area.xMax && pos.z > area.yMin && pos.z < area.yMax;
    }

    // Nearest wild fish. Prefers catchable (unlocked) fish; if only locked ones
    // are in range, returns the nearest locked one so the radar can show the
    // required level.
    public Fish FindTarget(Vector3 pos, float range)
    {
        Fish best = null, bestLocked = null;
        float bestD = range, bestLockedD = range;
        for (int i = 0; i < fishes.Count; i++)
        {
            Fish f = fishes[i];
            if (f == null || f.state != Fish.State.Wild || f.decorative) continue;
            float d = Vector3.Distance(pos, f.transform.position);
            if (f.golden || Game.gm.IsUnlocked(f.species))
            {
                if (d < bestD) { bestD = d; best = f; }
            }
            else if (d < bestLockedD) { bestLockedD = d; bestLocked = f; }
        }
        return best != null ? best : bestLocked;
    }

    public Fish FindGoldenNear(Vector3 pos, float range)
    {
        for (int i = 0; i < fishes.Count; i++)
        {
            Fish fish = fishes[i];
            if (fish != null && fish.golden && fish.state == Fish.State.Wild &&
                Vector3.Distance(pos, fish.transform.position) <= range) return fish;
        }
        return null;
    }

    // Radar aims where the player is FACING: only fish inside the cone count.
    public Fish FindTargetInCone(Vector3 pos, Vector3 aimDir, float range, float halfAngleDeg)
    {
        if (SharkNight) return null; // nothing to catch at night — only sharks!
        aimDir.y = 0f;
        if (aimDir.sqrMagnitude < 0.01f) return null;
        aimDir.Normalize();
        Fish best = null, bestLocked = null;
        float bestD = range, bestLockedD = range;
        for (int i = 0; i < fishes.Count; i++)
        {
            Fish f = fishes[i];
            if (f == null || f.state != Fish.State.Wild || f.decorative) continue;
            Vector3 to = f.transform.position - pos;
            to.y = 0f;
            float d = to.magnitude;
            if (d > range || d < 0.05f) continue;
            if (Vector3.Angle(aimDir, to) > halfAngleDeg) continue;
            if (f.golden || Game.gm.IsUnlocked(f.species))
            {
                if (d < bestD) { bestD = d; best = f; }
            }
            else if (d < bestLockedD) { bestLockedD = d; bestLocked = f; }
        }
        return best != null ? best : bestLocked;
    }

    // any wild fish near a point (pollution kills these)
    public bool KillOneNear(Vector3 pos, float radius)
    {
        for (int i = 0; i < fishes.Count; i++)
        {
            Fish f = fishes[i];
            if (f == null || f.state != Fish.State.Wild) continue;
            if (Vector3.Distance(pos, f.transform.position) < radius)
            {
                fishes.RemoveAt(i);
                f.Die();
                return true;
            }
        }
        return false;
    }

    public int FishCount { get { return fishes.Count; } }

    public void Remove(Fish f) { fishes.Remove(f); }
}

// Big decorative sea life (Quirky pack): not catchable, just wanders around.
public class AmbientAnimal : MonoBehaviour
{
    Bounds area;
    Vector3 target;
    float speed;
    Transform visual;

    public static AmbientAnimal Create(Rect rect, Transform parent)
    {
        GameObject go = new GameObject("AmbientAnimal");
        if (parent != null) go.transform.SetParent(parent, false);
        Vector3 pos = new Vector3(Random.Range(rect.xMin + 6f, rect.xMax - 6f), 0.35f, Random.Range(rect.yMin + 6f, rect.yMax - 6f));
        go.transform.position = pos;
        AmbientAnimal a = go.AddComponent<AmbientAnimal>();
        a.area = new Bounds(new Vector3(rect.center.x, 0.35f, rect.center.y), new Vector3(rect.width - 8f, 0.1f, rect.height - 8f));
        a.speed = Random.Range(0.8f, 1.8f);
        GameObject model = AssetLib.SpawnRandomSeaAnimal(go.transform, Random.Range(2.2f, 4.2f));
        if (model == null)
        {
            B.Prim(PrimitiveType.Sphere, "Body", go.transform, Vector3.zero, Vector3.zero,
                new Vector3(1.6f, 0.9f, 3f), MatLib.Get(new Color(0.55f, 0.6f, 0.72f)));
        }
        a.visual = go.transform;
        a.Pick();
        return a;
    }

    void Pick()
    {
        target = new Vector3(
            Random.Range(area.min.x, area.max.x), 0.35f,
            Random.Range(area.min.z, area.max.z));
    }

    void Update()
    {
        Vector3 to = target - transform.position;
        if (to.magnitude < 1f) { Pick(); return; }
        Vector3 dir = to.normalized;
        transform.position += dir * speed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 1.5f * Time.deltaTime);
    }
}
