using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RuminsterBackend.Extensions;
using RuminsterBackend.Models;

namespace RuminsterBackend.Data
{
    public class RuminsterDbContext : IdentityDbContext<
        User, Role, string,
        IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>,
        IdentityRoleClaim<string>, IdentityUserToken<string>>
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

        public new DbSet<UserToken> UserTokens { get; set; }

        public DbSet<TermsOfService> TermsOfService { get; set; }

        public DbSet<UserTosAcceptance> UserTosAcceptances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<User>(b =>
            {
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<Role>(b =>
            {
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });

            var datetimeConverter = new ValueConverter<DateTime, DateTime>(
                e => e.SafeToUniversalTime(),
                e => DateTime.SpecifyKind(e, DateTimeKind.Utc)
            );

            var ruminationMb = modelBuilder.Entity<Rumination>();
            ruminationMb.Property(s => s.Id).IsRequired();
            ruminationMb.Property(s => s.IsDeleted).HasDefaultValue(false).IsRequired();
            ruminationMb.Property(s => s.Content);
            ruminationMb.Property(s => s.IsPublished).HasDefaultValue(false).IsRequired();
            ruminationMb.Property(s => s.CreateById).IsRequired();
            ruminationMb.Property(s => s.UpdateById).IsRequired();
            ruminationMb.Property(s => s.CreateTMS).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP");
            ruminationMb.Property(s => s.UpdateTMS).HasConversion(datetimeConverter);

            ruminationMb
                .HasOne(s => s.CreateBy)
                .WithMany(s => s.RuminationsCreateBy)
                .HasForeignKey(s => s.CreateById)
                .OnDelete(DeleteBehavior.Restrict);
            ruminationMb
                .HasOne(s => s.UpdateBy)
                .WithMany(s => s.RuminationsUpdateBy)
                .HasForeignKey(s => s.UpdateById)
                .OnDelete(DeleteBehavior.Restrict);

            var ruminationLogMb = modelBuilder.Entity<RuminationLog>();
            ruminationLogMb.Property(s => s.Id).IsRequired();
            ruminationLogMb.Property(s => s.RuminationId).IsRequired();
            ruminationLogMb.Property(s => s.IsDeleted).HasDefaultValue(false).IsRequired();
            ruminationLogMb.Property(s => s.Content);
            ruminationLogMb.Property(s => s.IsPublished).HasDefaultValue(false).IsRequired();
            ruminationLogMb.Property(s => s.CallerMethod);
            ruminationLogMb.Property(s => s.CreateById).IsRequired();
            ruminationLogMb.Property(s => s.CreateTMS).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP");

            ruminationLogMb
                .HasOne(s => s.CreateBy)
                .WithMany(s => s.RuminationLogsCreateBy)
                .HasForeignKey(s => s.CreateById)
                .OnDelete(DeleteBehavior.Restrict);
            ruminationLogMb
                .HasOne(s => s.Rumination)
                .WithMany(s => s.Logs)
                .HasForeignKey(s => s.RuminationId)
                .OnDelete(DeleteBehavior.Cascade);

            var ruminationAudienceMb = modelBuilder.Entity<RuminationAudience>();
            ruminationAudienceMb.Property(s => s.Id).IsRequired();
            ruminationAudienceMb.Property(s => s.IsDeleted).HasDefaultValue(false).IsRequired();
            ruminationAudienceMb.Property(s => s.RuminationId).IsRequired();
            ruminationAudienceMb.Property(s => s.RelationType);
            ruminationAudienceMb.Property(s => s.CreateTMS).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP");
            ruminationAudienceMb.Property(s => s.UpdateTMS).HasConversion(datetimeConverter);
            
            ruminationAudienceMb
                .HasOne(s => s.Rumination)
                .WithMany(s => s.Audiences)
                .HasForeignKey(s => s.RuminationId)
                .OnDelete(DeleteBehavior.Cascade);

            var refreshTokenMb = modelBuilder.Entity<RefreshToken>();
            refreshTokenMb.Property(s => s.Id).IsRequired();
            refreshTokenMb.Property(s => s.UserId).IsRequired();
            refreshTokenMb.Property(s => s.Token).IsRequired();
            refreshTokenMb.Property(s => s.ExpiresAt).HasConversion(datetimeConverter);
            refreshTokenMb.Property(s => s.IsRevoked).HasDefaultValue(false).IsRequired();
            refreshTokenMb.Property(s => s.CreateTMS).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP");

            refreshTokenMb
                .HasOne(s => s.User)
                .WithMany(s => s.RefreshTokens)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

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
            userRelationMb.Property(s => s.CreateTMS).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP");
            userRelationMb.Property(s => s.UpdateTMS).HasConversion(datetimeConverter);

            userRelationMb
                .HasOne(s => s.Initiator)
                .WithMany(s => s.UserRelationsInitiator)
                .HasForeignKey(s => s.InitiatorId)
                .OnDelete(DeleteBehavior.Restrict);
            userRelationMb
                .HasOne(s => s.Receiver)
                .WithMany(s => s.UserRelationsReceiver)
                .HasForeignKey(s => s.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            userRelationMb
                .HasOne(s => s.CreateBy)
                .WithMany(s => s.UserRelationsCreateBy)
                .HasForeignKey(s => s.CreateById)
                .OnDelete(DeleteBehavior.Restrict);
            userRelationMb
                .HasOne(s => s.UpdateBy)
                .WithMany(s => s.UserRelationsUpdateBy)
                .HasForeignKey(s => s.UpdateById)
                .OnDelete(DeleteBehavior.Restrict);

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
            userRelationLogMb.Property(s => s.CreateTMS).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP");

            userRelationLogMb
                .HasOne(s => s.Initiator)
                .WithMany(s => s.UserRelationLogsInitiator)
                .HasForeignKey(s => s.InitiatorId)
                .OnDelete(DeleteBehavior.Restrict);
            userRelationLogMb
                .HasOne(s => s.Receiver)
                .WithMany(s => s.UserRelationLogsReceiver)
                .HasForeignKey(s => s.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            userRelationLogMb
                .HasOne(s => s.CreateBy)
                .WithMany(s => s.UserRelationLogsCreateBy)
                .HasForeignKey(s => s.CreateById)
                .OnDelete(DeleteBehavior.Restrict);
            userRelationLogMb
                .HasOne(s => s.UserRelation)
                .WithMany(s => s.Logs)
                .HasForeignKey(s => s.UserRelationId)
                .OnDelete(DeleteBehavior.Cascade);

            var userTokenMb = modelBuilder.Entity<UserToken>();
            userTokenMb.Property(s => s.Id).IsRequired();
            userTokenMb.Property(s => s.UserId).IsRequired();
            userTokenMb.Property(s => s.Token).IsRequired();
            userTokenMb.Property(s => s.TokenType).IsRequired();
            userTokenMb.Property(s => s.ExpiresAt).HasConversion(datetimeConverter);
            userTokenMb.Property(s => s.CreatedAt).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP");
            userTokenMb.Property(s => s.IsUsed).HasDefaultValue(false).IsRequired();

            userTokenMb
                .HasOne(s => s.User)
                .WithMany(s => s.UserTokens)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            userTokenMb.HasIndex(s => s.Token).IsUnique();

            var termsOfServiceMb = modelBuilder.Entity<TermsOfService>();
            termsOfServiceMb.Property(s => s.Id).IsRequired();
            termsOfServiceMb.Property(s => s.Version).IsRequired().HasMaxLength(50);
            termsOfServiceMb.Property(s => s.Content).IsRequired();
            termsOfServiceMb.Property(s => s.CreatedAt).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP");
            termsOfServiceMb.Property(s => s.IsActive).HasDefaultValue(false).IsRequired();

            var userTosAcceptanceMb = modelBuilder.Entity<UserTosAcceptance>();
            userTosAcceptanceMb.Property(s => s.Id).IsRequired();
            userTosAcceptanceMb.Property(s => s.UserId).IsRequired();
            userTosAcceptanceMb.Property(s => s.TermsOfServiceId).IsRequired();
            userTosAcceptanceMb.Property(s => s.AcceptedAt).HasConversion(datetimeConverter).HasDefaultValueSql("CURRENT_TIMESTAMP");
            userTosAcceptanceMb.Property(s => s.AcceptedVersion).IsRequired().HasMaxLength(50);

            userTosAcceptanceMb
                .HasOne(s => s.User)
                .WithMany(s => s.TosAcceptances)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            userTosAcceptanceMb
                .HasOne(s => s.TermsOfService)
                .WithMany(s => s.UserAcceptances)
                .HasForeignKey(s => s.TermsOfServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}