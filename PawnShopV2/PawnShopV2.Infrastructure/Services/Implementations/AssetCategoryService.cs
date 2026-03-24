using Microsoft.EntityFrameworkCore;
using PawnShopV2.Application.DTOs.AssetCategory;
using PawnShopV2.Application.DTOs.Common;
using PawnShopV2.Application.Services.Interfaces;
using PawnShopV2.Domain.Entities;
using PawnShopV2.Infrastructure.Data;

namespace PawnShopV2.Infrastructure.Services.Implementations;

public class AssetCategoryService : IAssetCategoryService
{
    private readonly PawnShopDbContext _context;

    public AssetCategoryService(PawnShopDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<AssetCategoryDto>>> GetAllAsync()
    {
        var categories = await _context.AssetCategories
            .OrderBy(c => c.Name)
            .ToListAsync();

        return ApiResponse<List<AssetCategoryDto>>.Ok(categories.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<List<AssetCategoryDto>>> GetActiveAsync()
    {
        var categories = await _context.AssetCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return ApiResponse<List<AssetCategoryDto>>.Ok(categories.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<AssetCategoryDto>> GetByIdAsync(int id)
    {
        var category = await _context.AssetCategories.FindAsync(id);
        if (category == null)
        {
            return ApiResponse<AssetCategoryDto>.Fail("Không tìm thấy danh mục");
        }

        return ApiResponse<AssetCategoryDto>.Ok(MapToDto(category));
    }

    public async Task<ApiResponse<AssetCategoryDto>> CreateAsync(CreateAssetCategoryRequest request)
    {
        if (await _context.AssetCategories.AnyAsync(c => c.Name == request.Name))
        {
            return ApiResponse<AssetCategoryDto>.Fail("Tên danh mục đã tồn tại");
        }

        var category = new AssetCategory
        {
            Name = request.Name,
            Description = request.Description,
            DefaultInterestRate = request.DefaultInterestRate,
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now
        };

        await _context.AssetCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        return ApiResponse<AssetCategoryDto>.Ok(MapToDto(category), "Tạo danh mục thành công");
    }

    public async Task<ApiResponse<AssetCategoryDto>> UpdateAsync(int id, UpdateAssetCategoryRequest request)
    {
        var category = await _context.AssetCategories.FindAsync(id);
        if (category == null)
        {
            return ApiResponse<AssetCategoryDto>.Fail("Không tìm thấy danh mục");
        }

        if (await _context.AssetCategories.AnyAsync(c => c.Name == request.Name && c.Id != id))
        {
            return ApiResponse<AssetCategoryDto>.Fail("Tên danh mục đã tồn tại");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.DefaultInterestRate = request.DefaultInterestRate;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return ApiResponse<AssetCategoryDto>.Ok(MapToDto(category), "Cập nhật danh mục thành công");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var category = await _context.AssetCategories
            .Include(c => c.Assets)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return ApiResponse<bool>.Fail("Không tìm thấy danh mục");
        }

        if (category.Assets.Any())
        {
            return ApiResponse<bool>.Fail("Không thể xóa danh mục đang có tài sản");
        }

        _context.AssetCategories.Remove(category);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Xóa danh mục thành công");
    }

    private static AssetCategoryDto MapToDto(AssetCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        DefaultInterestRate = category.DefaultInterestRate,
        IsActive = category.IsActive,
        CreatedAt = category.CreatedAt
    };
}
