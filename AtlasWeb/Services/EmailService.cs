
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;

namespace AtlasWeb.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendDemoEmailAsync(
            string adSoyad,
            string firma,
            string telefon,
            string email,
            string urun,
            string mesaj)
        {
            var settings = _configuration.GetSection("EmailSettings");

            var host = settings["Host"] ?? "smtp.gmail.com";
            var port = int.Parse(settings["Port"] ?? "587");
            var username = settings["Username"];
            var password = settings["Password"];
            var from = settings["From"];
            var to = settings["To"];

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(from) ||
                string.IsNullOrWhiteSpace(to))
            {
                _logger.LogError(
                    "SMTP yapılandırması eksik.");

                throw new InvalidOperationException(
                    "E-posta ayarları eksik.");
            }

            var mail = new MimeMessage();

            mail.From.Add(new MailboxAddress(
                "Atlas Soft | Demo Talepleri", from));

            mail.ReplyTo.Add(new MailboxAddress(
                adSoyad, email));

            mail.To.Add(MailboxAddress.Parse(to));

            mail.Subject =
                $"Atlas Soft | Yeni Demo Talebi - {urun}";

            mail.Body = new TextPart("plain")
            {
                Text =
                    "ATLAS SOFT - YENİ DEMO TALEBİ\n\n" +
                    $"Ad Soyad: {adSoyad}\n" +
                    $"Firma: {firma}\n" +
                    $"Telefon: {telefon}\n" +
                    $"E-posta: {email}\n" +
                    $"İlgilendiği Ürün: {urun}\n\n" +
                    $"Mesaj:\n{mesaj}"
            };

            using var client = new SmtpClient();

            try
            {
                _logger.LogInformation(
                    "SMTP bağlantısı başlatılıyor.");

                await client.ConnectAsync(
                    host,
                    port,
                    SecureSocketOptions.StartTls);

                _logger.LogInformation(
                    "SMTP bağlantısı başarılı.");

                await client.AuthenticateAsync(
                    username,
                    password);

                _logger.LogInformation(
                    "SMTP kimlik doğrulaması başarılı.");

                await client.SendAsync(mail);

                _logger.LogInformation(
                    "Demo e-postası başarıyla gönderildi.");

                await client.DisconnectAsync(true);
            }

            catch (Exception ex)
            {
                _logger.LogError(
                    "Demo e-posta gönderim hatası. Hata türü: {HataTuru}",
                    ex.GetType().Name);

                try
                {
                    var logKlasoru = Path.Combine(
                        AppContext.BaseDirectory,
                        "App_Data",
                        "Logs");

                    Directory.CreateDirectory(logKlasoru);

                    var logDosyasi = Path.Combine(
                        logKlasoru,
                        "smtp-test.log");

                    var hataMetni =
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | " +
                        $"SMTP HATASI | {ex.GetType().Name} | " +
                        $"{ex.Message}{Environment.NewLine}";

                    await File.AppendAllTextAsync(
                        logDosyasi,
                        hataMetni);
                }
                catch
                {
                    // Log yazilamasa bile asil SMTP hatasini koru.
                }

                throw;
            }

        }
    }
}
