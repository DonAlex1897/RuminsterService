using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RuminsterBackend.Extensions;
using RuminsterBackend.Models;

namespace RuminsterBackend.Data
{
    public class RuminsterDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public RuminsterDbContext(DbContextOptions<RuminsterDbContext> options)
            : base(options)
        {
        }


        public DbSet<Rumination> Ruminations { get; set; }

        public DbSet<RuminationLog> RuminationLogs { get; set; }

        public DbSet<RuminationAudience> RuminationAudiences { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<UserRelation> UserRelations { get; set; }

        public DbSet<UserRelationLog> UserRelationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensure Identity configuration is applied

            var datetimeConverter = new ValueConverter<DateTime, DateTime>(e => e.SafeToUniversalTime(), e => DateTime.SpecifyKind(e, DateTimeKind.Utc));

            var ruminationMb = modelBuilder.Entity<Rumination>();
            ruminationMb.Property(s => s.Id).IsRequired();
            ruminationMb.Property(s => s.IsDeleted).HasDefaultValue(false).IsRequired();
            ruminationMb.Property(s => s.Content);
            ruminationMb.Property(s => s.IsPublic).HasDefaultValue(false).IsRequired();
            ruminationMb.Property(s => s.CreateById).IsRequired();
            ruminationMb.Property(s => s.UpdateById).IsRequired();
            ruminationMb.Property(s => s.CreateTMS).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            ruminationMb.Property(s => s.UpdateTMS).HasConversion(datetimeConverter);

            ruminationMb.HasOne(s => s.CreateBy).WithMany()
                .HasForeignKey(s => s.CreateById).OnDelete(DeleteBehavior.Restrict);
            ruminationMb.HasOne(s => s.UpdateBy).WithMany()
                .HasForeignKey(s => s.UpdateById).OnDelete(DeleteBehavior.Restrict);

            var ruminationLogMb = modelBuilder.Entity<RuminationLog>();
            ruminationLogMb.Property(s => s.Id).IsRequired();
            ruminationLogMb.Property(s => s.RuminationId).IsRequired();
            ruminationLogMb.Property(s => s.IsDeleted).HasDefaultValue(false).IsRequired();
            ruminationLogMb.Property(s => s.Content);
            ruminationLogMb.Property(s => s.IsPublic).HasDefaultValue(false).IsRequired();
            ruminationLogMb.Property(s => s.CallerMethod);
            ruminationLogMb.Property(s => s.CreateById).IsRequired();
            ruminationLogMb.Property(s => s.CreateTMS).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            ruminationLogMb.HasOne(s => s.CreateBy).WithMany()
                .HasForeignKey(s => s.CreateById).OnDelete(DeleteBehavior.Restrict);
            ruminationLogMb.HasOne(s => s.Rumination).WithMany(s => s.Logs)
                .HasForeignKey(s => s.RuminationId).OnDelete(DeleteBehavior.Cascade);

            var ruminationAudienceMb = modelBuilder.Entity<RuminationAudience>();
            ruminationAudienceMb.Property(s => s.Id).IsRequired();
            ruminationAudienceMb.Property(s => s.IsDeleted).HasDefaultValue(false).IsRequired();
            ruminationAudienceMb.Property(s => s.RuminationId).IsRequired();
            ruminationAudienceMb.Property(s => s.RelationType);
            ruminationAudienceMb.Property(s => s.CreateTMS).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            ruminationAudienceMb.Property(s => s.UpdateTMS).HasConversion(datetimeConverter);
            
            ruminationAudienceMb.HasOne(s => s.Rumination).WithMany(s => s.Audiences)
                .HasForeignKey(s => s.RuminationId).OnDelete(DeleteBehavior.Cascade);

            var refreshTokenMb = modelBuilder.Entity<RefreshToken>();
            refreshTokenMb.Property(s => s.Id).IsRequired();
            refreshTokenMb.Property(s => s.UserId).IsRequired();
            refreshTokenMb.Property(s => s.Token).IsRequired();
            refreshTokenMb.Property(s => s.ExpiresAt).HasConversion(datetimeConverter);
            refreshTokenMb.Property(s => s.IsRevoked).HasDefaultValue(false).IsRequired();
            refreshTokenMb.Property(s => s.CreateTMS).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            refreshTokenMb.HasOne(s => s.User).WithMany()
                .HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Restrict);

            var userRelationMb = modelBuilder.Entity<UserRelation>();
            userRelationMb.Property(s => s.Id).IsRequired();
            userRelationMb.Property(s => s.IsDeleted).HasDefaultValue(false).IsRequired();
            userRelationMb.Property(s => s.InitiatorId).IsRequired();
            userRelationMb.Property(s => s.ReceiverId).IsRequired();
            userRelationMb.Property(s => s.IsAccepted).HasDefaultValue(false).IsRequired();
            userRelationMb.Property(s => s.IsRejected).HasDefaultValue(false).IsRequired();
            userRelationMb.Property(s => s.Type).IsRequired();
            userRelationMb.Property(s => s.CreateById).IsRequired();
            userRelationMb.Property(s => s.UpdateById).IsRequired();
            userRelationMb.Property(s => s.CreateTMS).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            userRelationMb.Property(s => s.UpdateTMS).HasConversion(datetimeConverter);

            userRelationMb.HasOne(s => s.CreateBy).WithMany()
                .HasForeignKey(s => s.CreateById).OnDelete(DeleteBehavior.Restrict);
            userRelationMb.HasOne(s => s.UpdateBy).WithMany()
                .HasForeignKey(s => s.UpdateById).OnDelete(DeleteBehavior.Restrict);

            var userRelationLogMb = modelBuilder.Entity<UserRelationLog>();
            userRelationLogMb.Property(s => s.Id).IsRequired();
            userRelationLogMb.Property(s => s.UserRelationId).IsRequired();
            userRelationLogMb.Property(s => s.IsDeleted).HasDefaultValue(false).IsRequired();
            userRelationLogMb.Property(s => s.InitiatorId).IsRequired();
            userRelationLogMb.Property(s => s.ReceiverId).IsRequired();
            userRelationLogMb.Property(s => s.IsAccepted).HasDefaultValue(false).IsRequired();
            userRelationLogMb.Property(s => s.IsRejected).HasDefaultValue(false).IsRequired();
            userRelationLogMb.Property(s => s.Type).IsRequired();
            userRelationLogMb.Property(s => s.CreateById).IsRequired();
            userRelationLogMb.Property(s => s.CreateTMS).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            userRelationLogMb.HasOne(s => s.CreateBy).WithMany()
                .HasForeignKey(s => s.CreateById).OnDelete(DeleteBehavior.Restrict);
            userRelationLogMb.HasOne(s => s.UserRelation).WithMany(s => s.Logs)
                .HasForeignKey(s => s.UserRelationId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}