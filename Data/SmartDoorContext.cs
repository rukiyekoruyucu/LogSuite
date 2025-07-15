using Microsoft.EntityFrameworkCore;
using SmartDoors.Models;

namespace SmartDoors.Data
{
    public class SmartDoorContext : DbContext
    {
        public SmartDoorContext(DbContextOptions<SmartDoorContext> options) : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Log> Logs { get; set; }
    }
}
