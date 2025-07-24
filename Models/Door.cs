using System.Collections.Generic;

namespace SmartDoors.Models
{
    public class Door
    {
        public int DoorID { get; set; }
        public string? DoorName { get; set; }

        public ICollection<UserDoor> UserDoors { get; set; } = new List<UserDoor>();
        public ICollection<Log> Logs { get; set; } = new List<Log>();
        public string? EntryDeviceID { get; set; }
        public string? ExitDeviceID { get; set; }

    }
}
