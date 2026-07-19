using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Global, save-independent localization. All UI/TextMesh output also passes
// through Tr(), so runtime-generated labels and world signs cannot bypass it.
public static class Loc
{
    // EN TR DE FR ES IT PT NL PL RU UK RO CS SV DA NO FI ID VI FIL
    public static readonly string[] Names = {
        "English", "Türkçe", "Deutsch", "Français", "Español", "Italiano",
        "Português", "Nederlands", "Polski", "Русский", "Українська", "Română",
        "Čeština", "Svenska", "Dansk", "Norsk", "Suomi", "Bahasa Indonesia",
        "Tiếng Việt", "Filipino" };

    public static readonly string[] FlagCodes = {
        "GB", "TR", "DE", "FR", "ES", "IT", "PT", "NL", "PL", "RU",
        "UA", "RO", "CZ", "SE", "DK", "NO", "FI", "ID", "VN", "PH" };

    const string LangKey = "AT_GLang";
    public static bool Chosen { get { return PlayerPrefs.GetInt(LangKey, -1) >= 0; } }
    public static int Lang { get { return Mathf.Clamp(PlayerPrefs.GetInt(LangKey, 0), 0, Names.Length - 1); } }

    public static void Set(int i)
    {
        i = Mathf.Clamp(i, 0, Names.Length - 1);
        PlayerPrefs.SetInt(LangKey, i);
        PlayerPrefs.Save();
        if (Game.gm != null) Game.gm.language = i;
    }

    static Dictionary<string, string[]> table;
    static List<KeyValuePair<string, string>> literalKeys;
    static List<KeyValuePair<string, string>> englishFallback;

