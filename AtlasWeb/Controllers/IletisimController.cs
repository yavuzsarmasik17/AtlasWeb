
using Microsoft.AspNetCore.Mvc;

namespace AtlasWeb.Controllers
{
    public class IletisimController : Controller
    {
        [HttpGet("/iletisim/whatsapp")]
        public IActionResult WhatsApp()
        {
            string mesaj = Uri.EscapeDataString(
                "Merhaba, ATLAS SOFT yazılımları hakkında bilgi almak istiyorum."
            );

            return Redirect(
                "https://wa.me/905493501072?text=" + mesaj
            );
        }
    }
}
