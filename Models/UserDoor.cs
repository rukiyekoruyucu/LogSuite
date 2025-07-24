using System.ComponentModel.DataAnnotations.Schema;

namespace SmartDoors.Models
{
    [Table("UsersDoors")]
    public class UserDoor
    {
        public int UserDoorID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; } = null!;
        public int DoorID { get; set; }
        public Door Door { get; set; } = null!;
        public bool AccessGranted { get; set; }
    }
}
