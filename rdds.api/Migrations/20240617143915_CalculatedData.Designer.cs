﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using rdds.api.Data;

#nullable disable

namespace rdds.api.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240617143915_CalculatedData")]
    partial class CalculatedData
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "ebabbafd-7356-4d7f-b248-339139cbadf7",
                            Name = "Admin",
                            NormalizedName = "ADMIN"
                        },
                        new
                        {
                            Id = "1fc7142a-a59e-4527-bb6d-7160da3f33a6",
                            Name = "User",
                            NormalizedName = "USER"
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("rdds.api.Models.AccessType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<List<string>>("Accesses")
                        .HasColumnType("text[]");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("AccessTypes");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Accesses = new List<string> { "read", "write", "update", "delete" },
                            Name = "Admin"
                        },
                        new
                        {
                            Id = 2,
                            Accesses = new List<string> { "read" },
                            Name = "User"
                        });
                });

            modelBuilder.Entity("rdds.api.Models.AppUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<int>("AccessTypeId")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("AccessTypeId");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("rdds.api.Models.Attempt", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("DeviceId")
                        .HasColumnType("text");

                    b.Property<DateTime?>("FinishedOn")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsFinished")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int?>("RoadCategoryId")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.HasIndex("RoadCategoryId");

                    b.ToTable("Attempts");
                });

            modelBuilder.Entity("rdds.api.Models.CalculatedData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int?>("AttemptId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp without time zone");

                    b.Property<float>("Velocity")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.HasIndex("AttemptId");

                    b.ToTable("CalculatedDatas");
                });

            modelBuilder.Entity("rdds.api.Models.Device", b =>
                {
                    b.Property<string>("MacAddress")
                        .HasColumnType("text");

                    b.Property<string>("AppUserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("DeviceName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("MacAddress");

                    b.HasIndex("AppUserId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("rdds.api.Models.RoadCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<float>("TotalLength")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.ToTable("RoadCategories");
                });

            modelBuilder.Entity("rdds.api.Models.RoadData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int?>("AttemptId")
                        .HasColumnType("integer");

                    b.Property<float>("Euclidean")
                        .HasColumnType("real");

                    b.Property<float>("Pitch")
                        .HasColumnType("real");

                    b.Property<float>("Roll")
                        .HasColumnType("real");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp without time zone");

                    b.Property<float>("Velocity")
                        .HasColumnType("real");

                    b.Property<float>("Yaw")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.HasIndex("AttemptId");

                    b.ToTable("RoadDatas");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("rdds.api.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("rdds.api.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("rdds.api.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("rdds.api.Models.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("rdds.api.Models.AppUser", b =>
                {
                    b.HasOne("rdds.api.Models.AccessType", "AccessType")
                        .WithMany("Users")
                        .HasForeignKey("AccessTypeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("AccessType");
                });

            modelBuilder.Entity("rdds.api.Models.Attempt", b =>
                {
                    b.HasOne("rdds.api.Models.Device", "Device")
                        .WithMany("Attempts")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("rdds.api.Models.RoadCategory", "RoadCategory")
                        .WithMany("Attempts")
                        .HasForeignKey("RoadCategoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Device");

                    b.Navigation("RoadCategory");
                });

            modelBuilder.Entity("rdds.api.Models.CalculatedData", b =>
                {
                    b.HasOne("rdds.api.Models.Attempt", "Attempt")
                        .WithMany("CalculatedData")
                        .HasForeignKey("AttemptId");

                    b.OwnsOne("rdds.api.Models.Coordinate", "Coordinate", b1 =>
                        {
                            b1.Property<Guid>("CalculatedDataId")
                                .HasColumnType("uuid");

                            b1.Property<float>("Latitude")
                                .HasColumnType("real")
                                .HasColumnName("Latitude");

                            b1.Property<float>("Longitude")
                                .HasColumnType("real")
                                .HasColumnName("Longitude");

                            b1.HasKey("CalculatedDataId");

                            b1.ToTable("CalculatedDatas");

                            b1.WithOwner()
                                .HasForeignKey("CalculatedDataId");
                        });

                    b.OwnsOne("rdds.api.Models.InternationalRoughnessIndex", "IRI", b1 =>
                        {
                            b1.Property<Guid>("CalculatedDataId")
                                .HasColumnType("uuid");

                            b1.Property<float>("Euclidean")
                                .HasColumnType("real")
                                .HasColumnName("IRI_Euclidean");

                            b1.Property<float>("Pitch")
                                .HasColumnType("real")
                                .HasColumnName("IRI_Pitch");

                            b1.Property<float>("Roll")
                                .ValueGeneratedOnUpdateSometimes()
                                .HasColumnType("real")
                                .HasColumnName("Roll");

                            b1.HasKey("CalculatedDataId");

                            b1.ToTable("CalculatedDatas");

                            b1.WithOwner()
                                .HasForeignKey("CalculatedDataId");
                        });

                    b.OwnsOne("rdds.api.Models.PowerSpectralDensity", "PSD", b1 =>
                        {
                            b1.Property<Guid>("CalculatedDataId")
                                .HasColumnType("uuid");

                            b1.Property<float>("Euclidean")
                                .HasColumnType("real")
                                .HasColumnName("PSD_Euclidean");

                            b1.Property<float>("Pitch")
                                .HasColumnType("real")
                                .HasColumnName("PSD_Pitch");

                            b1.Property<float>("Roll")
                                .ValueGeneratedOnUpdateSometimes()
                                .HasColumnType("real")
                                .HasColumnName("Roll");

                            b1.HasKey("CalculatedDataId");

                            b1.ToTable("CalculatedDatas");

                            b1.WithOwner()
                                .HasForeignKey("CalculatedDataId");
                        });

                    b.Navigation("Attempt");

                    b.Navigation("Coordinate")
                        .IsRequired();

                    b.Navigation("IRI")
                        .IsRequired();

                    b.Navigation("PSD")
                        .IsRequired();
                });

            modelBuilder.Entity("rdds.api.Models.Device", b =>
                {
                    b.HasOne("rdds.api.Models.AppUser", "AppUser")
                        .WithMany()
                        .HasForeignKey("AppUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AppUser");
                });

            modelBuilder.Entity("rdds.api.Models.RoadData", b =>
                {
                    b.HasOne("rdds.api.Models.Attempt", "Attempt")
                        .WithMany("RoadDatas")
                        .HasForeignKey("AttemptId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.OwnsOne("rdds.api.Models.Coordinate", "Coordinate", b1 =>
                        {
                            b1.Property<Guid>("RoadDataId")
                                .HasColumnType("uuid");

                            b1.Property<float>("Latitude")
                                .HasColumnType("real")
                                .HasColumnName("Latitude");

                            b1.Property<float>("Longitude")
                                .HasColumnType("real")
                                .HasColumnName("Longitude");

                            b1.HasKey("RoadDataId");

                            b1.ToTable("RoadDatas");

                            b1.WithOwner()
                                .HasForeignKey("RoadDataId");
                        });

                    b.Navigation("Attempt");

                    b.Navigation("Coordinate")
                        .IsRequired();
                });

            modelBuilder.Entity("rdds.api.Models.AccessType", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("rdds.api.Models.Attempt", b =>
                {
                    b.Navigation("CalculatedData");

                    b.Navigation("RoadDatas");
                });

            modelBuilder.Entity("rdds.api.Models.Device", b =>
                {
                    b.Navigation("Attempts");
                });

            modelBuilder.Entity("rdds.api.Models.RoadCategory", b =>
                {
                    b.Navigation("Attempts");
                });
#pragma warning restore 612, 618
        }
    }
}