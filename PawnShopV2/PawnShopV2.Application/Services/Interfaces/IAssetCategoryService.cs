using PawnShopV2.Application.DTOs.AssetCategory;
using PawnShopV2.Application.DTOs.Common;

namespace PawnShopV2.Application.Services.Interfaces;

public interface IAssetCategoryService
{
    Task<ApiResponse<List<AssetCategoryDto>>> GetAllAsync();
    Task<ApiResponse<List<AssetCategoryDto>>> GetActiveAsync();
    Task<ApiResponse<AssetCategoryDto>> GetByIdAsync(int id);
    Task<ApiResponse<AssetCategoryDto>> CreateAsync(CreateAssetCategoryRequest request);
    Task<ApiResponse<AssetCategoryDto>> UpdateAsync(int id, UpdateAssetCategoryRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
