using UnityEngine;

// Late-game saboteur. It visibly plants a bomb for ten seconds at one of
// three infrastructure targets, giving the player time to interrupt it.
public class Terrorist : MonoBehaviour
{
    enum Site { Bin, Sea, Beach }
    Site site;
    Transform visual;
    Vector3 target;
    bool entered;
    bool planting;
    float plantTime = 10f;
    int health = 50;
    TextMesh countdown;
    GameObject healthRoot;
    Transform healthFill;

    public static Terrorist Spawn(bool tutorial = false)
    {
        GameObject go = new GameObject("Terrorist");
        go.transform.position = Customer.DoorPos + new Vector3(2f, 0f, -1f);
        Terrorist terrorist = go.AddComponent<Terrorist>();
        terrorist.Build();
        if (Game.ui != null) Game.ui.Toast("TERORIST GELIYOR! Cop kutusu, sahil veya denize bomba koyabilir. Kurulumu bitmeden saldir!", 8f);
        Sfx.Play(Snd.Terrorist, 1f);
        Sfx.DangerFor(14f);
        return terrorist;
    }

    void Build()
    {
        visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);
        B.Stickman(visual, new Color(0.32f, 0.04f, 0.07f));
        B.Prim(PrimitiveType.Cube, "BombBag", visual, new Vector3(0f, 1.2f, -0.45f), Vector3.zero,
            new Vector3(0.65f, 0.7f, 0.3f), MatLib.Get(new Color(0.08f, 0.08f, 0.09f)));
        B.Text3D("TERORIST", transform, new Vector3(0f, 3f, 0f), 0.075f, new Color(1f, 0.25f, 0.2f)).fontStyle = FontStyle.Bold;
        healthRoot = new GameObject("HealthBar");
        healthRoot.transform.SetParent(transform, false);
        healthRoot.transform.localPosition = new Vector3(0f, 3.45f, 0f);
        B.Prim(PrimitiveType.Cube, "Back", healthRoot.transform, Vector3.zero, Vector3.zero, new Vector3(1.8f, 0.18f, 0.08f), MatLib.Get(new Color(0.16f, 0.04f, 0.04f)));
        healthFill = B.Prim(PrimitiveType.Cube, "Fill", healthRoot.transform, new Vector3(-0.9f, 0f, -0.05f), Vector3.zero,
            new Vector3(1.8f, 0.14f, 0.06f), MatLib.Get(UIKit.Red)).transform;
        healthRoot.AddComponent<Billboard>();

