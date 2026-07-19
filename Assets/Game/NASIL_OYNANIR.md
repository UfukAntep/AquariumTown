# Aquarium Town — Rehber

Unity'de **Play**'e bas → **giriş ekranı** gelir: Devam Et / Yeni Oyun / Çıkış.
Oyun her 10 saniyede ve çıkışta otomatik kaydeder; kaldığın yerden devam edersin.

## Kontroller
- **WASD / Oklar** veya fare sürükleme (tepeden bakış).
- **U** → TPS kamera: fare ile yön, WASD kameraya göre hareket.
- **E** → etkileşim (kasa PC'si, dükkan aç/kapat). Sağ altta sarı kutu çıkınca E'ye bas.
- **Esc** → Duraklat: Devam / Ses aç-kapa / Kaydet ve Çık.

## Temel Döngü
1. GİRİŞ kapısından müşteriler gelir (dükkan açıksa). Sağdaki DENİZE kemerinden çık, iskeleden zıpla veya yürü.
2. **Dev denizde 80 tür** var. Kilitli türlerin üstünde "SvN" rozeti görünür — o seviyeye gelince yakalayabilirsin. Radar dolunca balık yakalanır; balıklar kaçar.
3. Balıkları **kendi türünün akvaryumuna** bırak (limit yok!). Fazlası sayı olarak birikir.
4. Müşteri balığı fanusla kasaya taşır. **Kasada operatör olmalı** (sen ya da kasiyer). Ödeme → paralar tek tek "tık tık" dizilir → üstünden geç, topla.
5. **Gölge plot** üzerinde durup parayı akıt → yeni tür + **SEVİYE** + kutlama ekranı + vitrin akvaryumuna yeni balık.

## Kasa PC'si (E)
Zoom animasyonuyla açılır. 5 bölüm:
- **GELİŞTİRME**: çanta, koşu/yüzme hızı, radar hızı/menzili, bahşiş, müşteri hızı, ekstra para.
- **PERSONEL**: Kasiyer, Avcı, Taşıyıcı, Temizlikçi, Deniz Temizliği, Tuvaletçi — **aynı rolden birden fazla** alınabilir, her alım pahalanır. Tuvalet ünitesi de buradan.
- **AKVARYUMLAR**: her tankı 5 seviyeye kadar geliştir → içi süslenir + satış fiyatı +%15/seviye.
- **DEKOR**: palmiye, fıskiye, balon kemeri, heykel, halı, fenerler.
- **BOYA**: zemin ve duvar rengi.

## Kirlilik ve Memnuniyet
- Müşteriler altına **kaka** bırakır; çöpken denize girersen etrafa dağılır ve **balıkları öldürür**. Deniz kendi kendine de kirlenir — yüzerek topla, çöp kutusuna at (ya da personel tut).
- Çöp çoksa müşterilerin ~%50'si "Çok pis!" deyip alışverişsiz gider.
- **Memnuniyet %**si satış fiyatını etkiler (sağ üstte; düşükken kırmızı). Balık yok / kasada uzun bekleme → müşteri balığı geri bırakıp gider, memnuniyet düşer.
- **Tuvaletler** (sol üst bölge, Seviye 6): kaka oranını düşürür; kirlenince Tuvaletçi lazım.

## Olaylar
- **Hırsız**: normal müşteri gibi girer, içeride siyaha bürünür; kasadaki parayı ya da bir balığı kapıp kaçar. 10 sn içinde yakala → geri al; kaçarsa kahkaha atar.
- **Köpekbalığı**: denizde saldırır → taşınan balıklar + seviyene göre para gider.
- **Deprem**: tanklar kırılır, balıklar yerde çırpınır → yanında dur, tamir parası aksın; 40 sn içinde tamir etmezsen ölürler.
- **Altın balık** (bonus para) ve **para yağmuru** da var.

## Saat ve Dükkan
Sağ üstte saat (1 sn = 1 dk); gece ışık kısılır. Kapıdaki AÇ/KAPAT tabelasında E → dükkanı kapat/aç.

## Test / Hile (DevCheats.cs)
F1 +1K$, F2 +100K$, F3 seviye+1 (kutlamalı), F4 seviye+5, F5 memnuniyet+20,
F6 hırsız, F7 köpekbalığı, F8 deprem, F9 altın balık, F10 para yağmuru, F12 kaydı sil.
Inspector'dan da `DevCheats.setMoneyTo` / `setLevelTo` yazabilirsin.
