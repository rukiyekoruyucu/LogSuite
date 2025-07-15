namespace SmartDoors.Models
{
    public class Log
    {
        public int LogID { get; set; }
        public int? UserID { get; set; }
        public User? User { get; set; }  // Navigation property
        public string Operation { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string? CardID { get; set; }
        public bool ErrorStatus { get; set; } = false;
    }
}
