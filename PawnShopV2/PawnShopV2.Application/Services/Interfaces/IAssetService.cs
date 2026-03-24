using PawnShopV2.Application.DTOs.Asset;
using PawnShopV2.Application.DTOs.Common;

namespace PawnShopV2.Application.Services.Interfaces;

public interface IAssetService
{
    Task<ApiResponse<PagedResult<AssetDto>>> GetAllAsync(int page, int pageSize, int? categoryId = null, int? status = null);
    Task<ApiResponse<AssetDto>> GetByIdAsync(int id);
    Task<ApiResponse<List<AssetDto>>> GetAvailableAsync();
    Task<ApiResponse<AssetDto>> CreateAsync(CreateAssetRequest request);
    Task<ApiResponse<AssetDto>> UpdateAsync(int id, UpdateAssetRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
