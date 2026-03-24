using Microsoft.EntityFrameworkCore;
using PawnShopV2.Application.DTOs.Asset;
using PawnShopV2.Application.DTOs.Common;
using PawnShopV2.Application.Services.Interfaces;
using PawnShopV2.Domain.Entities;
using PawnShopV2.Domain.Enums;
using PawnShopV2.Infrastructure.Data;

namespace PawnShopV2.Infrastructure.Services.Implementations;

public class AssetService : IAssetService
{
    private readonly PawnShopDbContext _context;

    public AssetService(PawnShopDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<AssetDto>>> GetAllAsync(int page, int pageSize, int? categoryId = null, int? status = null)
    {
        var query = _context.Assets.Include(a => a.Category).AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == (AssetStatus)status.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<AssetDto>>.Ok(new PagedResult<AssetDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<AssetDto>> GetByIdAsync(int id)
    {
        var asset = await _context.Assets
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null)
        {
            return ApiResponse<AssetDto>.Fail("Không tìm thấy tài sản");
        }

        return ApiResponse<AssetDto>.Ok(MapToDto(asset));
    }

    public async Task<ApiResponse<List<AssetDto>>> GetAvailableAsync()
    {
        var assets = await _context.Assets
            .Include(a => a.Category)
            .Where(a => a.Status == AssetStatus.Available)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<AssetDto>>.Ok(assets.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<AssetDto>> CreateAsync(CreateAssetRequest request)
    {
        var category = await _context.AssetCategories.FindAsync(request.CategoryId);
        if (category == null)
        {
            return ApiResponse<AssetDto>.Fail("Danh mục không tồn tại");
        }

        var asset = new Asset
        {
            Name = request.Name,
            CategoryId = request.CategoryId,
            Description = request.Description,
            EstimatedValue = request.EstimatedValue,
            Status = AssetStatus.Available,
            ImageData = request.ImageData,
            LicensePlate = request.LicensePlate,
            IMEI = request.IMEI,
            SerialNumber = request.SerialNumber,
            CreatedAt = DateTime.Now
        };

        await _context.Assets.AddAsync(asset);
        await _context.SaveChangesAsync();

        asset.Category = category;
        return ApiResponse<AssetDto>.Ok(MapToDto(asset), "Tạo tài sản thành công");
    }

    public async Task<ApiResponse<AssetDto>> UpdateAsync(int id, UpdateAssetRequest request)
    {
        var asset = await _context.Assets
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null)
        {
            return ApiResponse<AssetDto>.Fail("Không tìm thấy tài sản");
        }

        var category = await _context.AssetCategories.FindAsync(request.CategoryId);
        if (category == null)
        {
            return ApiResponse<AssetDto>.Fail("Danh mục không tồn tại");
        }

        asset.Name = request.Name;
        asset.CategoryId = request.CategoryId;
        asset.Description = request.Description;
        asset.EstimatedValue = request.EstimatedValue;
        asset.Status = (AssetStatus)request.Status;
        asset.LicensePlate = request.LicensePlate;
        asset.IMEI = request.IMEI;
        asset.SerialNumber = request.SerialNumber;
        asset.UpdatedAt = DateTime.Now;

        if (!string.IsNullOrEmpty(request.ImageData))
        {
            asset.ImageData = request.ImageData;
        }

        await _context.SaveChangesAsync();

        asset.Category = category;
        return ApiResponse<AssetDto>.Ok(MapToDto(asset), "Cập nhật tài sản thành công");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var asset = await _context.Assets
            .Include(a => a.ContractAssets)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (asset == null)
        {
            return ApiResponse<bool>.Fail("Không tìm thấy tài sản");
        }

        if (asset.Status == AssetStatus.Pawned)
        {
            return ApiResponse<bool>.Fail("Không thể xóa tài sản đang được cầm cố");
        }

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Xóa tài sản thành công");
    }

    private static AssetDto MapToDto(Asset asset) => new()
    {
        Id = asset.Id,
        Name = asset.Name,
        CategoryId = asset.CategoryId,
        CategoryName = asset.Category?.Name,
        Description = asset.Description,
        EstimatedValue = asset.EstimatedValue,
        Status = (int)asset.Status,
        ImageData = asset.ImageData,
        LicensePlate = asset.LicensePlate,
        IMEI = asset.IMEI,
        SerialNumber = asset.SerialNumber,
        CreatedAt = asset.CreatedAt
    };
}
