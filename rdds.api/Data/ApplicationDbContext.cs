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
            builder.Entity<IdentityRole>().HasData(roles);

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
        }
    }
}