    static void Init()
    {
        if (table != null) return;
        table = new Dictionary<string, string[]>();
        literalKeys = new List<KeyValuePair<string, string>>();
        englishFallback = new List<KeyValuePair<string, string>>();

        Add("CONTINUE", "Continue", "Devam Et", "Weiter", "Continuer", "Continuar", "Continua", "Continuar", "Doorgaan", "Kontynuuj", "Продолжить", "Продовжити", "Continuă", "Pokračovat", "Fortsätt", "Fortsæt", "Fortsett", "Jatka", "Lanjutkan", "Tiếp tục", "Ituloy");
        Add("NEW_GAME", "New Game", "Yeni Oyun", "Neues Spiel", "Nouvelle partie", "Nueva partida", "Nuova partita", "Novo jogo", "Nieuw spel", "Nowa gra", "Новая игра", "Нова гра", "Joc nou", "Nová hra", "Nytt spel", "Nyt spil", "Nytt spill", "Uusi peli", "Permainan baru", "Trò chơi mới", "Bagong laro");
        Add("QUIT", "Quit", "Çıkış", "Beenden", "Quitter", "Salir", "Esci", "Sair", "Afsluiten", "Wyjdź", "Выйти", "Вийти", "Ieșire", "Ukončit", "Avsluta", "Afslut", "Avslutt", "Lopeta", "Keluar", "Thoát", "Umalis");
        Add("SLOGAN", "Catch • Sell • Build your empire!", "Yakala • Sat • İmparatorluğunu kur!", "Fangen • Verkaufen • Imperium bauen!", "Pêchez • Vendez • Bâtissez votre empire !", "¡Pesca • Vende • Construye tu imperio!", "Cattura • Vendi • Costruisci il tuo impero!", "Pesque • Venda • Construa seu império!", "Vang • Verkoop • Bouw je rijk!", "Łów • Sprzedawaj • Zbuduj imperium!", "Лови • Продавай • Строй империю!", "Лови • Продавай • Будуй імперію!", "Prinde • Vinde • Construiește-ți imperiul!", "Chytej • Prodávej • Vybuduj říši!", "Fånga • Sälj • Bygg ditt imperium!", "Fang • Sælg • Byg dit imperium!", "Fang • Selg • Bygg imperiet ditt!", "Kalasta • Myy • Rakenna valtakuntasi!", "Tangkap • Jual • Bangun kerajaanmu!", "Bắt • Bán • Xây dựng đế chế!", "Manghuli • Magbenta • Bumuo ng imperyo!");
        Add("PAUSED", "Paused", "Duraklatıldı", "Pausiert", "En pause", "En pausa", "In pausa", "Em pausa", "Gepauzeerd", "Pauza", "Пауза", "Пауза", "Pauză", "Pozastaveno", "Pausat", "Sat på pause", "Pauset", "Tauko", "Dijeda", "Tạm dừng", "Naka-pause");
        Add("RESUME", "Resume", "Devam", "Fortsetzen", "Reprendre", "Reanudar", "Riprendi", "Retomar", "Hervatten", "Wznów", "Продолжить", "Продовжити", "Continuă", "Pokračovat", "Fortsätt", "Fortsæt", "Fortsett", "Jatka", "Lanjutkan", "Tiếp tục", "Ipagpatuloy");
        Add("SOUND_ON", "Sound: On", "Ses: Açık", "Ton: An", "Son : activé", "Sonido: activado", "Audio: attivo", "Som: ligado", "Geluid: aan", "Dźwięk: wł.", "Звук: вкл.", "Звук: увімк.", "Sunet: pornit", "Zvuk: zap.", "Ljud: på", "Lyd: til", "Lyd: på", "Ääni: päällä", "Suara: nyala", "Âm thanh: bật", "Tunog: bukas");
        Add("SOUND_OFF", "Sound: Off", "Ses: Kapalı", "Ton: Aus", "Son : coupé", "Sonido: desactivado", "Audio: disattivo", "Som: desligado", "Geluid: uit", "Dźwięk: wył.", "Звук: выкл.", "Звук: вимк.", "Sunet: oprit", "Zvuk: vyp.", "Ljud: av", "Lyd: fra", "Lyd: av", "Ääni: pois", "Suara: mati", "Âm thanh: tắt", "Tunog: patay");
        Add("SAVE_MENU", "Save & Menu", "Kaydet ve Menü", "Speichern & Menü", "Sauver et menu", "Guardar y menú", "Salva e menu", "Salvar e menu", "Opslaan en menu", "Zapisz i menu", "Сохранить и в меню", "Зберегти й у меню", "Salvează și meniu", "Uložit a menu", "Spara och meny", "Gem og menu", "Lagre og meny", "Tallenna ja valikko", "Simpan & menu", "Lưu & menu", "I-save at menu");
        Add("SELECT_LANGUAGE", "Select language", "Dil seç", "Sprache wählen", "Choisir la langue", "Elegir idioma", "Scegli la lingua", "Escolher idioma", "Kies taal", "Wybierz język", "Выберите язык", "Оберіть мову", "Alege limba", "Vyberte jazyk", "Välj språk", "Vælg sprog", "Velg språk", "Valitse kieli", "Pilih bahasa", "Chọn ngôn ngữ", "Pumili ng wika");
        Add("LANGUAGE", "Language", "Dil", "Sprache", "Langue", "Idioma", "Lingua", "Idioma", "Taal", "Język", "Язык", "Мова", "Limbă", "Jazyk", "Språk", "Sprog", "Språk", "Kieli", "Bahasa", "Ngôn ngữ", "Wika");
        Add("OK", "OK", "Tamam", "OK", "OK", "Aceptar", "OK", "OK", "OK", "OK", "ОК", "ОК", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK");

        Add("UPGRADES", "UPGRADES", "GELİŞTİRME", "VERBESSERUNGEN", "AMÉLIORATIONS", "MEJORAS", "POTENZIAMENTI", "MELHORIAS", "UPGRADES", "ULEPSZENIA", "УЛУЧШЕНИЯ", "ПОКРАЩЕННЯ", "ÎMBUNĂTĂȚIRI", "VYLEPŠENÍ", "UPPGRADERINGAR", "OPGRADERINGER", "OPPGRADERINGER", "PÄIVITYKSET", "PENINGKATAN", "NÂNG CẤP", "MGA UPGRADE");
        Add("STAFF", "STAFF", "PERSONEL", "PERSONAL", "PERSONNEL", "PERSONAL", "PERSONALE", "EQUIPE", "PERSONEEL", "PERSONEL", "ПЕРСОНАЛ", "ПЕРСОНАЛ", "PERSONAL", "PERSONÁL", "PERSONAL", "PERSONALE", "ANSATTE", "HENKILÖSTÖ", "STAF", "NHÂN VIÊN", "MGA TAUHAN");
        Add("AQUARIUMS", "AQUARIUMS", "AKVARYUMLAR", "AQUARIEN", "AQUARIUMS", "ACUARIOS", "ACQUARI", "AQUÁRIOS", "AQUARIA", "AKWARIA", "АКВАРИУМЫ", "АКВАРІУМИ", "ACVARII", "AKVÁRIA", "AKVARIER", "AKVARIER", "AKVARIER", "AKVAARIOT", "AKUARIUM", "BỂ CÁ", "MGA AKWARYUM");
        Add("DECOR", "DECOR", "DEKOR", "DEKORATION", "DÉCOR", "DECORACIÓN", "ARREDI", "DECORAÇÃO", "DECOR", "DEKORACJE", "ДЕКОР", "ДЕКОР", "DECOR", "DEKORACE", "DEKOR", "DEKORATION", "DEKOR", "SISUSTUS", "DEKORASI", "TRANG TRÍ", "DEKORASYON");
        Add("PAINT", "PAINT", "BOYA", "FARBE", "PEINTURE", "PINTURA", "VERNICE", "PINTURA", "VERF", "FARBA", "КРАСКА", "ФАРБА", "VOPSEA", "BARVA", "FÄRG", "MALING", "MALING", "MAALI", "CAT", "SƠN", "PINTURA");
        Add("TECHNOLOGY", "TECHNOLOGY", "TEKNOLOJİ", "TECHNOLOGIE", "TECHNOLOGIE", "TECNOLOGÍA", "TECNOLOGIA", "TECNOLOGIA", "TECHNOLOGIE", "TECHNOLOGIA", "ТЕХНОЛОГИИ", "ТЕХНОЛОГІЇ", "TEHNOLOGIE", "TECHNOLOGIE", "TEKNIK", "TEKNOLOGI", "TEKNOLOGI", "TEKNOLOGIA", "TEKNOLOGI", "CÔNG NGHỆ", "TEKNOLOHIYA");
        Add("DATABASE", "DATABASE", "VERİTABANI", "DATENBANK", "BASE DE DONNÉES", "BASE DE DATOS", "DATABASE", "BANCO DE DADOS", "DATABASE", "BAZA DANYCH", "БАЗА ДАННЫХ", "БАЗА ДАНИХ", "BAZĂ DE DATE", "DATABÁZE", "DATABAS", "DATABASE", "DATABASE", "TIETOKANTA", "DATABASE", "CƠ SỞ DỮ LIỆU", "DATABASE");
        Add("HISTORY", "HISTORY", "GEÇMİŞ", "VERLAUF", "HISTORIQUE", "HISTORIAL", "CRONOLOGIA", "HISTÓRICO", "GESCHIEDENIS", "HISTORIA", "ИСТОРИЯ", "ІСТОРІЯ", "ISTORIC", "HISTORIE", "HISTORIK", "HISTORIK", "HISTORIKK", "HISTORIA", "RIWAYAT", "LỊCH SỬ", "KASAYSAYAN");
        Add("REVIEWS", "REVIEWS", "YORUMLAR", "BEWERTUNGEN", "AVIS", "RESEÑAS", "RECENSIONI", "AVALIAÇÕES", "REVIEWS", "OPINIE", "ОТЗЫВЫ", "ВІДГУКИ", "RECENZII", "RECENZE", "RECENSIONER", "ANMELDELSER", "ANMELDELSER", "ARVOSTELUT", "ULASAN", "ĐÁNH GIÁ", "MGA REVIEW");
        Add("CLOSE", "CLOSE", "KAPAT", "SCHLIESSEN", "FERMER", "CERRAR", "CHIUDI", "FECHAR", "SLUITEN", "ZAMKNIJ", "ЗАКРЫТЬ", "ЗАКРИТИ", "ÎNCHIDE", "ZAVŘÍT", "STÄNG", "LUK", "LUKK", "SULJE", "TUTUP", "ĐÓNG", "ISARA");
        Add("END_DAY", "END DAY", "GÜNÜ BİTİR", "TAG BEENDEN", "TERMINER LA JOURNÉE", "TERMINAR EL DÍA", "TERMINA GIORNATA", "ENCERRAR O DIA", "DAG BEËINDIGEN", "ZAKOŃCZ DZIEŃ", "ЗАВЕРШИТЬ ДЕНЬ", "ЗАВЕРШИТИ ДЕНЬ", "ÎNCHEIE ZIUA", "UKONČIT DEN", "AVSLUTA DAGEN", "AFSLUT DAGEN", "AVSLUTT DAGEN", "PÄÄTÄ PÄIVÄ", "AKHIRI HARI", "KẾT THÚC NGÀY", "TAPUSIN ANG ARAW");
        Add("PREVIOUS", "PREVIOUS", "ÖNCEKİ", "ZURÜCK", "PRÉCÉDENT", "ANTERIOR", "PRECEDENTE", "ANTERIOR", "VORIGE", "POPRZEDNIA", "НАЗАД", "НАЗАД", "ANTERIOR", "PŘEDCHOZÍ", "FÖREGÅENDE", "FORRIGE", "FORRIGE", "EDELLINEN", "SEBELUMNYA", "TRƯỚC", "NAKARAAN");
        Add("NEXT", "NEXT", "SONRAKİ", "WEITER", "SUIVANT", "SIGUIENTE", "SUCCESSIVO", "PRÓXIMO", "VOLGENDE", "NASTĘPNA", "ДАЛЕЕ", "ДАЛІ", "URMĂTOR", "DALŠÍ", "NÄSTA", "NÆSTE", "NESTE", "SEURAAVA", "BERIKUTNYA", "TIẾP", "SUSUNOD");
        Add("LOCKED", "LOCKED", "KİLİTLİ", "GESPERRT", "VERROUILLÉ", "BLOQUEADO", "BLOCCATO", "BLOQUEADO", "VERGRENDELD", "ZABLOKOWANE", "ЗАКРЫТО", "ЗАБЛОКОВАНО", "BLOCAT", "ZAMČENO", "LÅST", "LÅST", "LÅST", "LUKITTU", "TERKUNCI", "ĐÃ KHÓA", "NAKA-LOCK");
        Add("TOILET_AREA", "TOILET AREA", "TUVALET ALANI", "TOILETTENBEREICH", "ZONE DES TOILETTES", "ZONA DE BAÑOS", "AREA BAGNI", "ÁREA DOS BANHEIROS", "TOILETRUIMTE", "STREFA TOALET", "ТУАЛЕТНАЯ ЗОНА", "ЗОНА ТУАЛЕТІВ", "ZONĂ TOALETE", "TOALETY", "TOALETTOMRÅDE", "TOILETOMRÅDE", "TOALETTOMRÅDE", "WC-ALUE", "AREA TOILET", "KHU VỆ SINH", "LUGAR NG BANYO");
        Add("ENTRANCE", "ENTRANCE", "GİRİŞ", "EINGANG", "ENTRÉE", "ENTRADA", "INGRESSO", "ENTRADA", "INGANG", "WEJŚCIE", "ВХОД", "ВХІД", "INTRARE", "VCHOD", "INGÅNG", "INDGANG", "INNGANG", "SISÄÄNKÄYNTI", "MASUK", "LỐI VÀO", "PASUKAN");
        Add("SEA", "SEA", "DENİZ", "MEER", "MER", "MAR", "MARE", "MAR", "ZEE", "MORZE", "МОРЕ", "МОРЕ", "MARE", "MOŘE", "HAV", "HAV", "HAV", "MERI", "LAUT", "BIỂN", "DAGAT");
        Add("OPEN", "OPEN", "AÇIK", "OFFEN", "OUVERT", "ABIERTO", "APERTO", "ABERTO", "OPEN", "OTWARTE", "ОТКРЫТО", "ВІДКРИТО", "DESCHIS", "OTEVŘENO", "ÖPPET", "ÅBEN", "ÅPEN", "AVOINNA", "BUKA", "MỞ", "BUKAS");
        Add("REQUIRED", "REQUIRED", "GEREKLİ", "ERFORDERLICH", "REQUIS", "NECESARIO", "RICHIESTO", "NECESSÁRIO", "VEREIST", "WYMAGANE", "ТРЕБУЕТСЯ", "ПОТРІБНО", "NECESAR", "VYŽADOVÁNO", "KRÄVS", "KRÆVET", "KREVES", "VAADITAAN", "DIPERLUKAN", "BẮT BUỘC", "KAILANGAN");
        Add("DAILY_TASKS", "DAILY TASKS", "GÜNLÜK GÖREVLER", "TÄGLICHE AUFGABEN", "TÂCHES QUOTIDIENNES", "TAREAS DIARIAS", "INCARICHI GIORNALIERI", "TAREFAS DIÁRIAS", "DAGELIJKSE TAKEN", "CODZIENNE ZADANIA", "ЕЖЕДНЕВНЫЕ ЗАДАНИЯ", "ЩОДЕННІ ЗАВДАННЯ", "SARCINI ZILNICE", "DENNÍ ÚKOLY", "DAGLIGA UPPDRAG", "DAGLIGE OPGAVER", "DAGLIGE OPPGAVER", "PÄIVITTÄISET TEHTÄVÄT", "TUGAS HARIAN", "NHIỆM VỤ HẰNG NGÀY", "ARAW-ARAW NA GAWAIN");

        Add("CAMERA_VIEW", "Camera View", "Kamera Açısı", "Kameraansicht", "Vue caméra", "Vista de cámara", "Visuale camera", "Vista da câmera", "Cameraweergave", "Widok kamery", "Вид камеры", "Вигляд камери", "Unghi cameră", "Pohled kamery", "Kameravy", "Kameravisning", "Kameravisning", "Kameranäkymä", "Sudut kamera", "Góc camera", "Anggulo ng camera");
        Add("TOP_DOWN", "Top-down", "Tepeden", "Von oben", "Vue du dessus", "Desde arriba", "Dall'alto", "De cima", "Bovenaanzicht", "Z góry", "Сверху", "Згори", "De sus", "Shora", "Ovanifrån", "Oppefra", "Ovenfra", "Ylhäältä", "Dari atas", "Từ trên xuống", "Mula sa itaas");
        Add("TRASH_BIN", "TRASH BIN", "ÇÖP KUTUSU", "MÜLLEIMER", "POUBELLE", "PAPELERA", "CESTINO", "LIXEIRA", "AFVALBAK", "KOSZ", "МУСОРНЫЙ БАК", "СМІТНИК", "COȘ DE GUNOI", "ODPADKOVÝ KOŠ", "SOPTUNNA", "SKRALDESPAND", "SØPPELKASSE", "ROSKAKORI", "TEMPAT SAMPAH", "THÙNG RÁC", "BASURAHAN");
        Add("CAMERA_TUTORIAL", "Press C to change camera view. Use the mouse wheel to zoom in or out.", "Kamera açısını değiştirmek için C'ye bas. Uzaklaşmak veya yaklaşmak için fare tekerleğini kullan.", "Drücke C für die Kameraansicht. Zoome mit dem Mausrad.", "Appuyez sur C pour changer de vue. Utilisez la molette pour zoomer.", "Pulsa C para cambiar la cámara. Usa la rueda para acercar o alejar.", "Premi C per cambiare visuale. Usa la rotellina per lo zoom.", "Pressione C para mudar a câmera. Use a roda do mouse para zoom.", "Druk op C voor een ander camerastandpunt. Zoom met het muiswiel.", "Naciśnij C, aby zmienić kamerę. Użyj kółka myszy do zbliżania.", "Нажмите C, чтобы сменить камеру. Масштабируйте колёсиком мыши.", "Натисніть C, щоб змінити камеру. Масштабуйте коліщатком миші.", "Apasă C pentru a schimba camera. Folosește rotița pentru zoom.", "Stiskni C pro změnu kamery. Přibližuj kolečkem myši.", "Tryck C för att byta kameravy. Zooma med mushjulet.", "Tryk C for at skifte kamera. Zoom med musehjulet.", "Trykk C for å bytte kamera. Zoom med musehjulet.", "Vaihda kameraa C-näppäimellä. Zoomaa hiiren rullalla.", "Tekan C untuk mengganti kamera. Zoom dengan roda mouse.", "Nhấn C để đổi góc camera. Dùng con lăn chuột để thu phóng.", "Pindutin ang C para palitan ang camera. Gamitin ang mouse wheel para mag-zoom.");
        Add("CONTROL_HINT", "C: camera | E: interact | Mouse wheel: zoom", "C: kamera | E: etkileşim | Fare tekerleği: yakınlaş / uzaklaş", "C: Kamera | E: Interaktion | Mausrad: Zoom", "C : caméra | E : interagir | Molette : zoom", "C: cámara | E: interactuar | Rueda: zoom", "C: camera | E: interagisci | Rotellina: zoom", "C: câmera | E: interagir | Roda: zoom", "C: camera | E: actie | Muiswiel: zoom", "C: kamera | E: interakcja | Kółko: zoom", "C: камера | E: действие | Колесо: масштаб", "C: камера | E: взаємодія | Коліщатко: масштаб", "C: cameră | E: interacțiune | Rotiță: zoom", "C: kamera | E: interakce | Kolečko: zoom", "C: kamera | E: interagera | Mushjul: zoom", "C: kamera | E: interaktion | Musehjul: zoom", "C: kamera | E: samhandle | Musehjul: zoom", "C: kamera | E: toiminto | Hiiren rulla: zoom", "C: kamera | E: interaksi | Roda mouse: zoom", "C: camera | E: tương tác | Con lăn: thu phóng", "C: camera | E: kilos | Mouse wheel: zoom");

        Map("GELISTIRME", "UPGRADES"); Map("PERSONEL", "STAFF"); Map("AKVARYUMLAR", "AQUARIUMS");
        Map("DEKOR", "DECOR"); Map("BOYA", "PAINT"); Map("TEKNOLOJI", "TECHNOLOGY");
        Map("VERITABANI", "DATABASE"); Map("GECMIS", "HISTORY"); Map("YORUMLAR", "REVIEWS");
        Map("GUNU BITIR", "END_DAY"); Map("KAPAT", "CLOSE"); Map("ONCEKI", "PREVIOUS");
        Map("SONRAKI", "NEXT"); Map("KILITLI", "LOCKED"); Map("TUVALET ALANI", "TOILET_AREA");
        Map("GIRIS", "ENTRANCE"); Map("DENIZE", "SEA"); Map("ACIK", "OPEN"); Map("GEREKLI", "REQUIRED");
        Map("GUNLUK GOREVLER", "DAILY_TASKS");
        literalKeys.Sort(delegate(KeyValuePair<string, string> a, KeyValuePair<string, string> b) { return b.Key.Length.CompareTo(a.Key.Length); });

        // Full-sentence English fallbacks ensure newly routed game text never
        // remains Turkish while a non-Turkish language is active.
        En("AKVARYUM ISLETIM SISTEMI", "AQUARIUM MANAGEMENT SYSTEM");
        En("YENI GUNE BASLA", "START A NEW DAY"); En("Gelen Musteri", "Customers");
        En("Satilan Balik", "Fish sold"); En("Toplam Gelir", "Total income");
        En("Toplam Gider", "Total expenses"); En("NET KAR", "NET PROFIT");
        En("Bugun", "Today"); En("yorum", "reviews"); En("Ortalama", "Average"); En("yildiz", "stars");
        En("Henuz yorum yok. Musteri agirla!", "No reviews yet. Serve some customers!");
        En("Henuz gecmis yok. Ilk gununu bitir!", "No history yet. Finish your first day!");
        En("Musteri", "Customer"); En("Balik", "Fish"); En("Gelir", "Income"); En("Gider", "Expenses");
        En("Sayfa", "Page"); En("Kesfedilen", "Discovered"); En("SATIN AL", "BUY"); En("SAHIPSIN", "OWNED");
        En("GELISTIR", "UPGRADE"); En("MAKS", "MAX"); En("ALAN KAPALI", "AREA LOCKED");
        En("Dukkan", "Shop"); En("dukkan", "shop"); En("Magaza", "Shop"); En("magaza", "shop");
        En("Tuvalet", "Toilet"); En("tuvalet", "toilet"); En("Cop", "Trash"); En("cop", "trash");
        En("Deniz", "Sea"); En("deniz", "sea"); En("Temiz", "Clean"); En("temiz", "clean");
        En("GUN", "DAY"); En("Sv", "Lv"); En("gerekli", "required"); En("KAPALI", "CLOSED"); En("AC", "OPEN");
        En("Balik yakala", "Catch fish"); En("Balik sat", "Sell fish"); En("Cop at", "Dispose trash");
        En("TAMAM", "DONE"); En("GOREV TAMAM", "TASK COMPLETE");
        En("Cok pis burasi!", "This place is filthy!"); En("Tuvalet yok mu?!", "No toilet?!"); En("Balik yok!", "No fish!");
        En("HIRSIZ KACIYOR", "THIEF ESCAPING"); En("ALTIN BALIK", "GOLDEN FISH");
        En("TIKANDI", "CLOGGED"); En("KIRLI", "DIRTY"); En("TUVALETLER", "TOILETS");
        En("PARA", "MONEY"); En("MEMNUNIYET", "SATISFACTION"); En("SAAT", "TIME"); En("KIRLILIK", "POLLUTION");
        En("VITRIN AKVARYUMU", "DISPLAY AQUARIUM"); En("YENI ALAN", "NEW AREA"); En("EK BOLUM", "EXTRA SECTION");
        En("KILITLI ALAN", "LOCKED AREA"); En("KILITLI EK BOLUM", "LOCKED EXTRA SECTION");
        En("AC / KAPAT", "OPEN / CLOSE"); En("KAPAT (E)", "CLOSE (E)");
        En("YONETIM PANELINE HOSGELDIN", "WELCOME TO THE MANAGEMENT PANEL");
        En("Hadi dukkanina bir isim verelim!\nBu isim girisin uzerinde ve panelde gorunecek.", "Let's name your shop!\nThe name will appear above the entrance and in the panel.");
        En("Dukkan adi yaz", "Enter shop name"); En("BU ISMI KOY", "USE THIS NAME");
        En("Hayirli olsun", "Congratulations"); En("Seviye", "Level"); En("Calisan", "Employees");
        En("GUNLUK GIDER", "DAILY EXPENSE"); En("Maas", "Wages"); En("her gun sonu odenir", "paid at the end of each day");
        En("DEPO GEREKLI", "DEPOT REQUIRED"); En("TUVALET GEREKLI", "TOILET REQUIRED");
        En("ISE AL", "HIRE"); En("gun", "day"); En("Stok", "Stock"); En("Satis", "Sales");
        En("ISKELE GEREKLI", "DOCK REQUIRED"); En("ZEMIN RENGI", "FLOOR COLOR"); En("DUVAR RENGI", "WALL COLOR");
        En("Acilis", "Opening"); En("Kapanis", "Closing"); En("AKTIF", "ACTIVE"); En("teknoloji gerekli", "technology required");
        En("PUSULA", "COMPASS"); En("HARITA", "MAP"); En("GELISMIS NAVIGASYON", "ADVANCED NAVIGATION");
        En("UZAKTAN KONTROL", "REMOTE CONTROL"); En("OTOMATIK RADAR", "AUTO RADAR"); En("OTOMATIK DUKKAN", "AUTO SHOP");
        En("ANASAYFA", "HOME"); En("KUPALAR", "TROPHIES"); En("KUPA KOLEKSIYONU", "TROPHY COLLECTION");
        En("TOPLAM MUSTERI", "TOTAL CUSTOMERS"); En("YILDIZ ORTALAMASI", "AVERAGE RATING"); En("TOPLAM YORUM", "TOTAL REVIEWS");
        En("SIRKET PIYASA DEGERI", "COMPANY MARKET VALUE"); En("SIRKET SEVIYESI", "COMPANY LEVEL");
        En("ISMI DEGISTIR", "CHANGE NAME"); En("DUKKAN ISMINI DEGISTIR", "CHANGE SHOP NAME");
        En("KUPA KAZANDIN", "TROPHY UNLOCKED"); En("KILITLI KUPA", "LOCKED TROPHY"); En("Acildi", "Unlocked");
        En("Tum kupalarina Yonetim Paneli > Kupalar bolumunden ulasabilirsin.", "You can view all trophies in Management Panel > Trophies.");
        En("Deger; seviye, gelir, nakit, akvaryumlar, stok, gelistirmeler, teknoloji, dekor, personel ve memnuniyetten hesaplanir.", "Value is calculated from level, revenue, cash, aquariums, stock, upgrades, technology, decor, staff and satisfaction.");
        En("BALIK TASIMA", "FISH CAPACITY"); En("Akvaryumu", "Aquarium"); En("AKVARYUMU", "AQUARIUM");
        En("DUKKANI KAPAT", "CLOSE SHOP"); En("DUKKANI AC", "OPEN SHOP");
        En("Saat 22:00! Calisanlar paydos etti. Sabah 08:00'de geri gelecekler.", "It is 22:00! Staff have gone home and will return at 08:00.");
        En("OYUN BITTI - IFLAS", "GAME OVER - BANKRUPTCY");
        En("IFLAS UYARISI", "BANKRUPTCY WARNING"); En("iflas edeceksin", "you will go bankrupt");
        En("Jetski kirildi", "The jet ski is broken"); En("Jetski tamir edildi", "Jet ski repaired");
        En("tamir", "repair"); En("Hirsiz tuvaleti caldi", "The thief stole a toilet");

        En("CANTA", "BAG"); En("KOSU HIZI", "RUN SPEED"); En("YUZME HIZI", "SWIM SPEED");
        En("RADAR HIZI", "RADAR SPEED"); En("RADAR MENZILI", "RADAR RANGE"); En("BAHSIS", "TIP");
        En("MUSTERI HIZI", "CUSTOMER SPEED"); En("EKSTRA PARA", "EXTRA MONEY");
        En("+3 balik tasima kapasitesi", "+3 fish carrying capacity");
        En("+%8 kosu hizi", "+8% running speed"); En("+%10 yuzme hizi", "+10% swimming speed");
        En("+%12 deniz tarama hizi", "+12% sea scanning speed"); En("+0.5 deniz radar menzili", "+0.5 sea radar range");
        En("+%6 musteri bahsis sansi", "+6% customer tip chance"); En("+%10 musteri hareket hizi", "+10% customer movement speed");
        En("+%10 daha fazla satis geliri", "+10% additional sales income");
        En("Maks stok", "Max stock"); En("Kazanc", "Revenue"); En("Ozet", "Summary"); En("Yorum", "Reviews");
        En("tasima", "capacity"); En("hiz", "speed"); En("tarama", "scan"); En("menzil", "range"); En("bahsis", "tip"); En("satis", "sales");
        En("KASIYER", "CASHIER"); En("AVCI", "FISHER"); En("TASIYICI", "CARRIER"); En("TEMIZLIKCI", "JANITOR");
        En("DENIZ TEMIZLIGI", "SEA CLEANER"); En("TUVALETCI", "TOILET CLEANER"); En("GUVENLIK", "SECURITY"); En("SAHIL TEMIZLIGI", "BEACH CLEANER");
        En("Kasada calisir (maks 1)", "Works at the register (max 1)"); En("Denizden balik toplar", "Catches fish in the sea");
        En("Depodan tanklara tasir", "Carries fish from depot to tanks"); En("Magaza coplerini toplar", "Cleans shop waste");
        En("Denizdeki copleri toplar", "Cleans sea waste"); En("Tuvaletleri temizler", "Cleans toilets");
        En("Hirsizlari dover ve caldigini geri koyar", "Stops thieves and returns stolen goods"); En("Sahildeki copleri toplar", "Cleans beach waste");
        En("Palmiyeler", "Palm trees"); En("Fiskiye", "Fountain"); En("Balon Kemeri", "Balloon arch"); En("Balik Heykeli", "Fish statue");
        En("Kirmizi Hali", "Red carpet"); En("Fener Direkleri", "Lamp posts"); En("Ziplama Rampasi", "Jump ramp"); En("Iskele", "Dock");

        En("Kasaya birisi lazim", "Someone must operate the register"); En("Ilgilenen yok", "No one is helping me"); En("Bahsis", "Tip");
        En("DEPREM!!! Tanklari kontrol et", "EARTHQUAKE! Check the tanks");
        En("Kiyida ALTIN BALIK belirdi! Cok degerli, kacirma", "A GOLDEN FISH appeared near the shore! It is valuable—catch it");
        En("Comert musteri! Kasaya", "Generous customer! Added"); En("bagis birakti", "to the register");
        En("HIRSIZ! Yakala, esyalarini geri al", "THIEF! Catch them and recover your goods");
        En("Hirsizi yakaladin", "You caught the thief"); En("geri alindi", "recovered"); En("Balik geri alindi", "Fish recovered");
        En("Hirsiz eli bos yakalandi", "The thief was caught empty-handed"); En("Hirsiz kacti ve arkasindan guluyor", "The thief escaped, laughing");
        En("KOPEKBALIGI", "SHARK"); En("Sudan cik, KAC", "Get out of the water—RUN"); En("SALDIRDI", "ATTACKED"); En("kayip", "lost");
        En("GECE DENIZI TEHLIKELI", "THE SEA IS DANGEROUS AT NIGHT"); En("Sadece kopekbaliklari var, cikmadan saldirirlar", "Only sharks remain and they attack without warning");
        En("Dukkan ACILDI! Musteriler gelebilir", "Shop OPEN! Customers may enter"); En("Dukkan KAPATILDI. Musteri gelmeyecek", "Shop CLOSED. No customers will enter");
        En("Dukkan otomatik olarak ACILDI", "Shop opened automatically"); En("Dukkan otomatik olarak KAPATILDI", "Shop closed automatically");
        En("Harita alindi! Artik M tusuyla haritaya bakabilirsin", "Map purchased! Press M to view it");
        En("Giderleri tam odeyemedin! Personel mutsuz", "You could not cover all expenses! Staff are unhappy");
        En("basladi! Gunaydin", "started! Good morning");

        En("Tuvalet alani acildi! PC'den klozet ve lavabo ekleyebilirsin", "Toilet area unlocked! Add toilets and sinks from the PC");
        En("Depo acildi! Her turu buraya birakabilirsin", "Depot unlocked! You can store every species here");
        En("Sen yokken personel calisti", "Staff worked while you were away"); En("Dukkanin ek bolumu acildi", "The shop extension is unlocked");
        En("Yeni alan acildi! Duvarlar genisletildi", "New area unlocked! The walls were expanded");
        En("Jetski! Su ustunde cok hizlisin", "Jet ski! You are much faster on the water");
        En("Magaza kirleniyor! Musteriler rahatsiz olabilir", "The shop is getting dirty! Customers may be upset");
        En("Deniz cok kirlendi! Balik olumleri basladi, temizle", "The sea is heavily polluted! Fish are dying—clean it");
        En("Copler denize dagildi! Yayilmadan topla", "Waste scattered into the sea! Collect it before it spreads");
        En("Bir tuvalet TIKANDI! Basina git ve temizle (veya tuvaletci tut)", "A toilet is CLOGGED! Clean it or hire a toilet cleaner");
        En("Tuvalet temizlendi", "Toilet cleaned"); En("KIRIK! Tamir icin yaninda dur", "BROKEN! Stand nearby to repair");
        En("TAMIR EDILIYOR", "REPAIRING"); En("tanki tamir edildi", "tank repaired");

        En("Harika bir akvaryum, baliklar cok saglikli", "Wonderful aquarium; the fish are very healthy");
        En("Cocuklar bayildi, kesinlikle tekrar gelecegiz", "The children loved it; we will definitely return");
        En("Temiz ve guzel bir yer. Tavsiye ederim", "A clean and lovely place. Recommended");
        En("Cok cesit var, herkese oneririm", "A huge variety; I recommend it to everyone");
        En("Personel cok ilgili, mekan tertemiz", "Attentive staff and a spotless shop");
        En("Fena degil ama biraz kalabalikti", "Not bad, but it was a little crowded"); En("Guzel ama fiyatlar biraz yuksek", "Nice, but prices are a little high");
        En("Idare eder, gelistirilebilir", "It is okay, but could be improved"); En("Baliklar guzeldi ama sira uzundu", "The fish were nice, but the queue was long");
        En("Cok kirliydi, rahatsiz oldum", "It was very dirty and uncomfortable"); En("Tuvalet bile yoktu, berbat", "There was not even a toilet—awful");
        En("Kasada kimse yoktu, cok bekledim", "No one was at the register; I waited too long"); En("Balik yoktu, bos yere geldim", "There were no fish; the trip was pointless");

        En("TEBRIKLER", "CONGRATULATIONS"); En("Yeni tur acildi", "New species unlocked"); En("DEVAM", "CONTINUE");
        En("80 tur, sonsuz akvaryum", "80 species, unlimited aquariums"); En("HARITA  (M ile kapat)", "MAP  (press M to close)");

        // Species vocabulary.
        En("Altin", "Golden"); En("Mercan", "Coral"); En("Inci", "Pearl"); En("Zumrut", "Emerald");
        En("Yakut", "Ruby"); En("Safir", "Sapphire"); En("Gece", "Night"); En("Seker", "Candy");
        En("Firtina", "Storm"); En("Kral", "Royal"); En("Palyaco Baligi", "Clownfish");
        En("Denizati", "Seahorse"); En("Kaplumbaga", "Turtle"); En("Yengec", "Crab"); En("Istakoz", "Lobster");
        En("Katil Balina", "Orca"); En("Balon Baligi", "Pufferfish"); En("Kopekbaligi", "Shark");
        En("Vatoz", "Ray"); En("Somon", "Salmon"); En("Pirana", "Piranha"); En("Yayin", "Catfish"); En("Melek Baligi", "Angelfish");

        englishFallback.Sort(delegate(KeyValuePair<string, string> a, KeyValuePair<string, string> b) { return b.Key.Length.CompareTo(a.Key.Length); });
    }

    static void Add(string key, params string[] vals) { table[key] = vals; }
    static void Map(string source, string key) { literalKeys.Add(new KeyValuePair<string, string>(source, key)); }
    static void En(string source, string english) { englishFallback.Add(new KeyValuePair<string, string>(source, english)); }

    public static string T(string key)
    {
        Init();
        string[] vals;
        if (!table.TryGetValue(key, out vals)) return key;
        int l = Lang;
        return l < vals.Length && !string.IsNullOrEmpty(vals[l]) ? vals[l] : vals[0];
    }

    public static string Tr(string source)
    {
        if (string.IsNullOrEmpty(source) || Lang == 1) return source;
        Init();
        string result = source;
        for (int i = 0; i < literalKeys.Count; i++)
            if (result.Contains(literalKeys[i].Key)) result = result.Replace(literalKeys[i].Key, T(literalKeys[i].Value));
        for (int i = 0; i < englishFallback.Count; i++)
            if (result.Contains(englishFallback[i].Key)) result = result.Replace(englishFallback[i].Key, englishFallback[i].Value);
        return result;
    }

    public static void Clear()
    {
        table = null;
        literalKeys = null;
        englishFallback = null;
    }
}

// Tracks the original runtime text so later assignments and scene refreshes are
// translated as well, without translating an already translated value twice.
public class AutoLocalizeText : MonoBehaviour
{
    Text target;
    string source = "";
    string rendered = "";
    int renderedLanguage = -1;
    void Awake() { target = GetComponent<Text>(); }
    void LateUpdate()
    {
        if (target == null) return;
        bool changed = target.text != rendered;
        if (changed) source = target.text;
        int language = Loc.Lang;
        if (!changed && language == renderedLanguage) return;
        string next = Loc.Tr(source);
        if (target.text != next) target.text = next;
        rendered = next;
        renderedLanguage = language;
    }
}

public class AutoLocalizeTextMesh : MonoBehaviour
{
    TextMesh target;
    string source = "";
    string rendered = "";
    int renderedLanguage = -1;
    void Awake() { target = GetComponent<TextMesh>(); }
    void LateUpdate()
    {
        if (target == null) return;
        bool changed = target.text != rendered;
        if (changed) source = target.text;
        int language = Loc.Lang;
        if (!changed && language == renderedLanguage) return;
        string next = Loc.Tr(source);
        if (target.text != next) target.text = next;
        rendered = next;
        renderedLanguage = language;
    }
}
