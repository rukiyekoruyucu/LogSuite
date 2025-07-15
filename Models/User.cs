using System.ComponentModel.DataAnnotations;

namespace SmartDoors.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Kart ID zorunludur.")]
        [StringLength(100, ErrorMessage = "Kart ID en fazla 100 karakter olabilir.")]
        public string CardID { get; set; } = null!;

        [Display(Name = "Kapı Erişimi")]
        public bool DoorAccess { get; set; } = true;
    }
}