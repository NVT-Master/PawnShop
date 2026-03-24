using PawnShopV2.Application.DTOs.Common;
using PawnShopV2.Application.DTOs.Contract;

namespace PawnShopV2.Application.Services.Interfaces;

public interface IContractService
{
    Task<ApiResponse<PagedResult<ContractDto>>> GetAllAsync(int page, int pageSize, int? customerId = null, int? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<ApiResponse<ContractDto>> GetByIdAsync(int id);
    Task<ApiResponse<ContractDto>> GetByCodeAsync(string code);
    Task<ApiResponse<List<ContractDto>>> GetByCustomerIdAsync(int customerId);
    Task<ApiResponse<List<ContractDto>>> GetDueSoonAsync(int days);
    Task<ApiResponse<List<ContractDto>>> GetOverdueAsync();
    Task<ApiResponse<ContractDto>> CreateAsync(CreateContractRequest request);
    Task<ApiResponse<ContractDto>> UpdateAsync(int id, UpdateContractRequest request);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<ApiResponse<InterestCalculationDto>> CalculateInterestAsync(int id);
    Task<ApiResponse<ContractDto>> ExtendAsync(int id, ExtendContractRequest request);
    Task<ApiResponse<ContractDto>> RedeemAsync(int id, RedeemContractRequest request);
    Task<ApiResponse<ContractDto>> ForfeitAsync(int id, ForfeitContractRequest request);
    Task<ApiResponse<List<ContractDto>>> PublicLookupAsync(PublicLookupRequest request);
    Task<ApiResponse<ContractSoftCopyDto>> GetSoftCopyAsync(int id);
    Task<ApiResponse<List<ContractDto>>> GetMyContractsAsync(int customerId);
}
