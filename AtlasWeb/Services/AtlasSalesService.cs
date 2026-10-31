using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AtlasWeb.Services
{
    public class AtlasSalesService
    {
        private static readonly CultureInfo Turkish = new CultureInfo("tr-TR");

        public string GetResponse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "Size nasıl yardımcı olabilirim?";

            string q = message.Trim().ToLower(Turkish);
            var parts = new List<string>();

            bool packageRequest = IsPackageRecommendation(q);
            bool mobile = Has(q, "telefon", "tablet", "mobil", "cep telefonu");
            bool ocr = Has(q, "ocr", "pdf", "beyanname oku", "beyannameyi oku", "otomatik oku", "belge oku");
            bool price = Has(q, "fiyat", "ücret", "maliyet", "kaç para", "abonelik");
            bool demo = Has(q, "demo", "görüşme", "iletişim", "teklif al", "programı görmek");

            if (packageRequest)
                parts.Add(RecommendPackage(q));
            else if (price)
                parts.Add("ATLAS WMS aylık taslak fiyatları: Mini 4.900 TL, Başlangıç 7.900 TL, Profesyonel 14.900 TL, Business 24.900 TL + KDV. Kurumsal paket özel tekliflidir.");

            if (mobile)
                parts.Add("ATLAS WMS web tabanlıdır. Mobil kullanım kapsamını demo sırasında gösterebiliriz.");


            if (ocr)
                parts.Add("Evet, ATLAS WMS antrepo beyannamelerini PDF üzerinden OCR teknolojisiyle okuyabilir. Beyanname üzerindeki firma, kap sayısı, mal tanımı ve ilgili bilgiler otomatik olarak mal kabul ekranına aktarılır.");


            if (Has(q, "sunucu", "server", "kurulum", "hosting", "bulut"))
                parts.Add("Kurulum ve sunucu seçenekleri teknik ihtiyaçlarınıza göre değerlendirilir.");

            if (Has(q, "güvenlik", "yetki", "şifre", "rol"))
                parts.Add("Kullanıcı giriş ve yetkilendirme özellikleri bulunur; kapsamı demo sırasında netleştirebiliriz.");

            if (Has(q, "rapor", "excel", "istatistik"))
                parts.Add("Operasyonel raporlama özellikleri bulunur. İhtiyacınız olan raporları birlikte değerlendirebiliriz.");

            if (Has(q, "muhasebe", "cari", "borç", "alacak", "fatura", "tahsilat"))
                parts.Add("Cari hesap, muhasebe hareketleri ve ilgili raporları takip edebilirsiniz.");

            if (Has(q, "ardiye", "elleçleme", "mesai", "kaldığı gün"))
                parts.Add("Ardiye süresi ve ilgili operasyonel hizmetleri takip edebilirsiniz.");

            if (Has(q, "mal kabul", "beyanname", "gümrük", "konteyner", "kap sayısı") && !ocr)
                parts.Add("Mal kabulde beyanname, firma, kap, ağırlık, konteyner ve araç bilgilerini yönetebilirsiniz.");

            if (Has(q, "mal çıkış", "sevkiyat", "parçalı çıkış"))
                parts.Add("Komple ve parçalı mal çıkış süreçlerini takip edebilirsiniz.");

            if (Has(q, "stok", "depo adres", "lokasyon", "palet", "barkod", "raf"))
                parts.Add("Stok, adres ve yük hareketlerini tek sistemden takip edebilirsiniz.");

            if (Has(q, "yapay zeka", "yapay zekâ", "atlas ai"))
                parts.Add("ATLAS AI belge ve iş süreçleri için geliştirilmektedir; bazı özellikler test aşamasındadır.");

            if (Has(q, "atlas business") || (Has(q, "business") && !packageRequest && !price))
                parts.Add("ATLAS Business, işletme süreçlerini tek platformda yönetmeye yönelik çözümümüzdür.");

            if (parts.Count == 0 && Has(q, "wms", "antrepo", "depo yönetimi"))
                parts.Add("ATLAS WMS; mal kabul, mal çıkış, stok, adres, ardiye ve muhasebe süreçlerini bir araya getirir.");

            if (parts.Count == 0 && Has(q, "merhaba", "selam", "iyi günler"))
                parts.Add("Merhaba! ATLAS SOFT satış danışmanıyım. Ürünler, fiyatlar ve demo hakkında yardımcı olabilirim.");

            if (demo)
                parts.Add("Demo Talep Et bölümünden bize ulaşabilirsiniz.");

            if (parts.Count == 0)
                return "Bu konuda doğrulanmış bir yanıtım yok. Ürünlerimiz, fiyatlandırma veya demo için yardımcı olabilirim.";

            // Aynı mesajdaki farklı konulara kısa, okunabilir cevap ver.
            // Çok uzun cevaplardan kaçınmak için en fazla dört bölüm göster.
            if (parts.Count > 4)
                parts = parts.GetRange(0, 4);

            return string.Join("\n\n", parts);
        }

        public bool IsPackageRecommendation(string question)
        {
            question = (question ?? "").ToLower(Turkish);
            return Has(question, "hangi paket", "paket öner", "paket tavsiye",
                "bana uygun", "hangi lisans", "kaç kullanıcı", "kaç personel", "kaç depo") ||
                Regex.IsMatch(question,
                    @"\d+\s*(?:adet\s+)?(?:depo|antrepo|kullanıcı|personel|çalışan)");
        }

        private static bool Has(string text, params string[] words)
        {
            foreach (string word in words)
                if (text.Contains(word)) return true;
            return false;
        }

        private string RecommendPackage(string question)
        {
            int? depots = ExtractCount(question,
                @"(\d+)\s*(?:adet\s+)?(?:depo|antrepo)");
            int? users = ExtractCount(question,
                @"(\d+)\s*(?:adet\s+)?(?:kullanıcı|personel|çalışan)");

            if (!depots.HasValue && !users.HasValue)
                return "Size uygun paketi önermem için depo ve kullanıcı sayınızı yazar mısınız? Örnek: 2 depo, 10 kullanıcı.";
            if (!depots.HasValue)
                return "Kaç deponuz veya antreponuz var?";
            if (!users.HasValue)
                return "Kaç personel sistemi kullanacak?";

            int d = depots.Value, u = users.Value;
            if (d < 1 || u < 1)
                return "Depo ve kullanıcı sayısı en az 1 olmalıdır.";

            string name, price, capacity;
            if (d <= 1 && u <= 2) { name = "Mini"; price = "4.900 TL"; capacity = "1 depo / 2 kullanıcı"; }
            else if (d <= 1 && u <= 5) { name = "Başlangıç"; price = "7.900 TL"; capacity = "1 depo / 5 kullanıcı"; }
            else if (d <= 2 && u <= 10) { name = "Profesyonel"; price = "14.900 TL"; capacity = "2 depo / 10 kullanıcı"; }
            else if (d <= 3 && u <= 20) { name = "Business"; price = "24.900 TL"; capacity = "3 depo / 20 kullanıcı"; }
            else return $"{d} depo ve {u} kullanıcı için Kurumsal Paket öneriyoruz. Fiyat ve kapsam için özel teklif hazırlayabiliriz.";

            return $"{d} depo ve {u} kullanıcı için {name} Paket uygun görünüyor ({capacity}). Aylık taslak fiyat: {price} + KDV.";
        }

        private static int? ExtractCount(string question, string pattern)
        {
            var match = Regex.Match(question, pattern, RegexOptions.IgnoreCase);
            if (!match.Success) return null;
            return int.TryParse(match.Groups[1].Value, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int value) && value > 0 ? value : (int?)null;
        }
    }
}
