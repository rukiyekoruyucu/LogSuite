using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartDoors.ViewModels
{
    public class UserFormViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kart ID zorunludur.")]
        [StringLength(100)]
        public string CardID { get; set; } = string.Empty;

        public List<int> SelectedDoorIds { get; set; } = new();

        public List<SelectListItem> AllDoors { get; set; } = new();
    }
}
