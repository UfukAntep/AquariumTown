using System.Collections.Generic;
using UnityEngine;

// Lightweight localization. 20 languages with simple procedural flags.
// Core UI strings are translated for the Latin-script languages; others fall
// back to English. Base in-game text stays as-is unless routed through Loc.T.
public static class Loc
{
    public static readonly string[] Names = {
        "English", "Turkce", "Deutsch", "Francais", "Espanol", "Italiano",
        "Portugues", "Nederlands", "Polski", "Русский", "Українська", "Romana",
        "Cestina", "Svenska", "Dansk", "Norsk", "Suomi", "Indonesia",
        "Tieng Viet", "Filipino" };

    // 3-colour flags (top, mid, bottom or L/M/R). vertical[i] = stripes vertical.
    public static readonly Color[][] Flags = {
        C(0.0f,0.14f,0.4f, 1,1,1, 0.8f,0.1f,0.15f),            // EN (blue/white/red)
        C(0.9f,0.1f,0.15f, 1,1,1, 0.9f,0.1f,0.15f),            // TR (red/white/red)
        C(0,0,0, 0.85f,0.1f,0.1f, 1f,0.8f,0.1f),               // DE
        C(0.0f,0.14f,0.5f, 1,1,1, 0.9f,0.1f,0.2f),             // FR (vertical)
        C(0.8f,0.1f,0.12f, 1f,0.82f,0.1f, 0.8f,0.1f,0.12f),    // ES
        C(0.1f,0.55f,0.25f, 1,1,1, 0.8f,0.1f,0.15f),           // IT (vertical)
        C(0.1f,0.5f,0.25f, 1f,0.8f,0.1f, 0.8f,0.1f,0.15f),     // PT
        C(0.8f,0.1f,0.12f, 1,1,1, 0.1f,0.2f,0.6f),             // NL
        C(1,1,1, 0.85f,0.1f,0.2f, 0.85f,0.1f,0.2f),            // PL
        C(1,1,1, 0.1f,0.2f,0.7f, 0.8f,0.1f,0.15f),             // RU
        C(0.1f,0.4f,0.85f, 0.1f,0.4f,0.85f, 1f,0.82f,0.1f),    // UA
        C(0.0f,0.2f,0.6f, 1f,0.82f,0.1f, 0.8f,0.1f,0.15f),     // RO (vertical)
        C(1,1,1, 0.8f,0.1f,0.15f, 0.1f,0.2f,0.6f),             // CZ
        C(0.1f,0.35f,0.7f, 1f,0.82f,0.1f, 0.1f,0.35f,0.7f),    // SE
        C(0.8f,0.1f,0.15f, 1,1,1, 0.8f,0.1f,0.15f),            // DK
        C(0.8f,0.1f,0.15f, 1,1,1, 0.1f,0.2f,0.6f),             // NO
        C(1,1,1, 0.1f,0.35f,0.75f, 1,1,1),                     // FI
        C(0.85f,0.1f,0.15f, 1,1,1, 0.85f,0.1f,0.15f),          // ID
        C(0.85f,0.1f,0.15f, 0.85f,0.1f,0.15f, 1f,0.82f,0.1f),  // VN
        C(0.1f,0.2f,0.6f, 1,1,1, 0.8f,0.1f,0.15f) };           // PH

    public static readonly bool[] Vertical = {
        false,false,false,true,false,true,false,false,false,false,false,true,
        false,false,false,false,false,false,false,false };

    static Color[] C(float r1,float g1,float b1,float r2,float g2,float b2,float r3,float g3,float b3)
    { return new Color[] { new Color(r1,g1,b1), new Color(r2,g2,b2), new Color(r3,g3,b3) }; }

    // language is a GLOBAL setting (independent of the save slot)
    const string LangKey = "AT_GLang";
    public static bool Chosen { get { return PlayerPrefs.GetInt(LangKey, -1) >= 0; } }
    public static int Lang { get { return Mathf.Clamp(PlayerPrefs.GetInt(LangKey, 0), 0, Names.Length - 1); } }
    public static void Set(int i)
    {
        PlayerPrefs.SetInt(LangKey, Mathf.Clamp(i, 0, Names.Length - 1));
        PlayerPrefs.Save();
        if (Game.gm != null) Game.gm.language = i;
    }

    // translation table: key -> per-language string (index by language, English fallback)
    static Dictionary<string, string[]> table;

