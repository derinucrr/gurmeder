# images/

Bu klasördeki fotoğraflar **gerçek yemek fotoğraflarıdır** — otomatik
üretilmemiş, bir arama motorundan rastgele çekilmemiştir. Sekiz turda,
toplam **77 gerçek dosya** eklendi (bu sayıya ana sayfa bannerı için
ayrı eklenen `ana-sayfa-banner.jpg` dahil değildir — o bir tarif
fotoğrafı değildir, bkz. bu dosyanın sonundaki not):

- İlk 7'si: bir GitHub portföy projesinin yükleme arşivinden, tek tek
  incelenip doğrulanarak seçildi (23 dosyalık arşivde 2 kişinin gerçek
  yüz fotoğrafı ve 2 yazılım diyagramı vardı — bunlar kesinlikle
  kullanılmadı).
- Sonraki 20'si, ardından 19'u, ardından 17'si: kullanıcı tarafından, tarif
  adlarıyla birebir eşleşen dosya adlarıyla (`karniyarik.jpg`, `humus.jpg`,
  `lahmacun.jpg`, `tiramisu.jpg`, vb.) doğrudan yüklendi; her biri hangi
  tarife ait olduğunu doğrulamak için görsel olarak incelendi (ör.
  Karnıyarık'ın görünür kıyma dolgusu ile etsiz İmam Bayıldı'dan ayırt
  edilmesi, Kuru Fasulye'nin sulu güveç hâli ile Piyaz'ın taze soğanlı
  salata hâlinin karıştırılmaması, ya da "şehriye çorbası" adıyla yüklenen
  ama görünürde hiç şehriye/tel makarna olmayan, aksine mantar dilimleriyle
  süslenmiş bir fotoğrafın aslında **Mantar Çorbası**'na ait olduğunun fark
  edilip doğru tarife yönlendirilmesi gibi) ve orijinal boyutlarından
  ortalama %47 daha küçük olacak şekilde sıkıştırıldı (900px uzun kenar,
  JPEG kalite 80). Bu turlarda ayrıca üç tarif (Mantı, Lahmacun,
  Zerdeçallı Altın Süt) daha önce ya Unsplash URL'si ya da dolaylı bir
  temsil (zerdeçal kökü/tozu fotoğrafı, içeceğin kendisi değil) kullanıyordu;
  gerçek/daha doğrudan yerel dosyalar geldiğinde bunlar değiştirildi. İki
  fotoğraf ise (bir "şalgam suyu" adıyla gelen ama görünürde pancar
  gösteren, bir de net şekilde yumurta içermeyen bir "sucuklu yumurta")
  tarifle gerçekten eşleşmediği için **hiçbir tarife atanmadı**.
- Beşinci turda 11 dosya daha yüklendi. 5'i zaten iyi bir fotoğrafa sahip
  tariflerle aynı isimde geldiği için değiştirilmeden bırakıldı; Baklava
  önceden kullandığı Unsplash URL'sinden yerel dosyaya yükseltildi; kalan
  4'ü gerçek eksikleri doldurdu. Bunlardan ikisi özellikle dikkat çekici:
  bir önceki turda "içerik uyuşmazlığı" nedeniyle fotoğrafsız bırakılan
  **Şehriye Çorbası** ve **Sucuklu Yumurta** için bu kez doğru fotoğraflar
  geldi — Şehriye Çorbası'nda artık gerçekten tel şehriye görünüyor,
  Sucuklu Yumurta'da artık yumurta net şekilde görünüyor. Gelen ikinci
  `salgam-suyu.jpg` dosyası ise yine pancar gösterdiğinden aynı gerekçeyle
  tekrar dışlandı.
- Altıncı turda, ayrı bir HTML/JS mini-sitesinden yapıştırılan 6 tarifle
  birlikte 7 fotoğraf daha geldi. `fırında_sutlac.JPG` zaten iyi bir
  fotoğrafa sahip Fırın Sütlaç ile aynı isimde geldiği için değiştirilmedi.
  `zeytinyagli-yaprak-sarma.JPG` **görünür bir "Creative Market" filigranı
  taşıdığı için kullanılmadı** — satın alınmamış, lisanssız bir stok
  fotoğraf önizlemesiydi; ayrıca bu tarif zaten mevcuttu. Kalan 5 dosya,
  yapıştırılan metinden gelen 5 yeni tarife doğru şekilde atandı.
- Yedinci turda, kullanıcı 7 tarifin (Zeytinyağlı Yaprak Sarma, Su
  Böreği, Revani, Peynirli Sigara Böreği, İçli Köfte, Tas Kebabı,
  İskender Kebap) dış Unsplash URL'lerini yerel dosyalarla **değiştirmek**
  istedi. İncelemede **iki dosyada görünür filigran/marka tespit edildi**:
  `zeytinyagli-yaprak-sarma.JPG` yine aynı "Creative Market" filigranını
  taşıyordu (bir önceki turda reddedilen dosyayla aynı sorun — kullanıcı
  muhtemelen aynı dosyayı fark etmeden tekrar yüklemişti); `iskender-kebap.JPG`
  ise sol üst köşede "I ❤ FOOD" marka/logo yazısı içeriyordu. **İkisi de
  kullanılmadı**, bu iki tarifin mevcut, çalışan Unsplash fotoğrafları
  değiştirilmeden bırakıldı — hiç fotoğrafsız kalmaktansa (ya da lisanssız
  bir görsel kullanmaktansa) var olan doğru fotoğrafı korumak tercih
  edildi. Kalan 5 dosya sorunsuzca uygulandı.
