using DigitalNoticeBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalNoticeBoard.Infrastructure.Persistence;

public sealed class NoticeBoardDbContext(DbContextOptions<NoticeBoardDbContext> options)
    : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Notice> Notices => Set<Notice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Username).HasMaxLength(100).IsRequired();
            entity.Property(user => user.DisplayName).HasMaxLength(160).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(user => user.Role).HasMaxLength(32).IsRequired();
            entity.HasIndex(user => user.Username).IsUnique();
        });

        modelBuilder.Entity<Notice>(entity =>
        {
            entity.ToTable("Notices");
            entity.HasKey(notice => notice.Id);
            entity.Property(notice => notice.Title).HasMaxLength(160).IsRequired();
            entity.Property(notice => notice.Summary).HasMaxLength(500);
            entity.Property(notice => notice.Content).HasMaxLength(4000).IsRequired();
            entity.Property(notice => notice.Category).HasMaxLength(100).IsRequired();
            entity.Property(notice => notice.Status)
                .HasConversion<string>()
                .HasMaxLength(24)
                .IsRequired();
            entity.HasIndex(notice => new
            {
                notice.Status,
                notice.PublishFromUtc,
                notice.PublishUntilUtc
            });
            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(notice => notice.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
