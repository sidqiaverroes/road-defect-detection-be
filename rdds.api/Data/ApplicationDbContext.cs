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
        public DbSet<CalculatedData> CalculatedDatas { get; set; }
        public DbSet<AccessType> AccessTypes { get; set; }
        public DbSet<RoadCategory> RoadCategories { get; set; }

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
                new AccessType { Id = 1, Name = "Admin", Accesses = new List<string> { "read", "write", "update", "delete" } },
                new AccessType { Id = 2, Name = "User", Accesses = new List<string> { "read" } }
            };
            
            builder.Entity<IdentityRole>().HasData(roles);
            builder.Entity<AccessType>().HasData(accessTypes);

            builder.Entity<RoadData>()
            .OwnsOne(rd => rd.Coordinate, coord =>
            {
                coord.Property(c => c.Latitude).HasColumnName("Latitude");
                coord.Property(c => c.Longitude).HasColumnName("Longitude");
            });

            builder.Entity<CalculatedData>()
            .OwnsOne(cd => cd.Coordinate, coord =>
            {
                coord.Property(c => c.Latitude).HasColumnName("Latitude");
                coord.Property(c => c.Longitude).HasColumnName("Longitude");
            });

            builder.Entity<CalculatedData>()
            .OwnsOne(cd => cd.PSD, psd =>
            {
                psd.Property(p => p.Roll).HasColumnName("PSD_Roll");
                psd.Property(p => p.Pitch).HasColumnName("PSD_Pitch");
                psd.Property(p => p.Euclidean).HasColumnName("PSD_Euclidean");
            });

            builder.Entity<CalculatedData>()
            .OwnsOne(cd => cd.IRI, iri =>
            {
                iri.Property(i => i.Roll).HasColumnName("IRI_Roll");
                iri.Property(i => i.Pitch).HasColumnName("IRI_Pitch");
                iri.Property(i => i.Euclidean).HasColumnName("IRI_Euclidean");
            });

            builder.Entity<CalculatedData>()
            .HasOne(cd => cd.Attempt)
            .WithMany(a => a.CalculatedData)
            .HasForeignKey(cd => cd.AttemptId);

            builder.Entity<Device>()
            .HasMany(a => a.Attempts)
            .WithOne(rd => rd.Device)
            .HasForeignKey(rd => rd.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);;

            builder.Entity<Attempt>()
            .HasMany(a => a.RoadDatas)
            .WithOne(rd => rd.Attempt)
            .HasForeignKey(rd => rd.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);;
            
            // Configure many-to-many relationship between AppUser and AccessType
            builder.Entity<AppUser>()
            .HasOne(u => u.AccessType)
            .WithMany(a => a.Users)
            .HasForeignKey(u => u.AccessTypeId)
            .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AccessType>()
            .HasMany(a => a.Users)
            .WithOne(rd => rd.AccessType)
            .HasForeignKey(rd => rd.AccessTypeId);

            builder.Entity<RoadCategory>()
            .HasMany(rc => rc.Attempts)
            .WithOne(a => a.RoadCategory)
            .HasForeignKey(a => a.RoadCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
        }
    }
}