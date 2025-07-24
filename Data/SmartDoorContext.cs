using Microsoft.EntityFrameworkCore;
using SmartDoors.Models;

namespace SmartDoors.Data
{
    public class SmartDoorContext : DbContext
    {
        public SmartDoorContext(DbContextOptions<SmartDoorContext> options) : base(options) { }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Door> Doors { get; set; }
        public DbSet<UserDoor> UserDoors { get; set; }
        public DbSet<Log> Logs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDoor>()
                .ToTable("UsersDoors")    // Veritabanındaki tablo adı bu ise bunu kullan
                .HasKey(ud => ud.UserDoorID);

            modelBuilder.Entity<UserDoor>()
                .HasOne(ud => ud.User)
                .WithMany(u => u.UserDoors)
                .HasForeignKey(ud => ud.UserID);

            modelBuilder.Entity<UserDoor>()
                .HasOne(ud => ud.Door)
                .WithMany(d => d.UserDoors)
                .HasForeignKey(ud => ud.DoorID);

            modelBuilder.Entity<Log>()
                .Property(l => l.Timestamp)
                .HasColumnType("datetime2");

            modelBuilder.Entity<Log>()
                .HasOne(l => l.User)
                .WithMany(u => u.Logs)
                .HasForeignKey(l => l.UserID);

            modelBuilder.Entity<Log>()
                .HasOne(l => l.Door)
                .WithMany(d => d.Logs)
                .HasForeignKey(l => l.DoorID);
        }
    }
}

