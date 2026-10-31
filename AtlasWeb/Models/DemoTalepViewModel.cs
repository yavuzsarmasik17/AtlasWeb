
using System.ComponentModel.DataAnnotations;

namespace AtlasWeb.Models
{
    public class DemoTalepViewModel
    {
        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = "";

        [Required(ErrorMessage = "Firma adı zorunludur.")]
        [StringLength(150)]
        [Display(Name = "Firma Adı")]
        public string Firma { get; set; } = "";

        [Required(ErrorMessage = "Telefon zorunludur.")]
        [StringLength(30)]
        [Phone(ErrorMessage = "Geçerli bir telefon giriniz.")]
        public string Telefon { get; set; } = "";

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        [StringLength(150)]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Lütfen bir yazılım seçiniz.")]
        public string Urun { get; set; } = "";

        [StringLength(2000)]
        public string Mesaj { get; set; } = "";

        // Kullanıcıların görmediği spam tuzağı
        public string? Website { get; set; }
    }
}
