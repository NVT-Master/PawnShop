using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PawnShopV2.Domain.Entities;
using PawnShopV2.Domain.Enums;
using PawnShopV2.Infrastructure.Data;

namespace PawnShopV2.Infrastructure.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(PawnShopDbContext context, IConfiguration configuration)
    {
        await SeedAssetCategoriesAsync(context);
        await SeedAdminUserAsync(context, configuration);
        await SeedSampleDataAsync(context);
    }

    private static async Task SeedAssetCategoriesAsync(PawnShopDbContext context)
    {
        if (await context.AssetCategories.AnyAsync()) return;

        var categories = new List<AssetCategory>
        {
            new() { Name = "Xe máy", Description = "Xe máy các loại", DefaultInterestRate = 0.3m, IsActive = true },
            new() { Name = "Ô tô", Description = "Ô tô, xe hơi", DefaultInterestRate = 0.25m, IsActive = true },
            new() { Name = "Điện thoại", Description = "Điện thoại di động", DefaultInterestRate = 0.4m, IsActive = true },
            new() { Name = "Laptop", Description = "Máy tính xách tay", DefaultInterestRate = 0.35m, IsActive = true },
            new() { Name = "Trang sức", Description = "Vàng, bạc, kim cương", DefaultInterestRate = 0.2m, IsActive = true },
            new() { Name = "Khác", Description = "Các loại tài sản khác", DefaultInterestRate = 0.5m, IsActive = true }
        };

        await context.AssetCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(PawnShopDbContext context, IConfiguration configuration)
    {
        if (await context.Users.AnyAsync(u => u.Username == "admin")) return;
        var adminPassword = configuration["SeedData:AdminPassword"] ?? "Admin@123";

        var admin = new User
        {
            Username = "admin",
            PasswordHash = HashPassword(adminPassword),
            Role = UserRole.Owner,
            FullName = "Quản trị viên",
            Email = "admin@pawnshop.com",
            CreatedAt = DateTime.Now
        };

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSampleDataAsync(PawnShopDbContext context)
    {
        if (await context.Customers.AnyAsync()) return;

        // Sample customers - CreatedAt phải trước hoặc bằng ngày bắt đầu hợp đồng
        var customers = new List<Customer>
        {
            new()
            {
                FullName = "Nguyễn Văn An",
                CitizenId = "001234567890",
                PhoneNumber = "0901234567",
                Address = "123 Đường ABC, Quận 1, TP.HCM",
                Email = "nguyenvanan@gmail.com",
                Status = CustomerStatus.Active,
                CreatedAt = DateTime.Now.AddDays(-15) // Tạo trước hợp đồng 5 ngày
            },
            new()
            {
                FullName = "Trần Thị Bình",
                CitizenId = "001234567891",
                PhoneNumber = "0912345678",
                Address = "456 Đường XYZ, Quận 3, TP.HCM",
                Email = "tranthibinh@gmail.com",
                Status = CustomerStatus.Active,
                CreatedAt = DateTime.Now.AddDays(-30) // Tạo trước hợp đồng 5 ngày
            },
            new()
            {
                FullName = "Lê Văn Cường",
                CitizenId = "001234567892",
                PhoneNumber = "0923456789",
                Address = "789 Đường DEF, Quận 5, TP.HCM",
                Status = CustomerStatus.Active,
                CreatedAt = DateTime.Now.AddDays(-45) // Tạo trước hợp đồng 5 ngày
            }
        };

        await context.Customers.AddRangeAsync(customers);
        await context.SaveChangesAsync();

        // Get categories
        var phoneCategory = await context.AssetCategories.FirstAsync(c => c.Name == "Điện thoại");
        var laptopCategory = await context.AssetCategories.FirstAsync(c => c.Name == "Laptop");
        var motorbikeCategory = await context.AssetCategories.FirstAsync(c => c.Name == "Xe máy");

        // Sample assets
        var assets = new List<Asset>
        {
            new()
            {
                Name = "iPhone 15 Pro Max",
                CategoryId = phoneCategory.Id,
                Description = "iPhone 15 Pro Max 256GB, màu đen",
                EstimatedValue = 25000000,
                Status = AssetStatus.Available,
                IMEI = "123456789012345",
                CreatedAt = DateTime.Now.AddDays(-10)
            },
            new()
            {
                Name = "MacBook Pro 14",
                CategoryId = laptopCategory.Id,
                Description = "MacBook Pro 14 inch M3, 16GB RAM",
                EstimatedValue = 45000000,
                Status = AssetStatus.Available,
                SerialNumber = "C02ABC123",
                CreatedAt = DateTime.Now.AddDays(-25)
            },
            new()
            {
                Name = "Honda SH 150i",
                CategoryId = motorbikeCategory.Id,
                Description = "Honda SH 150i đời 2023, màu đen",
                EstimatedValue = 80000000,
                Status = AssetStatus.Available,
                LicensePlate = "59A1-12345",
                CreatedAt = DateTime.Now.AddDays(-40)
            }
        };

        await context.Assets.AddRangeAsync(assets);
        await context.SaveChangesAsync();

        // Sample contracts
        var savedCustomers = await context.Customers.ToListAsync();
        var savedAssets = await context.Assets.ToListAsync();

        var contracts = new List<Contract>
        {
            new()
            {
                ContractCode = "HD-2024-001",
                CustomerId = savedCustomers[0].Id,
                LoanAmount = 20000000,
                InterestRate = 0.4m,
                OverdueInterestRate = 0.6m,
                StartDate = DateTime.Now.AddDays(-10),
                DueDate = DateTime.Now.AddDays(20),
                Status = ContractStatus.Active,
                ExtensionCount = 0,
                CreatedAt = DateTime.Now.AddDays(-10)
            },
            new()
            {
                ContractCode = "HD-2024-002",
                CustomerId = savedCustomers[1].Id,
                LoanAmount = 40000000,
                InterestRate = 0.35m,
                OverdueInterestRate = 0.55m,
                StartDate = DateTime.Now.AddDays(-25),
                DueDate = DateTime.Now.AddDays(5),
                Status = ContractStatus.Active,
                ExtensionCount = 0,
                CreatedAt = DateTime.Now.AddDays(-25)
            },
            new()
            {
                ContractCode = "HD-2024-003",
                CustomerId = savedCustomers[2].Id,
                LoanAmount = 70000000,
                InterestRate = 0.3m,
                OverdueInterestRate = 0.5m,
                StartDate = DateTime.Now.AddDays(-40),
                DueDate = DateTime.Now.AddDays(-5),
                Status = ContractStatus.Overdue,
                ExtensionCount = 0,
                CreatedAt = DateTime.Now.AddDays(-40)
            }
        };

        await context.Contracts.AddRangeAsync(contracts);
        await context.SaveChangesAsync();

        // Link assets to contracts
        var savedContracts = await context.Contracts.ToListAsync();
        var contractAssets = new List<ContractAsset>
        {
            new() { ContractId = savedContracts[0].Id, AssetId = savedAssets[0].Id, AssetValueAtContract = 20000000 },
            new() { ContractId = savedContracts[1].Id, AssetId = savedAssets[1].Id, AssetValueAtContract = 40000000 },
            new() { ContractId = savedContracts[2].Id, AssetId = savedAssets[2].Id, AssetValueAtContract = 70000000 }
        };

        // Update asset status to Pawned
        foreach (var asset in savedAssets)
        {
            asset.Status = AssetStatus.Pawned;
        }

        await context.ContractAssets.AddRangeAsync(contractAssets);
        await context.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
