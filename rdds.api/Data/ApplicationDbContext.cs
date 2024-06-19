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
        public DbSet<RoadCategory> RoadCategories { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

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
            .OwnsOne(cd => cd.IRI, iri =>
            {
                iri.Property(i => i.Roll).HasColumnName("IRI_Roll");
                iri.Property(i => i.Pitch).HasColumnName("IRI_Pitch");
                iri.Property(i => i.Euclidean).HasColumnName("IRI_Euclidean");
                iri.Property(i => i.Average).HasColumnName("IRI_Average");
                iri.Property(i => i.RollProfile).HasColumnName("Roll_Profile");
                iri.Property(i => i.PitchProfile).HasColumnName("Pitch_Profile");
                iri.Property(i => i.EuclideanProfile).HasColumnName("Euclidean_Profile");
                iri.Property(i => i.AverageProfile).HasColumnName("Average_Profile");
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
            
            // Configure many-to-many relationship between AppUser and Permission
            builder.Entity<UserPermission>()
            .HasKey(up => new { up.UserId, up.PermissionId });

            builder.Entity<UserPermission>()
                .HasOne(aup => aup.User)
                .WithMany(au => au.UserPermissions)
                .HasForeignKey(aup => aup.UserId);

            builder.Entity<UserPermission>()
                .HasOne(aup => aup.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(aup => aup.PermissionId);

            builder.Entity<RoadCategory>()
            .HasMany(rc => rc.Attempts)
            .WithOne(a => a.RoadCategory)
            .HasForeignKey(a => a.RoadCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
        }
    }
}