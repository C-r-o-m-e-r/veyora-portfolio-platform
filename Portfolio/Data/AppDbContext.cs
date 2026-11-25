using Microsoft.EntityFrameworkCore;
using Portfolio.Models;

namespace Portfolio.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<AvailableSlot> AvailableSlots => Set<AvailableSlot>();
    public DbSet<BookingRequest> BookingRequests => Set<BookingRequest>();
    public DbSet<UserCredential> UserCredentials => Set<UserCredential>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).HasMaxLength(500).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(2000);
            entity.Property(t => t.StartDate).IsRequired();
            entity.Property(t => t.Deadline).IsRequired();
            entity.Property(t => t.Priority).HasConversion<int>();
            entity.HasIndex(t => t.StartDate);
            entity.HasIndex(t => t.Deadline);
        });

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<UserCredential>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CredentialId).IsRequired();
            entity.Property(c => c.PublicKey).IsRequired();
            entity.Property(c => c.UserHandle).IsRequired();
            entity.Property(c => c.CredType).HasMaxLength(64).IsRequired();
            entity.Property(c => c.RegDate).IsRequired();
            entity.HasIndex(c => c.UserId);
            entity.HasIndex(c => c.CredentialId).IsUnique();

            entity.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Name).HasMaxLength(100).IsRequired();
            entity.Property(m => m.Email).HasMaxLength(100).IsRequired();
            entity.Property(m => m.Message).HasMaxLength(1000).IsRequired();
            entity.HasIndex(m => m.CreatedAt);
        });

        modelBuilder.Entity<AvailableSlot>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Date).IsRequired();
            entity.Property(s => s.StartTime).IsRequired();
            entity.HasIndex(s => new { s.Date, s.StartTime }).IsUnique();
            entity.HasIndex(s => s.IsBooked);
        });

        modelBuilder.Entity<BookingRequest>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.GuestName).HasMaxLength(100).IsRequired();
            entity.Property(b => b.GuestEmail).HasMaxLength(100).IsRequired();
            entity.Property(b => b.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(b => b.Status).HasConversion<int>();
            entity.HasIndex(b => b.RequestedDateTime);
            entity.HasIndex(b => b.Status);
            entity.HasIndex(b => b.AvailableSlotId).IsUnique();

            entity.HasOne(b => b.AvailableSlot)
                .WithOne(s => s.BookingRequest)
                .HasForeignKey<BookingRequest>(b => b.AvailableSlotId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
