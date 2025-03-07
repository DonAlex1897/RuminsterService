﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RuminsterBackend.Data;

#nullable disable

namespace RuminsterBackend.Migrations
{
    [DbContext(typeof(RuminsterDbContext))]
    partial class RuminsterDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("RoleId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(34)
                        .HasColumnType("varchar(34)");

                    b.HasKey("UserId", "RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);

                    b.HasDiscriminator<string>("Discriminator").HasValue("IdentityUserRole<string>");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("RuminsterBackend.Models.RefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreateTMS")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("IsRevoked")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("RuminsterBackend.Models.Role", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("RuminsterBackend.Models.Rumination", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("CreateById")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("CreateTMS")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsPublic")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<string>("UpdateById")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("UpdateTMS")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("CreateById");

                    b.HasIndex("UpdateById");

                    b.ToTable("Ruminations");
                });

            modelBuilder.Entity("RuminsterBackend.Models.RuminationAudience", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreateTMS")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<int>("RelationType")
                        .HasColumnType("int");

                    b.Property<int>("RuminationId")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdateTMS")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("RuminationId");

                    b.ToTable("RuminationAudiences");
                });

            modelBuilder.Entity("RuminsterBackend.Models.RuminationLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CallerMethod")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("CreateById")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("CreateTMS")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsPublic")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<int>("RuminationId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CreateById");

                    b.HasIndex("RuminationId");

                    b.ToTable("RuminationLogs");
                });

            modelBuilder.Entity("RuminsterBackend.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("RuminsterBackend.Models.UserRelation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CreateById")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("CreateTMS")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                    b.Property<string>("InitiatorId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("IsAccepted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsRejected")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<string>("ReceiverId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<string>("UpdateById")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("UpdateTMS")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("CreateById");

                    b.HasIndex("InitiatorId");

                    b.HasIndex("ReceiverId");

                    b.HasIndex("UpdateById");

                    b.ToTable("UserRelations");
                });

            modelBuilder.Entity("RuminsterBackend.Models.UserRelationLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CreateById")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("CreateTMS")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                    b.Property<string>("InitiatorId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<bool>("IsAccepted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsRejected")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(false);

                    b.Property<string>("ReceiverId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<int>("UserRelationId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CreateById");

                    b.HasIndex("InitiatorId");

                    b.HasIndex("ReceiverId");

                    b.HasIndex("UserRelationId");

                    b.ToTable("UserRelationLogs");
                });

            modelBuilder.Entity("RuminsterBackend.Models.UserRole", b =>
                {
                    b.HasBaseType("Microsoft.AspNetCore.Identity.IdentityUserRole<string>");

                    b.HasIndex("RoleId");

                    b.HasDiscriminator().HasValue("UserRole");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("RuminsterBackend.Models.Role", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("RuminsterBackend.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("RuminsterBackend.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("RuminsterBackend.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RuminsterBackend.Models.RefreshToken", b =>
                {
                    b.HasOne("RuminsterBackend.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("RuminsterBackend.Models.Rumination", b =>
                {
                    b.HasOne("RuminsterBackend.Models.User", "CreateBy")
                        .WithMany()
                        .HasForeignKey("CreateById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("RuminsterBackend.Models.User", "UpdateBy")
                        .WithMany()
                        .HasForeignKey("UpdateById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("CreateBy");

                    b.Navigation("UpdateBy");
                });

            modelBuilder.Entity("RuminsterBackend.Models.RuminationAudience", b =>
                {
                    b.HasOne("RuminsterBackend.Models.Rumination", "Rumination")
                        .WithMany("Audiences")
                        .HasForeignKey("RuminationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Rumination");
                });

            modelBuilder.Entity("RuminsterBackend.Models.RuminationLog", b =>
                {
                    b.HasOne("RuminsterBackend.Models.User", "CreateBy")
                        .WithMany()
                        .HasForeignKey("CreateById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("RuminsterBackend.Models.Rumination", "Rumination")
                        .WithMany("Logs")
                        .HasForeignKey("RuminationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreateBy");

                    b.Navigation("Rumination");
                });

            modelBuilder.Entity("RuminsterBackend.Models.UserRelation", b =>
                {
                    b.HasOne("RuminsterBackend.Models.User", "CreateBy")
                        .WithMany()
                        .HasForeignKey("CreateById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("RuminsterBackend.Models.User", "Initiator")
                        .WithMany()
                        .HasForeignKey("InitiatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RuminsterBackend.Models.User", "Receiver")
                        .WithMany()
                        .HasForeignKey("ReceiverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RuminsterBackend.Models.User", "UpdateBy")
                        .WithMany()
                        .HasForeignKey("UpdateById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("CreateBy");

                    b.Navigation("Initiator");

                    b.Navigation("Receiver");

                    b.Navigation("UpdateBy");
                });

            modelBuilder.Entity("RuminsterBackend.Models.UserRelationLog", b =>
                {
                    b.HasOne("RuminsterBackend.Models.User", "CreateBy")
                        .WithMany()
                        .HasForeignKey("CreateById")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("RuminsterBackend.Models.User", "Initiator")
                        .WithMany()
                        .HasForeignKey("InitiatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RuminsterBackend.Models.User", "Receiver")
                        .WithMany()
                        .HasForeignKey("ReceiverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RuminsterBackend.Models.UserRelation", "UserRelation")
                        .WithMany("Logs")
                        .HasForeignKey("UserRelationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreateBy");

                    b.Navigation("Initiator");

                    b.Navigation("Receiver");

                    b.Navigation("UserRelation");
                });

            modelBuilder.Entity("RuminsterBackend.Models.UserRole", b =>
                {
                    b.HasOne("RuminsterBackend.Models.Role", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RuminsterBackend.Models.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("RuminsterBackend.Models.Role", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("RuminsterBackend.Models.Rumination", b =>
                {
                    b.Navigation("Audiences");

                    b.Navigation("Logs");
                });

            modelBuilder.Entity("RuminsterBackend.Models.User", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("RuminsterBackend.Models.UserRelation", b =>
                {
                    b.Navigation("Logs");
                });
#pragma warning restore 612, 618
        }
    }
}
