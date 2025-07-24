using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartDoors.ViewModels
{
    public class AddLogViewModel
    {
        [Required(ErrorMessage = "Kart ID zorunludur.")]
        [Display(Name = "Kart ID")]
        public string CardID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kapı seçimi zorunludur.")]
        [Display(Name = "Kapı")]
        public string DoorName { get; set; } = string.Empty;

        [Display(Name = "Giriş")]
        public bool IsEntry { get; set; }

        [Display(Name = "Çıkış")]
        public bool IsExit { get; set; }

        [Display(Name = "Hata Durumu")]
        public bool ErrorStatus { get; set; }

        [Display(Name = "Tarih / Saat")]
        [DataType(DataType.DateTime)]
        public DateTime? Timestamp { get; set; }

        public string? UserFullName { get; set; }

        public List<string> AuthorizedDoors { get; set; } = new List<string>();

        public List<SelectListItem> Doors { get; set; } = new List<SelectListItem>();
    }
}

