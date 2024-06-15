using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using rdds.api.Models;

namespace rdds.api.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<Device> Devices {get; set;}
        public DbSet<Attempt> Attempts {get; set;}
        public DbSet<RoadData> RoadDatas { get; set; }
        public DbSet<AccessType> AccessTypes { get; set; }
        public DbSet<UserAccess> UserAccesses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Name = "User",
                    NormalizedName = "USER"
                },
            };

            List<AccessType> accessTypes = new List<AccessType>
            {
                new AccessType { Id = 1, Name = "read" },
                new AccessType { Id = 2, Name = "write" },
                new AccessType { Id = 3, Name = "update" },
                new AccessType { Id = 4, Name = "delete" }
            };
            
            builder.Entity<IdentityRole>().HasData(roles);
            builder.Entity<AccessType>().HasData(accessTypes);

            builder.Entity<RoadData>()
            .OwnsOne(rd => rd.Coordinate);

            builder.Entity<Device>()
            .HasMany(a => a.Attempts)
            .WithOne(rd => rd.Device)
            .HasForeignKey(rd => rd.DeviceId);

            builder.Entity<Attempt>()
            .HasMany(a => a.RoadDatas)
            .WithOne(rd => rd.Attempt)
            .HasForeignKey(rd => rd.AttemptId);

            // Configure many-to-many relationship between AppUser and AccessType via UserAccess
            builder.Entity<UserAccess>()
                .HasKey(ua => new { ua.UserId, ua.AccessTypeId });

            builder.Entity<UserAccess>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserAccesses)
                .HasForeignKey(ua => ua.UserId);

            builder.Entity<UserAccess>()
                .HasOne(ua => ua.AccessType)
                .WithMany(at => at.UserAccesses)
                .HasForeignKey(ua => ua.AccessTypeId);
        }
    }
}