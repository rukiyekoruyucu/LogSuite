using System;

namespace SmartDoors.ViewModels
{
    public class LogViewModel
    {
        public int LogID { get; set; }
        public string UserFullName { get; set; } = string.Empty;  // Null gelme riskine karşı boş string atandı
        public string DoorName { get; set; } = string.Empty;      // Aynı şekilde
        public DateTime Timestamp { get; set; }
        public bool ErrorStatus { get; set; }
        public bool HasAccess { get; set; }
        public bool IsEntry { get; set; } // true = Giriş, false = Çıkış


    }
}

