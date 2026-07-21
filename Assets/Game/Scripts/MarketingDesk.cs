using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Physical campaign desk unlocked by expanding the management room.
public class MarketingDesk : MonoBehaviour
{
    static readonly string[] Names = {
        "GAZETE ILANI", "EL ILANI", "YEREL RADYO", "OKUL OTOBUSU CAGIR",
        "INDIRIM KUPONU", "SOSYAL MEDYA", "INFLUENCER CAGIR", "DERGI REKLAMI",
        "DERGI KAPAGI", "TV REKLAMI", "SEHIR BILLBOARDU", "UNLU DAVETI",
        "FESTIVAL SPONSORLUGU", "ULUSAL KAMPANYA", "GLOBAL CANLI YAYIN"
    };
    static readonly string[] Descs = {
        "Mesai bitimine kadar ziyaretciyi %5 artirir.", "Yakindaki musteri trafigini %8 artirir.",
        "Sehirde duyulur; ziyaretciyi %10 artirir.", "Okul gezisini hemen getirir ve ziyaretciyi %5 artirir.",
        "Bugunku ziyaretciyi %12 artirir.", "Bugunku ziyaretciyi %15 artirir.",
        "Influencer, fotografcilar ve hayranlari toplu alisverise gelir; %25 artis.", "Bugunku ziyaretciyi %18 artirir.",
        "Dergi kapagiyla ziyaretciyi %30 artirir.", "Mesai bitimine kadar ziyaretciyi %35 artirir.",
        "Sehir genelinde ziyaretciyi %22 artirir.", "Unlu konuk sayesinde ziyaretciyi %40 artirir.",
        "Festival boyunca ziyaretciyi %45 artirir.", "Ulusal tanitim ziyaretciyi %55 artirir.",
        "En guclu kampanya; ziyaretciyi %65 artirir."
    };
    static readonly int[] Levels = { 1,3,5,8,10,12,15,18,20,25,28,32,36,45,55 };
    static readonly int[] Costs = { 150,300,600,1000,850,1400,2500,3200,5000,8500,12000,18000,26000,50000,90000 };
    static readonly float[] Bonuses = { .05f,.08f,.10f,.05f,.12f,.15f,.25f,.18f,.30f,.35f,.22f,.40f,.45f,.55f,.65f };

    GameObject viewer;
    Transform listRoot;
    Text pageText, activeText;
    int page;
    float openedAt;
    public bool ViewerOpen { get { return viewer != null && viewer.activeSelf; } }
    public bool PlayerNear(Vector3 p) { return Vector3.Distance(p, transform.position) < 3.4f; }

    public static MarketingDesk Create(Transform parent, Vector3 localPosition)
    {
        GameObject root = new GameObject("MarketingDeskStation");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = localPosition;
        Material wood = MatLib.Get(new Color(.55f,.31f,.16f));
        B.Prim(PrimitiveType.Cube, "Desk", root.transform, new Vector3(0,.72f,0), Vector3.zero, new Vector3(3.2f,1.25f,1.2f), wood, true);
        B.Prim(PrimitiveType.Cube, "Telephone", root.transform, new Vector3(-.75f,1.45f,0), Vector3.zero, new Vector3(.48f,.2f,.35f), MatLib.Get(new Color(.12f,.14f,.18f)));
        B.Prim(PrimitiveType.Cube, "CampaignFiles", root.transform, new Vector3(.65f,1.45f,0), new Vector3(0,12f,0), new Vector3(.72f,.08f,.5f), MatLib.Get(UIKit.Yellow));
        B.Text3D("PAZARLAMA", root.transform, new Vector3(0,2.05f,0), .065f, Color.white);
        MarketingDesk desk = root.AddComponent<MarketingDesk>();
        Game.marketingDesk = desk;
        return desk;
    }

    public void Open()
    {
        EnsureUI(); page = 0; Refresh(); viewer.SetActive(true); openedAt = Time.unscaledTime; Sfx.Play(Snd.Tick,.5f);
    }