- Sekizinci turda, kullanıcı **üçüncü kez** "Zeytinyağlı Yaprak Sarma"
  için bir fotoğraf yükledi. Bu kez dosya hash'i öncekiyle karşılaştırılıp
  gerçekten farklı bir dosya olduğu doğrulandı, görsel olarak dikkatle
  yeniden incelendi ve **hiçbir filigran veya marka bulunamadı** — temiz,
  lisanslı görünümlü bir fotoğraftı. Kabul edilip mevcut Unsplash URL'sinin
  yerine yerel dosya olarak eklendi.

> **Not:** Toplamda 88 tarifin gerçek fotoğrafı var — bu 77'si burada
> yerel dosya olarak, kalan 11'i `data/recipes.json` içinde doğrudan
> Unsplash URL'si olarak tutuluyor. Tam liste ve fotoğraf seçim/eleme
> kuralları için ana `README.md`'deki "Fotoğraflar" bölümüne bakın.
>
> `ana-sayfa-banner.jpg` bu sayıma dahil değildir — bir tarif fotoğrafı
> değil, ana sayfadaki dekoratif baharat bannerı için kullanılan ayrı bir
> görseldir (bkz. ana `README.md`'deki "Ana sayfa tasarımı" bölümü).

## Kaynak

Fotoğraflar, bu projeye yüklenen bir arşivden (2019 tarihli, kaynağı açık
bir GitHub portföy projesinin "yükleme testi" klasörü) geldi. Arşivde 23
dosya vardı; bunlar tek tek incelendi:

- **Baytesbüyt kontrolü**: checksum karşılaştırmasıyla arşivdeki
  tekrarlanan (aynı dosyanın farklı isimlerle birden çok kez yüklenmiş
  hâli) dosyalar elendi — 23 dosya, gerçekte yalnızca 11 **benzersiz**
  görsele karşılık geliyordu.
- **İçerik kontrolü**: 11 benzersiz görselin her biri tek tek görüntülenip
  doğrulandı. Bunlardan **4 tanesi yemekle ilgisizdi** (iki kişinin gerçek,
  tanımlanabilir yüz fotoğrafı + iki yazılım mimarisi diyagramı — muhtemelen
  geliştiricinin dosya yükleme özelliğini test ederken eklediği rastgele
  dosyalar) ve **kesinlikle kullanılmadı**. Kalan **7 tanesi** gerçek,
  profesyonel kalitede yemek/malzeme fotoğrafıydı — bu 7'si projeye alındı.

## Kullanılan 7 fotoğraf ve eşleştiği tarif

| Dosya | Tarif |
| --- | --- |
| `brusetta-domates-krem-peynir.jpg` | Bruschetta (Domates ve Krem Peynirli) |
| `etli-taco.jpg` | Etli Taco (Limonlu) |
| `cilekli-yulaf-kavanozu.jpg` | Çilekli Yulaf |
| `elmali-somon.jpg` | Elmalı Somon |
| `zerdecalli-altin-sut.jpg` | Zerdeçallı Altın Süt |
| `az-pisirilmis-ton-baligi.jpg` | Az Pişirilmiş Ton Balığı |
| `taze-istiridye.jpg` | Taze İstiridye (Limon ve Mignonette Soslu) |

Her tarif, fotoğrafta gerçekten görünen yemeğe göre yazıldı (fotoğraf önce,
tarif sonra) — yani fotoğraf ile tarif içeriği (malzemeler, teknik) kasıtlı
olarak birbiriyle tutarlıdır, rastgele eşleştirilmemiştir.

## Lisans notu

Fotoğraflarda görünür bir filigran veya EXIF telif bilgisi yok; büyük
olasılıkla orijinal geliştirici tarafından ücretsiz stok fotoğraf
kaynaklarından (Unsplash/Pexels tarzı) alınıp bir demo projede kullanılmıştı.
Bu proje kişisel/portföy amaçlı kullanım için makul görülüyor, ancak siteyi
**herkese açık veya ticari olarak yayınlamayı** düşünüyorsanız, bu 7
fotoğrafın lisansını kendiniz doğrulamanız veya kendi çektiğiniz/lisanslı
fotoğraflarla değiştirmeniz önerilir.

## Yeni fotoğraf eklemek

`data/recipes.json` içindeki herhangi bir tarifin `image` alanı şu an
`null`. Bir tarife gerçek fotoğraf eklemek için:

1. Fotoğrafı bu klasöre, açıklayıcı bir dosya adıyla (Türkçe karakter ve
   boşluk kullanmadan, örn. `mercimek-corbasi.jpg`) kaydedin.
2. `data/recipes.json`'da ilgili tarifin `"image": null` satırını
   `"image": "images/mercimek-corbasi.jpg"` olarak güncelleyin.

Başka hiçbir kod değişikliği gerekmez — kart ve detay sayfası otomatik
olarak yeni fotoğrafı kullanır.
