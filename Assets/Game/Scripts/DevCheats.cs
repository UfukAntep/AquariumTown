using UnityEngine;

// Test/cheat script — tweak money, level and trigger events instantly.
// Keys:
//   F1  +1.000$        F2  +100.000$
//   F3  Seviye +1 (kutlama ekrani ile)
//   F4  Seviye +5 (sessiz)
//   F5  Memnuniyet +20
//   F6  Hirsiz cagir      F7  Kopekbaligi
//   F8  Deprem            F9  Altin balik
//   F10 Para yagmuru      F12 Kaydi sil + yeni oyun
// Inspector'dan da SetMoney / SetLevel cagirabilirsin.
public class DevCheats : MonoBehaviour
{
    public static bool Enabled { get; private set; }
    public int setMoneyTo = -1;   // change in inspector during play
    public int setLevelTo = -1;
    readonly System.Collections.Generic.List<float> unlockPresses = new System.Collections.Generic.List<float>();

    void Awake()
    {
        // Session-only: every game launch starts with test keys hidden.
        Enabled = false;
    }

    public static void SetMoney(int amount)
    {
        if (Game.gm == null) return;
        Game.gm.Money = Mathf.Max(0, amount);
        if (Game.ui != null) Game.ui.OnMoneyChanged();
    }

    public static void SetLevel(int level)
    {
        if (Game.gm == null) return;
        Game.gm.CheatSetLevel(level);
    }

    void Update()
    {
        if (Game.gm == null) return;

        DetectSecretToggle();

        // inspector-driven
        if (setMoneyTo >= 0) { SetMoney(setMoneyTo); setMoneyTo = -1; }
        if (setLevelTo >= 1) { SetLevel(setLevelTo); setLevelTo = -1; }

        if (!Enabled) return;

        if (Input.GetKeyDown(KeyCode.F1)) { Game.gm.CheatMoney(1000); Game.ui.Toast("[CHEAT] +1.000$"); }
        if (Input.GetKeyDown(KeyCode.F2)) { Game.gm.CheatMoney(100000); Game.ui.Toast("[CHEAT] +100.000$"); }
        if (Input.GetKeyDown(KeyCode.F3)) Game.gm.UnlockNext();
        if (Input.GetKeyDown(KeyCode.F4)) { Game.gm.CheatSetLevel(Game.gm.Level + 5); Game.ui.Toast("[CHEAT] Seviye: " + Game.gm.Level); }
        if (Input.GetKeyDown(KeyCode.F5)) { Game.gm.AddSatisfaction(20f); Game.ui.Toast("[CHEAT] Memnuniyet +20"); }
        if (Input.GetKeyDown(KeyCode.F6) && Game.events != null) Game.events.SpawnThief();
        if (Input.GetKeyDown(KeyCode.F7) && Game.events != null) Game.events.TriggerShark();
        if (Input.GetKeyDown(KeyCode.F8) && Game.events != null) Game.events.TriggerQuake();
        if (Input.GetKeyDown(KeyCode.F9) && Game.events != null) Game.events.TriggerGoldenFish();
        if (Input.GetKeyDown(KeyCode.F10) && Game.events != null) Game.events.TriggerMoneyRain();
        if (Input.GetKeyDown(KeyCode.F12)) GameManager.NewGame();
    }

    void DetectSecretToggle()
    {
        if (!Input.GetKeyDown(KeyCode.U)) return;
        if (Game.ui != null && Game.ui.AnyMenuOpen) return;

        float now = Time.unscaledTime;
        unlockPresses.RemoveAll(t => now - t > 10f);
        unlockPresses.Add(now);
        if (unlockPresses.Count < 5) return;

        Enabled = !Enabled;
        unlockPresses.Clear();
        if (Game.ui != null)
        {
            Game.ui.SetCheatIndicator(Enabled);
            Game.ui.Toast(Enabled ? "Gizli test tuslari acildi." : "Gizli test tuslari kapatildi.", 3f);
        }
        Sfx.Play(Enabled ? Snd.Buy : Snd.Tick, 0.55f);
    }

    void OnDestroy()
    {
        Enabled = false;
        if (Game.ui != null) Game.ui.SetCheatIndicator(false);
    }
}
