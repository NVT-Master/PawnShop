using PawnShopV2.Application.DTOs.Common;
using PawnShopV2.Application.DTOs.Customer;

namespace PawnShopV2.Application.Services.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<PagedResult<CustomerDto>>> GetAllAsync(int page, int pageSize, string? search = null);
    Task<ApiResponse<CustomerDto>> GetByIdAsync(int id);
    Task<ApiResponse<CustomerDto>> GetByCitizenIdAsync(string citizenId);
    Task<ApiResponse<CustomerDto>> CreateAsync(CreateCustomerRequest request);
    Task<ApiResponse<CustomerDto>> UpdateAsync(int id, UpdateCustomerRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
}