        site = (Site)Random.Range(0, 3);
        if (site == Site.Bin && Game.trash != null) target = Game.trash.BinPos;
        else if (site == Site.Sea && Game.sea != null)
            target = new Vector3(Game.sea.area.xMin + Game.sea.area.width * 0.42f, 0f, Random.Range(Game.sea.area.yMin + 12f, Game.sea.area.yMax - 12f));
        else target = new Vector3(23f, 0f, Random.Range(5f, 38f));
    }

    void Update()
    {
        if (Game.gm == null) { Destroy(gameObject); return; }
        float dt = Time.deltaTime;
        if (!entered)
        {
            if (MoveTo(Customer.GateInside, dt)) entered = true;
            return;
        }
        if (!planting)
        {
            if (MoveTo(target, dt))
            {
                planting = true;
                countdown = B.Text3D("BOMBA 10", transform, new Vector3(0f, 3.8f, 0f), 0.095f, UIKit.Red);
                countdown.fontStyle = FontStyle.Bold;
            }
            return;
        }
        plantTime -= dt;
        int seconds = Mathf.Max(0, Mathf.CeilToInt(plantTime));
        if (countdown != null) countdown.text = "BOMBA " + seconds;
        if (Mathf.CeilToInt(plantTime + dt) != seconds) Sfx.Play(Snd.BombTick, 0.75f);
        visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 15f) * 10f);
        if (plantTime <= 0f) Explode();
    }

    bool MoveTo(Vector3 destination, float dt)
    {
        destination.y = 0f;
        Vector3 delta = destination - transform.position; delta.y = 0f;
        if (delta.magnitude < 0.5f) return true;
        Vector3 direction = delta.normalized;
        transform.position += direction * 4.2f * dt;
        visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(direction), 10f * dt);
        return false;
    }

    public void PlayerHit(Vector3 attacker, int damage)
    {
        health = Mathf.Max(0, health - Mathf.Max(1, damage));
        float ratio = health / 50f;
        healthFill.localScale = new Vector3(1.8f * ratio, 0.14f, 0.06f);
        healthFill.localPosition = new Vector3(-0.9f + 0.9f * ratio, 0f, -0.05f);
        Sfx.Play(Snd.Punch, 0.8f);
        Vector3 away = transform.position - attacker; away.y = 0f;
        if (away.sqrMagnitude > 0.01f) transform.position += away.normalized * 0.45f;
        if (health <= 0)
        {
            if (Game.ui != null) Game.ui.Toast("Teroristi durdurdun; bomba kurulmadan etkisiz hale getirildi!", 5f);
            Sfx.EndDanger();
            Destroy(gameObject);
        }
    }

    void Explode()
    {
        Sfx.Play(Snd.Explosion, 1f);
        if (Game.cam != null) Game.cam.Shake(1.8f, 0.9f);
        GameObject blast = B.Prim(PrimitiveType.Sphere, "BombExplosion", null, target + Vector3.up, Vector3.zero, Vector3.one,
            MatLib.Glass(new Color(1f, 0.3f, 0.05f, 0.75f)));
        blast.AddComponent<BombBlastVisual>();

        if (site == Site.Sea)
        {
            if (Game.sea != null) Game.sea.KillInRadius(target, 17f, 40);
            Staff[] staff = FindObjectsByType<Staff>(FindObjectsSortMode.None);
            for (int i = 0; i < staff.Length; i++) if (Vector3.Distance(staff[i].transform.position, target) < 17f) staff[i].BlastHit(target);
            if (Game.trash != null) Game.trash.ScatterIntoSea(target, 12);
            if (Game.ui != null) Game.ui.Toast("Bomba denizde patladi! Baliklar oldu ve deniz kirlendi!", 7f);
        }
        else if (site == Site.Bin)
        {
            if (Game.trash != null)
            {
                for (int i = 0; i < 32; i++) Game.trash.SpawnLandTrash(new Vector3(Random.Range(-42f, 6f), 0f, Random.Range(-6f, 50f)), true);
                if (Game.sea != null) Game.trash.ScatterIntoSea(new Vector3(Game.sea.area.xMin + 2f, 0f, target.z), 12);
            }
            if (Game.ui != null) Game.ui.Toast("Cop kutusu patladi! Pislikler dukkana ve denize sacildi!", 7f);
        }
        else
        {
            BeachVisitor[] visitors = FindObjectsByType<BeachVisitor>(FindObjectsSortMode.None);
            for (int i = 0; i < visitors.Length; i++) if (Vector3.Distance(visitors[i].transform.position, target) < 15f) visitors[i].BlastHit(target);
            GameObject[] all = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
                if (all[i] != null && all[i].name == "Lounger" && Vector3.Distance(all[i].transform.position, target) < 16f) Destroy(all[i]);
            if (Game.trash != null)
                for (int i = 0; i < 24; i++) Game.trash.SpawnLandTrash(target + new Vector3(Random.Range(-11f, 11f), 0f, Random.Range(-11f, 11f)), false);
            if (Game.ui != null) Game.ui.Toast("Sahil bombalandi! Sezlonglar parcalandi ve sahil kirlendi!", 7f);
        }
        Game.gm.AddSatisfaction(-10f);
        Sfx.EndDanger();
        Destroy(gameObject);
    }
}

public class BombBlastVisual : MonoBehaviour
{
    float time;
    void Update()
    {
        time += Time.deltaTime;
        transform.localScale = Vector3.one * (1f + time * 18f);
        if (time > 0.75f) Destroy(gameObject);
    }
}
