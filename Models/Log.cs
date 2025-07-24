using System;

namespace SmartDoors.Models
{
    public class Log
    {
        public int LogID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; } = null!;

        public int DoorID { get; set; }
        public Door Door { get; set; } = null!;

        public bool Operation { get; set; }  // true = giriş, false = çıkış

        public string CardID { get; set; } = null!;

        public bool ErrorStatus { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
