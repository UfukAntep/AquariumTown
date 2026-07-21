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

        Add("PLAYER_UPGRADES", "PLAYER UPGRADES", "KİŞİ GELİŞTİRME", "SPIELER-VERBESSERUNGEN", "AMÉLIORATIONS DU JOUEUR", "MEJORAS DEL JUGADOR", "POTENZIAMENTI GIOCATORE", "MELHORIAS DO JOGADOR", "SPELERUPGRADES", "ULEPSZENIA GRACZA", "УЛУЧШЕНИЯ ИГРОКА", "ПОКРАЩЕННЯ ГРАВЦЯ", "ÎMBUNĂTĂȚIRI JUCĂTOR", "VYLEPŠENÍ HRÁČE", "SPELARUPPGRADERINGAR", "SPILLEROPGRADERINGER", "SPILLEROPPGRADERINGER", "PELAAJAN PÄIVITYKSET", "PENINGKATAN PEMAIN", "NÂNG CẤP NGƯỜI CHƠI", "MGA UPGRADE NG MANLALARO");
        Add("SHOP_UPGRADES", "SHOP UPGRADES", "DÜKKAN GELİŞTİRME", "LADEN-VERBESSERUNGEN", "AMÉLIORATIONS DU MAGASIN", "MEJORAS DE LA TIENDA", "POTENZIAMENTI NEGOZIO", "MELHORIAS DA LOJA", "WINKELUPGRADES", "ULEPSZENIA SKLEPU", "УЛУЧШЕНИЯ МАГАЗИНА", "ПОКРАЩЕННЯ МАГАЗИНУ", "ÎMBUNĂTĂȚIRI MAGAZIN", "VYLEPŠENÍ OBCHODU", "BUTIKSUPPGRADERINGAR", "BUTIKSOPGRADERINGER", "BUTIKKOPPGRADERINGER", "KAUPAN PÄIVITYKSET", "PENINGKATAN TOKO", "NÂNG CẤP CỬA HÀNG", "MGA UPGRADE NG TINDAHAN");
        Add("SHOP_ENTRANCE", "SHOP ENTRANCE", "DÜKKAN GİRİŞİ", "LADENEINGANG", "ENTRÉE DU MAGASIN", "ENTRADA DE LA TIENDA", "INGRESSO DEL NEGOZIO", "ENTRADA DA LOJA", "WINKELINGANG", "WEJŚCIE DO SKLEPU", "ВХОД В МАГАЗИН", "ВХІД ДО МАГАЗИНУ", "INTRAREA ÎN MAGAZIN", "VCHOD DO OBCHODU", "BUTIKSINGÅNG", "BUTIKSINDGANG", "BUTIKKINNGANG", "KAUPAN SISÄÄNKÄYNTI", "PINTU MASUK TOKO", "LỐI VÀO CỬA HÀNG", "PASUKAN NG TINDAHAN");
        Add("SEA_WAY", "WAY TO THE SEA", "DENİZ YOLU", "WEG ZUM MEER", "CHEMIN VERS LA MER", "CAMINO AL MAR", "VIA PER IL MARE", "CAMINHO PARA O MAR", "WEG NAAR ZEE", "DROGA DO MORZA", "ПУТЬ К МОРЮ", "ШЛЯХ ДО МОРЯ", "DRUM SPRE MARE", "CESTA K MOŘI", "VÄGEN TILL HAVET", "VEJ TIL HAVET", "VEI TIL SJØEN", "TIE MERELLE", "JALAN KE LAUT", "ĐƯỜNG RA BIỂN", "DAAN PAPUNTANG DAGAT");

        Map("KISI GELISTIRME", "PLAYER_UPGRADES"); Map("DUKKAN GELISTIRME", "SHOP_UPGRADES");
        Map("DUKKAN GIRISI", "SHOP_ENTRANCE"); Map("DENIZ YOLU", "SEA_WAY");
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
        En("KONTROLLER", "CONTROLS"); En("KLAVYE", "KEYBOARD"); En("FARE", "MOUSE");
        En("SECILI", "SELECTED"); En("YUKARI OK", "UP ARROW"); En("ASAGI OK", "DOWN ARROW");
        En("SOL OK", "LEFT ARROW"); En("SAG OK", "RIGHT ARROW"); En("BASILI TUT", "HOLD");
        En("Hareket: WASD veya ok tuslari. Degistirmek icin mavi tusa, sonra yeni klavye tusuna bas.",
            "Movement: WASD or arrow keys. To rebind, press the blue button, then press a new keyboard key.");
        En("Sol ve sag tik atamalarini istedigin zaman yer degistirebilirsin.",
            "You can swap the left and right mouse assignments at any time.");
        En("HAREKET / YONLENDIRME", "MOVE / STEER"); En("Tepeden gorunumde git", "Move in top-down view");
        En("Musteri veya hirsiza vur", "Hit a customer or thief");
        En("SOL / SAG TIK YER DEGISTIR", "SWAP LEFT / RIGHT CLICK");
        En("Space ve fare yumrugu birlikte kullanilabilir. Ok yonleri her zaman aktiftir.",
            "Space and mouse punch can both be used. Arrow keys are always active.");
        En("MUSTERI SAATLERI", "CUSTOMER HOURS");
        En("Musteriler sabah 06:00 ile aksam 22:00 arasinda gelir.",
            "Customers visit between 06:00 and 22:00.");
        En("Gunu bitirmek icin yonetim masandaki bilgisayara gidebilirsin.",
            "You can use the computer at your management desk to end the day.");
        En("GUNU BITIRMEK ICIN YONETIM MASANA GIT", "GO TO YOUR MANAGEMENT DESK TO END THE DAY");
        En("YONETIM MASASINI KULLAN", "USE THE MANAGEMENT DESK");
        En("2. AKVARYUMU AC", "OPEN THE 2ND AQUARIUM");
        En("KLAVYE VE FARE DESTEGI", "KEYBOARD AND MOUSE SUPPORT");
        En("Oyunu hem KLAVYE hem de FARE ile oynayabilirsin.", "You can play with both KEYBOARD and MOUSE.");
        En("Klavye: WASD veya ok tuslariyla hareket et.", "Keyboard: Move with WASD or the arrow keys.");
        En("Fare: Sol tusa basili tutarak git.", "Mouse: Hold the left button to move.");
        En("DUKKANINI HAZIRLA", "PREPARE YOUR SHOP");
        En("Dukkanin su an KAPALI.", "Your shop is currently CLOSED.");
        En("Once yerdeki copleri toplayip disaridaki kutuya at,", "First collect the trash and put it in the bin outside,");
        En("sonra denize kos ve radarla balik yakala!", "then run to the sea and catch a fish with the radar!");
        En("Hazir olunca kapidaki tabeladan dukkani ac.", "When ready, open the shop using the sign by the door.");
        En("YONETIM MASASI ACILDI", "MANAGEMENT DESK UNLOCKED");
        En("Yonetim masasindaki bilgisayardan gelistirmeler, calisanlar, akvaryumlar, teknoloji ve dukkan ayarlarini yonetebilirsin.",
            "Use the management computer for upgrades, staff, aquariums, technology and shop settings.");
        En("Masayi gosteren isareti takip et ve bilgisayara E ile gir.", "Follow the marker and press E at the computer.");
        En("ILK GUNUNU BITIR", "END YOUR FIRST DAY");
        En("Saat 19:00 oldu. Hazir oldugunda gunu bitirmek icin yonetim masandaki bilgisayara git.",
            "It is 19:00. When ready, use the management computer to end the day.");
        En("CALISAN VARDIYASI", "STAFF SHIFTS");
        En("CALISANLAR PAYDOS ETTI", "STAFF HAVE CLOCKED OUT");
        En("GECE DENIZ TEHLIKELI", "THE SEA IS DANGEROUS AT NIGHT");
        En("Calisanlar her sabah 08:00'de ise gelir ve aksam 21:00'de evlerine gider.",
            "Staff arrive at 08:00 and go home at 21:00 every day.");
        En("Ise alirken bugunun maasi hemen odenir; sonraki maaslar gun sonunda kesilir.",
            "Today's wage is paid immediately when hiring; later wages are charged at day end.");
        En("Saat 21:00 oldu; calisanlar evlerine gidiyor ve her sabah 08:00'de yeniden geliyor.",
            "It is 21:00; staff are going home and will return at 08:00.");
        En("Saat 22:00'dan sonra deniz tehlikelidir.", "The sea is dangerous after 22:00.");
        En("Islerini 22:00'dan once bitir ve gunu bitirmeyi unutma.",
            "Finish your work before 22:00 and remember to end the day.");
        En("Gun sonu icin yonetim masandaki bilgisayara git.",
            "Go to the management computer to end the day.");
        En("GUN SONUNDA ODENECEK", "DUE AT DAY END");
        En("Yeni calisanin bugunku maasi ise alirken kesilir",
            "A new employee's wage for today is charged when hired");
        En("BUGUN", "TODAY");
        En("Calisanlar her sabah 08:00'de ise gelir ve aksam 21:00'de evlerine gider.",
            "Staff arrive at 08:00 and go home at 21:00 every day.");
        En("Mesai disinda calismazlar; gunluk maaslari gun sonunda odenir.",
            "They do not work off shift; daily wages are paid at day end.");
        En("Saat 21:00 oldu; calisanlar evlerine gidiyor.", "It is 21:00; staff are going home.");
        En("Her sabah 08:00'de yeniden ise gelecekler.", "They will return at 08:00 every morning.");
        En("Hirsiz baliklarinizi veya paralarinizi calabilir.", "A thief can steal your fish or money.");
        En("Onu kovala ve farenin SAG TIK tusuyla veya SPACE ile vur!",
            "Chase him and hit him with RIGHT CLICK or SPACE!");
        En("DENIZ KIRLENMEYE BASLADI", "THE SEA IS GETTING POLLUTED");
        En("Artik deniz zamanla kendi kendine kirlenebilir.", "The sea can now become polluted over time.");
        En("Deniz kirliligine dikkat et; temizlemezsen baliklar olebilir.",
            "Watch sea pollution; fish may die if you do not clean it.");
        En("MUSTERILERIN TUVALET IHTIYACI VAR", "CUSTOMERS NEED TOILETS");
        En("Musteriler artik tuvalet kullanmak istiyor.", "Customers now want to use toilets.");
        En("Tuvalet alanin yoksa satin al. Temiz bir tuvalet bulunmazsa musteri memnuniyeti duser.",
            "Buy a toilet area if you do not have one. Customer satisfaction falls when no clean toilet is available.");
        En("SAHIL TATILI BASLADI", "BEACH SEASON HAS STARTED");
        En("Tatilciler artik sahile ve denize geliyor.", "Vacationers now visit the beach and sea.");
        En("Sahili pisletebilir ve arkalarinda cop birakabilirler. Sahili ara ara temizlemeyi unutma.",
            "They can litter the beach. Remember to clean it regularly.");
        En("DIKKAT! Sopali bir hirsiz dukkana geliyor", "WARNING! A baton-wielding thief is entering the shop");
        En("TEHLIKE! Silahli bir hirsiz dukkana geliyor", "DANGER! An armed thief is entering the shop");
        En("DUKKAN KIRLILIGI", "SHOP POLLUTION"); En("DENIZ KIRLILIGI", "SEA POLLUTION");
        En("SAHIL KIRLILIGI", "BEACH POLLUTION"); En("CAN", "HEALTH");
        En("DUKKAN", "SHOP"); En("DENIZ", "SEA"); En("SAHIL", "BEACH");
        En("DUKKAN SAGLAMLASTIRMA", "SHOP REINFORCEMENT");
        En("YONETIM ODASI", "MANAGEMENT ROOM"); En("KAMERA IZLEME MASASI", "CAMERA MONITORING DESK");
        En("GUVENLIK KAMERALARI", "SECURITY CAMERAS"); En("JENERATOR", "GENERATOR");
        En("ELEKTRIK TEKNISYENI", "ELECTRICIAN"); En("JENERATOR GEREKLI", "GENERATOR REQUIRED");
        En("IZLEME MASASI GEREKLI", "MONITORING DESK REQUIRED");
        En("ELEKTRIK KESINTILERI BASLADI", "POWER OUTAGES HAVE STARTED");
        En("OKUL GEZILERI BASLADI", "SCHOOL TRIPS HAVE STARTED");
        En("FIRTINA MEVSIMI BASLADI", "STORM SEASON HAS STARTED");
        En("DUSMANLARIN ARTTI", "YOUR ENEMIES HAVE MULTIPLIED");
        En("OKUL GEZISI", "SCHOOL TRIP"); En("OGRETMEN", "TEACHER"); En("OGRENCI", "STUDENT");
        En("Kayit hazirlaniyor", "Preparing save data"); En("Oyun sahnesi yukleniyor", "Loading game scene");
        En("Oyun dunyasi hazirlaniyor", "Preparing game world"); En("Kayit ve sirket bilgileri okunuyor", "Reading save and company data");
        En("Dukkan ve sahil kuruluyor", "Building shop and beach"); En("Kamera ve arayuz hazirlaniyor", "Preparing camera and interface");
        En("Deniz canlilari ve olaylar hazirlaniyor", "Preparing sea life and events"); En("Dukkan bolumleri kontrol ediliyor", "Checking shop areas");
        En("Akvaryumlar yerlestiriliyor", "Placing aquariums"); En("Personel ise geliyor", "Staff are arriving");
        En("Baliklar akvaryumlara birakiliyor", "Moving fish into aquariums"); En("Son kontroller yapiliyor", "Running final checks");
        En("Hazir! Akvaryum kasabana hos geldin", "Ready! Welcome to your aquarium town"); En("Hazirlaniyor", "Preparing");
        En("PERSONEL EGITIMI", "STAFF TRAINING"); En("EGITIM", "TRAINING"); En("EGIT", "TRAIN"); En("ONCE ISE AL", "HIRE FIRST");
        En("KISI GELISTIRME", "PLAYER UPGRADES"); En("DUKKAN GELISTIRME", "SHOP UPGRADES");
        En("DUKKAN GIRISI", "SHOP ENTRANCE"); En("DENIZ YOLU", "WAY TO THE SEA");
        En("DEPREM DAYANIKLILIGI", "EARTHQUAKE RESISTANCE"); En("HIRSIZ ALARMI", "THIEF ALARM");
        En("HIJYEN SISTEMI", "HYGIENE SYSTEM"); En("ENERJI VERIMLILIGI", "ENERGY EFFICIENCY");
        En("BOYA ATOLYESI", "PAINT WORKSHOP"); En("ZEMIN RENKLERI", "FLOOR COLORS"); En("DUVAR RENKLERI", "WALL COLORS");
        En("ON IZLE", "PREVIEW"); En("DEGISIKLIK YOK", "NO CHANGES"); En("SEC", "SELECT");
        En("DUKKAN ALTYAPISI", "SHOP INFRASTRUCTURE"); En("Kalici koruma ve isletme gelistirmeleri", "Permanent protection and business upgrades");
        En("Her seviye tanklarin depremde kirilmama sansini %12 artirir.", "Each level gives tanks +12% chance to resist earthquakes.");
        En("Her seviye hirsizi yakalamak icin 2 saniye daha kazandirir.", "Each level adds 2 seconds to catch a thief.");
        En("Her seviye magazada kaka birakilma ihtimalini %10 azaltir.", "Each level reduces indoor poop chance by 10%.");
        En("Her seviye tuvaletlerin gunluk giderini %8 azaltir.", "Each level reduces daily toilet costs by 8%.");
        En("Once sec, sonra on izle veya satin al", "Select first, then preview or buy");
        En("Zemin: mevcut", "Floor: current"); En("Duvar: mevcut", "Wall: current"); En("Toplam", "Total");
        En("BOYA ON IZLEMESI - PC'ye donmek icin herhangi bir tusa bas", "PAINT PREVIEW - press any key to return to the PC");
        En("On izleme kapandi; satin almazsan eski renkler korunur.", "Preview closed; the old colors stay unless you buy.");
        En("YONETIM", "MANAGEMENT"); En("Yonetim PC'sini Ac", "Open management PC");
        En("Odeme noktasinda bekle", "Wait at checkout"); En("Bekleyen musteri yok.", "No customers are waiting.");
        En("Odeme aliniyor; kasada bekle.", "Payment is processing; remain at the checkout.");
        En("Bir seyler ters gidiyor... YER SARSILIYOR!", "Something is wrong... THE GROUND IS SHAKING!");
        En("Zemin hazirlaniyor", "Preparing the ground"); En("Dukkan kuruluyor", "Building the shop");
        En("Giris ve iskele kuruluyor", "Building the entrance and dock");
        En("YUKLENIYOR", "LOADING"); En("OYUN CALISIYOR", "GAME IS RUNNING");
        En("80 TUR", "80 SPECIES"); En("YAKALA", "CATCH"); En("BUYUT", "GROW");
        En("AKVARYUM IMPARATORLUGUNU KUR", "BUILD YOUR AQUARIUM EMPIRE");
        En("DENIZIN YILDIZLARI", "STARS OF THE SEA"); En("DENIZ CANLISI", "SEA CREATURE");
        En("COPLERI TOPLA", "COLLECT TRASH"); En("COP KUTUSUNA GOTUR", "TAKE TO TRASH BIN");
        En("COP KUTUSU", "TRASH BIN");
        En("MAKS SEVIYE", "MAX LEVEL"); En("YUMRUK", "PUNCH"); En("ETKILESIM", "INTERACT");
        En("ILERI", "FORWARD"); En("GERI", "BACK"); En("SOLA", "LEFT"); En("SAGA", "RIGHT");
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
        En("DEPAR", "SPRINT");
        En("Shift basiliyken hizli kos; her seviye depari hizlandirir",
            "Run faster while holding Shift; each level makes sprinting faster");
        En("Shift henuz etkisiz", "Shift is not active yet");
        En("DAHA HIZLI BUYU", "GROW FASTER");
        En("Actigin her yeni akvaryum seni bir sonraki seviyeye daha hizli tasir. Seviye atladikca yeni olaylar ve oyun mekanikleri acilir.",
            "Every new aquarium moves you toward the next level faster. New events and mechanics unlock as you level up.");
        En("Hizli buyumek icin Yonetim Paneli'nden calisan ise al, akvaryum kapasitesini gelistir ve stoklarini dolu tut.",
            "To grow quickly, hire staff, upgrade aquarium capacity and keep stock full from the Management Panel.");
        En("DUKKANI ACMA ZAMANI", "TIME TO OPEN THE SHOP");
        En("Ilk baligin akvaryumda! Musterilerin gelebilmesi icin kapidaki AC / KAPAT tabelasindan dukkani ac.",
            "Your first fish is in the aquarium! Open the shop from the OPEN / CLOSE sign so customers can enter.");
        En("Tabelayi gosteren isareti takip et ve yaninda E'ye bas.",
            "Follow the marker to the sign and press E beside it.");
        En("DUKKANI AC", "OPEN THE SHOP");
        En("Oyunu hem KLAVYE hem de FARE ile oynayabilirsin.", "You can play with both KEYBOARD and MOUSE.");
        En("Klavye: WASD veya ok tuslariyla hareket et.", "Keyboard: Move with WASD or the arrow keys.");
        En("Fare: Sol tusa basili tutarak git; sag tikla vur.", "Mouse: Hold left click to move; right click to punch.");
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
