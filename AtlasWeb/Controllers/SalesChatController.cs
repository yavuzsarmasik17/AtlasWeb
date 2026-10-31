using AtlasWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AtlasWeb.Controllers
{
    [ApiController]
    [Route("api/sales-chat")]
    public class SalesChatController : ControllerBase
    {
        private readonly AtlasSalesService _salesService;
        private static readonly CultureInfo Turkish = new CultureInfo("tr-TR");

        public SalesChatController(AtlasSalesService salesService)
        {
            _salesService = salesService;
        }

        [HttpPost("ask")]
        [EnableRateLimiting("SalesChatLimiti")]
        [RequestSizeLimit(4096)]
        public IActionResult Ask([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Lütfen bir mesaj yazın." });

            if (request.Message.Length > 500)
                return BadRequest(new { error = "Mesaj en fazla 500 karakter olabilir." });

            string message = request.Message.Trim();
            string question = message.ToLower(Turkish);

            if (question == "yeni hesaplama" || question == "yeni paket hesapla" ||
                question == "baştan başla" || question == "sıfırla")
            {
                HttpContext.Session.Remove("AtlasDepotCount");
                HttpContext.Session.Remove("AtlasUserCount");
                HttpContext.Session.SetString("AtlasPackageFlow", "1");
                return Ok(new
                {
                    reply = "Yeni hesaplama başladı. Kaç deponuz veya antreponuz var?",
                    packageInfo = (object)null
                });
            }

            int? depotCount = HttpContext.Session.GetInt32("AtlasDepotCount");
            int? userCount = HttpContext.Session.GetInt32("AtlasUserCount");
            bool flowActive = HttpContext.Session.GetString("AtlasPackageFlow") == "1";

            // Tek başına gönderilen sayı, yalnızca devam eden paket konuşmasında yorumlanır.
            if (flowActive && int.TryParse(question, out int number) && number > 0)
            {
                if (!depotCount.HasValue) question = number + " depo";
                else if (!userCount.HasValue) question = number + " kullanıcı";
                message = question;
            }

            var depotMatch = Regex.Match(question,
                @"(\d+)\s*(?:adet\s+)?(?:depo|antrepo)", RegexOptions.IgnoreCase);
            var userMatch = Regex.Match(question,
                @"(\d+)\s*(?:adet\s+)?(?:kullanıcı|personel|çalışan)", RegexOptions.IgnoreCase);

            if (depotMatch.Success && int.TryParse(depotMatch.Groups[1].Value, out int depots) && depots > 0)
            {
                depotCount = depots;
                HttpContext.Session.SetInt32("AtlasDepotCount", depots);
            }

            if (userMatch.Success && int.TryParse(userMatch.Groups[1].Value, out int users) && users > 0)
            {
                userCount = users;
                HttpContext.Session.SetInt32("AtlasUserCount", users);
            }

            bool wantsRecommendation = _salesService.IsPackageRecommendation(question) ||
                depotMatch.Success || userMatch.Success;

            if (wantsRecommendation)
                HttpContext.Session.SetString("AtlasPackageFlow", "1");

            string answer;
            object packageInfo = null;

            if (wantsRecommendation && !depotCount.HasValue)
            {
                answer = "Size uygun paketi önerebilmem için kaç deponuz olduğunu yazar mısınız?";
            }
            else if (wantsRecommendation && !userCount.HasValue)
            {
                answer = $"{depotCount.Value} deponuz var. Peki sistemi kaç personel kullanacak?";
            }
            else if (wantsRecommendation)
            {
                // Sayıları birleştir; diğer soruları da koru.
                string combined = $"{depotCount.Value} depo {userCount.Value} kullanıcı hangi paket? " + message;
                answer = _salesService.GetResponse(combined);
                packageInfo = new { depotCount = depotCount.Value, userCount = userCount.Value };
                HttpContext.Session.Remove("AtlasPackageFlow");
            }
            else
            {
                answer = _salesService.GetResponse(message);
            }

            return Ok(new { reply = answer, packageInfo });
        }

        public class ChatRequest
        {
            public string Message { get; set; } = "";
        }
    }
}
