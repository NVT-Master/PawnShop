using Microsoft.EntityFrameworkCore;
using PawnShopV2.Domain.Entities;

namespace PawnShopV2.Infrastructure.Data;

public class PawnShopDbContext : DbContext
{
    public PawnShopDbContext(DbContextOptions<PawnShopDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractAsset> ContractAssets => Set<ContractAsset>();
    public DbSet<PaymentHistory> PaymentHistories => Set<PaymentHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var isSqlite = string.Equals(Database.ProviderName, "Microsoft.EntityFrameworkCore.Sqlite", StringComparison.Ordinal);
        var largeTextColumnType = isSqlite ? "TEXT" : "NVARCHAR(MAX)";

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.RefreshToken).HasMaxLength(256);

            entity.HasOne(e => e.Customer)
                .WithOne(c => c.User)
                .HasForeignKey<User>(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CitizenId).IsUnique();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CitizenId).HasMaxLength(20).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(15).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FaceImageData).HasColumnType(largeTextColumnType);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        // AssetCategory
        modelBuilder.Entity<AssetCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DefaultInterestRate).HasPrecision(10, 4);
        });

        // Asset
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.EstimatedValue).HasPrecision(18, 2);
            entity.Property(e => e.ImageData).HasColumnType(largeTextColumnType);
            entity.Property(e => e.LicensePlate).HasMaxLength(20);
            entity.Property(e => e.IMEI).HasMaxLength(50);
            entity.Property(e => e.SerialNumber).HasMaxLength(100);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Assets)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Contract
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContractCode).IsUnique();
            entity.Property(e => e.ContractCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LoanAmount).HasPrecision(18, 2);
            entity.Property(e => e.InterestRate).HasPrecision(10, 4);
            entity.Property(e => e.OverdueInterestRate).HasPrecision(10, 4);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Contracts)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.OriginalContract)
                .WithMany()
                .HasForeignKey(e => e.OriginalContractId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ContractAsset (Many-to-Many)
        modelBuilder.Entity<ContractAsset>(entity =>
        {
            entity.HasKey(e => new { e.ContractId, e.AssetId });
            entity.Property(e => e.AssetValueAtContract).HasPrecision(18, 2);

            entity.HasOne(e => e.Contract)
                .WithMany(c => c.ContractAssets)
                .HasForeignKey(e => e.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Asset)
                .WithMany(a => a.ContractAssets)
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PaymentHistory
        modelBuilder.Entity<PaymentHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.PrincipalAmount).HasPrecision(18, 2);
            entity.Property(e => e.InterestAmount).HasPrecision(18, 2);
            entity.Property(e => e.PenaltyAmount).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Contract)
                .WithMany(c => c.PaymentHistories)
                .HasForeignKey(e => e.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
