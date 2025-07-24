using System.Collections.Generic;
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
        public bool IsActive { get; set; } = true;

        public ICollection<UserDoor> UserDoors { get; set; } = new List<UserDoor>();

        // ✅ Giriş-çıkış log ilişkisi için gerekli
        public ICollection<Log> Logs { get; set; } = new List<Log>();
    }
}