    void EnsureUI()
    {
        if (viewer != null) return;
        viewer = new GameObject("MarketingCampaignViewer");
        Canvas canvas = viewer.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 48;
        CanvasScaler scaler = viewer.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600,900); scaler.matchWidthOrHeight=.5f;
        viewer.AddComponent<GraphicRaycaster>();
        GameObject shade = UIKit.Panel(viewer.transform, new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(1600,900), new Color(.03f,.05f,.09f,.96f), false, false);
        GameObject box = UIKit.Panel(shade.transform, new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(1280,760), UIKit.Cream, true, true);
        GameObject header = UIKit.Panel(box.transform, new Vector2(.5f,1f), new Vector2(.5f,1f), new Vector2(0,-12), new Vector2(1240,78), UIKit.Blue, true, true);
        UIKit.Label(header.transform, "PAZARLAMA MASASI", 28, Color.white, TextAnchor.MiddleCenter, true);
        GameObject active = UIKit.Panel(box.transform, new Vector2(.5f,1f), new Vector2(.5f,1f), new Vector2(0,-94), new Vector2(1160,50), new Color(1f,.94f,.76f), true, false);
        activeText = UIKit.Label(active.transform, "", 17, UIKit.TextDark, TextAnchor.MiddleCenter);
        GameObject list = UIKit.Panel(box.transform, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(0,-4), new Vector2(1160,500), new Color(0,0,0,.001f), false, false);
        listRoot = list.transform;
        UIKit.Btn(box.transform, new Vector2(-330,-328), new Vector2(210,52), UIKit.Blue, "< ONCEKI", 16, delegate { page=Mathf.Max(0,page-1); Refresh(); });
        UIKit.Btn(box.transform, new Vector2(0,-328), new Vector2(210,52), UIKit.Red, "KAPAT (E)", 16, Close);
        UIKit.Btn(box.transform, new Vector2(330,-328), new Vector2(210,52), UIKit.Blue, "SONRAKI >", 16, delegate { page=Mathf.Min(2,page+1); Refresh(); });
        GameObject pg = UIKit.Panel(box.transform,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,24),new Vector2(180,34),new Color(0,0,0,.001f),false,false);
        pageText = UIKit.Label(pg.transform,"",15,UIKit.TextDark,TextAnchor.MiddleCenter);
        viewer.SetActive(false);
    }

    void Refresh()
    {
        for (int i=listRoot.childCount-1;i>=0;i--) Destroy(listRoot.GetChild(i).gameObject);
        page=Mathf.Clamp(page,0,2); pageText.text="Sayfa "+(page+1)+"/3";
        activeText.text=Game.gm.marketingVisitorBonus>0f ? "AKTIF: "+Game.gm.activeMarketingName+"   +%"+Mathf.RoundToInt(Game.gm.marketingVisitorBonus*100)+" ziyaretci (21:00'a kadar)" : "Aktif kampanya yok";
        int start=page*5;
        for(int n=0;n<5;n++)
        {
            int idx=start+n; if(idx>=Names.Length) break;
            float y=200f-n*96f;
            GameObject row=UIKit.Panel(listRoot,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,y),new Vector2(1120,86),Color.white,true,true);
            GameObject badge=UIKit.Icon(row.transform,UIKit.Circle(),new Vector2(0,.5f),new Vector2(48,0),new Vector2(54,54),idx<5?UIKit.Green:idx<10?UIKit.Orange:UIKit.Purple);
            UIKit.Icon(badge.transform,UIKit.Star(),new Vector2(.5f,.5f),Vector2.zero,new Vector2(34,34),Color.white);
            GameObject txt=UIKit.Panel(row.transform,new Vector2(0,.5f),new Vector2(0,.5f),new Vector2(88,0),new Vector2(700,72),new Color(0,0,0,.001f),false,false);
            UIKit.Label(txt.transform,Names[idx]+"  (Sv "+Levels[idx]+")\n"+Descs[idx],15,UIKit.TextDark,TextAnchor.MiddleLeft);
            int campaign=idx;
            Button b=UIKit.Btn(row.transform,new Vector2(410,0),new Vector2(230,52),UIKit.Green,"",15,delegate { Buy(campaign); });
            Text bt=b.GetComponentInChildren<Text>();
            bool levelOk=Game.gm.Level>=Levels[idx], moneyOk=Game.gm.Money>=Costs[idx];
            bt.text=!levelOk?"Sv "+Levels[idx]+" GEREKLI":"BASLAT  $"+B.Money(Costs[idx]);
            b.interactable=levelOk && moneyOk; b.image.color=levelOk&&moneyOk?UIKit.Green:new Color(.65f,.65f,.65f);
        }
    }

    void Buy(int i)
    {
        if (Game.gm.clockMinutes < 6*60f || Game.gm.clockMinutes >= 21*60f) { Game.ui.Toast("Kampanyalar 06:00-21:00 mesaisinde baslatilabilir.",4f); return; }
        if (Game.gm.Level<Levels[i] || !Game.gm.TrySpend(Costs[i])) { Sfx.Play(Snd.Drop,.35f); return; }
        Game.gm.marketingVisitorBonus=Bonuses[i]; Game.gm.activeMarketingName=Names[i];
        if(i==3 && Game.events!=null) Game.events.TriggerSchoolTrip(true);
        if(i==6) StartCoroutine(InfluencerGroup());
        Sfx.Play(Snd.Buy,.85f); Game.ui.Toast(Names[i]+" basladi! Ziyaretci +%"+Mathf.RoundToInt(Bonuses[i]*100),4f); Refresh();
    }

    IEnumerator InfluencerGroup()
    {
        Customer.SpawnInfluencer(false);
        for(int i=0;i<4;i++){ yield return new WaitForSeconds(.18f); Customer.SpawnInfluencer(true); }
        for(int i=0;i<5;i++){ yield return new WaitForSeconds(.14f); Customer.Spawn(); }
    }

    void Update(){ if(ViewerOpen && Time.unscaledTime-openedAt>.2f && (ControlBindings.Down(ControlAction.Interact)||Input.GetKeyDown(KeyCode.Escape))) Close(); }
    public void Close(){ if(viewer!=null)viewer.SetActive(false); Sfx.Play(Snd.Tick,.35f); }
    void OnDestroy(){ if(viewer!=null)Destroy(viewer); if(Game.marketingDesk==this)Game.marketingDesk=null; }
}