    static void Init()
    {
        if (table != null) return;
        table = new Dictionary<string, string[]>();
        // columns: EN TR DE FR ES IT PT NL PL RU UA RO CZ SE DK NO FI ID VN PH
        Add("CONTINUE", "Continue", "Devam Et", "Weiter", "Continuer", "Continuar", "Continua", "Continuar", "Doorgaan", "Kontynuuj", "Continue", "Continue", "Continua", "Pokracovat", "Fortsatt", "Fortsaet", "Fortsett", "Jatka", "Lanjutkan", "Continue", "Ituloy");
        Add("NEW_GAME", "New Game", "Yeni Oyun", "Neues Spiel", "Nouvelle Partie", "Nuevo Juego", "Nuova Partita", "Novo Jogo", "Nieuw Spel", "Nowa Gra", "New Game", "New Game", "Joc Nou", "Nova Hra", "Nytt Spel", "Nyt Spil", "Nytt Spill", "Uusi Peli", "Permainan Baru", "New Game", "Bagong Laro");
        Add("QUIT", "Quit", "Cikis", "Beenden", "Quitter", "Salir", "Esci", "Sair", "Afsluiten", "Wyjscie", "Quit", "Quit", "Iesire", "Konec", "Avsluta", "Afslut", "Avslutt", "Lopeta", "Keluar", "Quit", "Umalis");
        Add("SLOGAN", "Catch - Sell - Build your empire!", "Balik tut - Sat - Imparatorlugunu kur!", "Fangen - Verkaufen - Baue dein Reich!", "Peche - Vends - Batis ton empire!", "Pesca - Vende - Construye tu imperio!", "Pesca - Vendi - Costruisci il tuo impero!", "Pesque - Venda - Construa seu imperio!", "Vang - Verkoop - Bouw je rijk!", "Low - Sprzedaj - Zbuduj imperium!", "Catch - Sell - Build your empire!", "Catch - Sell - Build your empire!", "Prinde - Vinde - Construieste imperiul!", "Chytej - Prodej - Vybuduj imperium!", "Fanga - Salj - Bygg ditt imperium!", "Fang - Salg - Byg dit rige!", "Fang - Selg - Bygg ditt imperium!", "Kalasta - Myy - Rakenna imperiumi!", "Tangkap - Jual - Bangun kerajaanmu!", "Catch - Sell - Build your empire!", "Manghuli - Magbenta - Bumuo!");
        Add("PAUSED", "Paused", "Duraklatildi", "Pausiert", "En Pause", "Pausado", "In Pausa", "Pausado", "Gepauzeerd", "Pauza", "Paused", "Paused", "Pauza", "Pozastaveno", "Pausad", "Pauset", "Pauset", "Tauolla", "Dijeda", "Paused", "Naka-pause");
        Add("RESUME", "Resume", "Devam", "Fortsetzen", "Reprendre", "Reanudar", "Riprendi", "Retomar", "Hervatten", "Wznow", "Resume", "Resume", "Reia", "Obnovit", "Ateruppta", "Genoptag", "Fortsett", "Jatka", "Lanjutkan", "Resume", "Ipagpatuloy");
        Add("SOUND_ON", "Sound: On", "Ses: Acik", "Ton: An", "Son: Actif", "Sonido: Si", "Audio: On", "Som: Ligado", "Geluid: Aan", "Dzwiek: Wl", "Sound: On", "Sound: On", "Sunet: Pornit", "Zvuk: Zap", "Ljud: Pa", "Lyd: Til", "Lyd: Pa", "Aani: Paalla", "Suara: Nyala", "Sound: On", "Tunog: On");
        Add("SOUND_OFF", "Sound: Off", "Ses: Kapali", "Ton: Aus", "Son: Coupe", "Sonido: No", "Audio: Off", "Som: Desligado", "Geluid: Uit", "Dzwiek: Wyl", "Sound: Off", "Sound: Off", "Sunet: Oprit", "Zvuk: Vyp", "Ljud: Av", "Lyd: Fra", "Lyd: Av", "Aani: Pois", "Suara: Mati", "Sound: Off", "Tunog: Off");
        Add("SAVE_MENU", "Save & Menu", "Kaydet ve Menu", "Speichern & Menu", "Sauver & Menu", "Guardar & Menu", "Salva & Menu", "Salvar & Menu", "Opslaan & Menu", "Zapisz & Menu", "Save & Menu", "Save & Menu", "Salveaza & Meniu", "Ulozit & Menu", "Spara & Meny", "Gem & Menu", "Lagre & Meny", "Tallenna & Valikko", "Simpan & Menu", "Save & Menu", "I-save & Menu");
        Add("SELECT_LANGUAGE", "Select Language", "Dil Sec", "Sprache Wahlen", "Choisir la Langue", "Elige Idioma", "Scegli Lingua", "Escolha o Idioma", "Kies Taal", "Wybierz Jezyk", "Select Language", "Select Language", "Alege Limba", "Vyber Jazyk", "Valj Sprak", "Vaelg Sprog", "Velg Sprak", "Valitse Kieli", "Pilih Bahasa", "Chon Ngon Ngu", "Pumili ng Wika");
        Add("LANGUAGE", "Language", "Dil", "Sprache", "Langue", "Idioma", "Lingua", "Idioma", "Taal", "Jezyk", "Language", "Language", "Limba", "Jazyk", "Sprak", "Sprog", "Sprak", "Kieli", "Bahasa", "Ngon Ngu", "Wika");
        Add("OK", "OK", "Tamam", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK");
        table_ready = true;
    }
    static bool table_ready;

    static void Add(string key, params string[] vals) { table[key] = vals; }

    public static string T(string key)
    {
        Init();
        string[] vals;
        if (table.TryGetValue(key, out vals))
        {
            int l = Lang;
            if (l < vals.Length && !string.IsNullOrEmpty(vals[l])) return vals[l];
            return vals[0];
        }
        return key;
    }

    public static void Clear() { table = null; table_ready = false; }
}
