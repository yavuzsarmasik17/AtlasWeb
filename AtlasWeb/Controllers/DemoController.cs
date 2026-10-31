
using Microsoft.AspNetCore.RateLimiting;
using AtlasWeb.Models;
using AtlasWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace AtlasWeb.Controllers
{
    public class DemoController : Controller
    {
        private readonly EmailService _emailService;
        private readonly ILogger<DemoController> _logger;

        public DemoController(
            EmailService emailService,
            ILogger<DemoController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(string? urun)
        {
            var model = new DemoTalepViewModel();

            var izinliUrunler = new[]
            {
                "ATLAS WMS",
                "ATLAS AI",
                "ATLAS Business"
            };

            if (!string.IsNullOrWhiteSpace(urun) &&
                izinliUrunler.Contains(
                    urun,
                    StringComparer.OrdinalIgnoreCase))
            {
                model.Urun = izinliUrunler.First(x =>
                    x.Equals(
                        urun,
                        StringComparison.OrdinalIgnoreCase));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("DemoTalepLimiti")]
        public async Task<IActionResult> Index(
            DemoTalepViewModel model)
        {
            // Botların doldurabileceği gizli alan
            if (!string.IsNullOrWhiteSpace(model.Website))
            {
                ModelState.AddModelError(
                    "",
                    "Talep gönderilemedi.");
            }

            var urunler = new[]
            {
                "ATLAS WMS",
                "ATLAS AI",
                "ATLAS Business"
            };

            if (!urunler.Contains(model.Urun))
            {
                ModelState.AddModelError(
                    nameof(model.Urun),
                    "Geçerli bir yazılım seçiniz.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _emailService.SendDemoEmailAsync(
                    model.AdSoyad,
                    model.Firma,
                    model.Telefon,
                    model.Email,
                    model.Urun,
                    model.Mesaj);

                // Başarı sayfasına yalnızca başarılı
                // gönderimden sonra erişilebilsin.
                TempData["DemoGonderildi"] = true;

                return RedirectToAction(nameof(Basarili));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Demo talebi e-posta gönderimi başarısız.");

                ModelState.AddModelError(
                    "",
                    "Talebiniz şu anda gönderilemedi. " +
                    "Lütfen daha sonra tekrar deneyiniz.");

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Basarili()
        {
            if (TempData["DemoGonderildi"] is not bool gonderildi ||
                !gonderildi)
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }
    }
}
