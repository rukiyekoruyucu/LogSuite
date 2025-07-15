namespace SmartDoors.Models
{
    public class Admin
    {
        public int AdminID { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
    }
}
