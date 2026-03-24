using Microsoft.EntityFrameworkCore;
using PawnShopV2.Application.DTOs.Asset;
using PawnShopV2.Application.DTOs.Common;
using PawnShopV2.Application.DTOs.Contract;
using PawnShopV2.Application.DTOs.Customer;
using PawnShopV2.Application.Services.Interfaces;
using PawnShopV2.Domain.Entities;
using PawnShopV2.Domain.Enums;
using PawnShopV2.Infrastructure.Data;

namespace PawnShopV2.Application.Services.Implementations;

public class ContractService : IContractService
{
    private readonly PawnShopDbContext _context;

    public ContractService(PawnShopDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<ContractDto>>> GetAllAsync(int page, int pageSize, int? customerId = null, int? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.ContractAssets).ThenInclude(ca => ca.Asset).ThenInclude(a => a.Category)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(c => c.CustomerId == customerId.Value);

        if (status.HasValue)
            query = query.Where(c => c.Status == (ContractStatus)status.Value);

        if (fromDate.HasValue)
            query = query.Where(c => c.StartDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(c => c.StartDate <= toDate.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<ContractDto>>.Ok(new PagedResult<ContractDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<ContractDto>> GetByIdAsync(int id)
    {
        var contract = await GetContractWithDetailsAsync(id);
        if (contract == null)
            return ApiResponse<ContractDto>.Fail("Không tìm thấy hợp đồng");

        return ApiResponse<ContractDto>.Ok(MapToDto(contract));
    }

    public async Task<ApiResponse<ContractDto>> GetByCodeAsync(string code)
    {
        var contract = await _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.ContractAssets).ThenInclude(ca => ca.Asset).ThenInclude(a => a.Category)
            .FirstOrDefaultAsync(c => c.ContractCode == code);

        if (contract == null)
            return ApiResponse<ContractDto>.Fail("Không tìm thấy hợp đồng");

        return ApiResponse<ContractDto>.Ok(MapToDto(contract));
    }

    public async Task<ApiResponse<List<ContractDto>>> GetByCustomerIdAsync(int customerId)
    {
        var contracts = await _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.ContractAssets).ThenInclude(ca => ca.Asset).ThenInclude(a => a.Category)
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<ContractDto>>.Ok(contracts.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<List<ContractDto>>> GetDueSoonAsync(int days)
    {
        var dueDate = DateTime.Now.AddDays(days);
        var contracts = await _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.ContractAssets).ThenInclude(ca => ca.Asset).ThenInclude(a => a.Category)
            .Where(c => (c.Status == ContractStatus.Active || c.Status == ContractStatus.Extended) &&
                        c.DueDate <= dueDate && c.DueDate >= DateTime.Now)
            .OrderBy(c => c.DueDate)
            .ToListAsync();

        return ApiResponse<List<ContractDto>>.Ok(contracts.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<List<ContractDto>>> GetOverdueAsync()
    {
        var contracts = await _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.ContractAssets).ThenInclude(ca => ca.Asset).ThenInclude(a => a.Category)
            .Where(c => (c.Status == ContractStatus.Active || c.Status == ContractStatus.Extended ||
                        c.Status == ContractStatus.Overdue) && c.DueDate < DateTime.Now)
            .OrderBy(c => c.DueDate)
            .ToListAsync();

        // Update status to Overdue if needed
        foreach (var contract in contracts.Where(c => c.Status != ContractStatus.Overdue))
        {
            contract.Status = ContractStatus.Overdue;
            contract.UpdatedAt = DateTime.Now;
        }
        await _context.SaveChangesAsync();

        return ApiResponse<List<ContractDto>>.Ok(contracts.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<ContractDto>> CreateAsync(CreateContractRequest request)
    {
        var customer = await _context.Customers.FindAsync(request.CustomerId);
        if (customer == null)
            return ApiResponse<ContractDto>.Fail("Không tìm thấy khách hàng");

        // Generate contract code
        var today = DateTime.Now;
        var countToday = await _context.Contracts.CountAsync(c => c.CreatedAt.Date == today.Date);
        var contractCode = $"HD-{today:yyyyMMdd}-{(countToday + 1):D4}";

        var contract = new Contract
        {
            ContractCode = contractCode,
            CustomerId = request.CustomerId,
            LoanAmount = request.LoanAmount,
            InterestRate = request.InterestRate,
            OverdueInterestRate = 0.5m,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            Status = ContractStatus.Active,
            Notes = request.Notes,
            CreatedAt = DateTime.Now
        };

        await _context.Contracts.AddAsync(contract);
        await _context.SaveChangesAsync();

        // Handle assets
        foreach (var assetReq in request.Assets)
        {
            Asset asset;
            if (assetReq.AssetId.HasValue)
            {
                asset = await _context.Assets.FindAsync(assetReq.AssetId.Value)
                    ?? throw new Exception("Tài sản không tồn tại");

                if (asset.Status != AssetStatus.Available)
                    return ApiResponse<ContractDto>.Fail($"Tài sản '{asset.Name}' đã được cầm cố");
            }
            else
            {
                // Create new asset
                asset = new Asset
                {
                    Name = assetReq.Name ?? "Tài sản mới",
                    CategoryId = assetReq.CategoryId ?? 6, // Default to "Khác"
                    Description = assetReq.Description,
                    EstimatedValue = assetReq.EstimatedValue ?? 0,
                    ImageData = assetReq.ImageData,
                    LicensePlate = assetReq.LicensePlate,
                    IMEI = assetReq.IMEI,
                    SerialNumber = assetReq.SerialNumber,
                    Status = AssetStatus.Available,
                    CreatedAt = DateTime.Now
                };
                await _context.Assets.AddAsync(asset);
                await _context.SaveChangesAsync();
            }

            asset.Status = AssetStatus.Pawned;

            var contractAsset = new ContractAsset
            {
                ContractId = contract.Id,
                AssetId = asset.Id,
                AssetValueAtContract = asset.EstimatedValue
            };
            await _context.ContractAssets.AddAsync(contractAsset);
        }

        await _context.SaveChangesAsync();

        var result = await GetContractWithDetailsAsync(contract.Id);
        return ApiResponse<ContractDto>.Ok(MapToDto(result!), "Tạo hợp đồng thành công");
    }

    public async Task<ApiResponse<ContractDto>> UpdateAsync(int id, UpdateContractRequest request)
    {
        var contract = await GetContractWithDetailsAsync(id);
        if (contract == null)
            return ApiResponse<ContractDto>.Fail("Không tìm thấy hợp đồng");

        if (contract.Status == ContractStatus.Closed || contract.Status == ContractStatus.Forfeited)
            return ApiResponse<ContractDto>.Fail("Không thể chỉnh sửa hợp đồng đã đóng");

        contract.InterestRate = request.InterestRate;
        contract.DueDate = request.DueDate;
        contract.Notes = request.Notes;
        contract.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return ApiResponse<ContractDto>.Ok(MapToDto(contract), "Cập nhật hợp đồng thành công");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.ContractAssets)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null)
            return ApiResponse<bool>.Fail("Không tìm thấy hợp đồng");

        if (contract.Status != ContractStatus.Closed && contract.Status != ContractStatus.Cancelled)
            return ApiResponse<bool>.Fail("Chỉ có thể xóa hợp đồng đã đóng hoặc hủy");

        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Xóa hợp đồng thành công");
    }

    public async Task<ApiResponse<InterestCalculationDto>> CalculateInterestAsync(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null)
            return ApiResponse<InterestCalculationDto>.Fail("Không tìm thấy hợp đồng");

        var today = DateTime.Now.Date;
        var startDate = contract.StartDate.Date;
        var dueDate = contract.DueDate.Date;

        var daysInTerm = (int)(Math.Min(dueDate, today) - startDate).TotalDays;
        var daysOverdue = today > dueDate ? (int)(today - dueDate).TotalDays : 0;

        if (daysInTerm < 0) daysInTerm = 0;

        var regularInterest = contract.LoanAmount * (contract.InterestRate / 100) * daysInTerm;
        var penaltyInterest = contract.LoanAmount * (contract.OverdueInterestRate / 100) * daysOverdue;
        var totalInterest = regularInterest + penaltyInterest;
        var totalPayment = contract.LoanAmount + totalInterest;

        return ApiResponse<InterestCalculationDto>.Ok(new InterestCalculationDto
        {
            LoanAmount = contract.LoanAmount,
            InterestRate = contract.InterestRate,
            OverdueInterestRate = contract.OverdueInterestRate,
            DaysInTerm = daysInTerm,
            DaysOverdue = daysOverdue,
            RegularInterest = Math.Round(regularInterest, 0),
            PenaltyInterest = Math.Round(penaltyInterest, 0),
            TotalInterest = Math.Round(totalInterest, 0),
            TotalPayment = Math.Round(totalPayment, 0)
        });
    }

    public async Task<ApiResponse<ContractDto>> ExtendAsync(int id, ExtendContractRequest request)
    {
        var contract = await GetContractWithDetailsAsync(id);
        if (contract == null)
            return ApiResponse<ContractDto>.Fail("Không tìm thấy hợp đồng");

        if (contract.Status != ContractStatus.Active && contract.Status != ContractStatus.Extended &&
            contract.Status != ContractStatus.Overdue)
            return ApiResponse<ContractDto>.Fail("Chỉ có thể gia hạn hợp đồng đang hoạt động hoặc quá hạn");

        // Calculate interest to be paid
        var interestResult = await CalculateInterestAsync(id);
        if (!interestResult.Success)
            return ApiResponse<ContractDto>.Fail(interestResult.Message!);

        // Record payment for interest
        var payment = new PaymentHistory
        {
            ContractId = contract.Id,
            Amount = interestResult.Data!.TotalInterest,
            InterestAmount = interestResult.Data.RegularInterest,
            PenaltyAmount = interestResult.Data.PenaltyInterest,
            PaymentType = PaymentType.Extension,
            PaymentDate = DateTime.Now,
            Notes = request.Notes,
            CreatedAt = DateTime.Now
        };
        await _context.PaymentHistories.AddAsync(payment);

        // Update contract
        if (request.CapitalizeInterest)
        {
            contract.LoanAmount += interestResult.Data.TotalInterest;
        }

        contract.Status = ContractStatus.Extended;
        contract.ExtensionCount++;
        contract.StartDate = DateTime.Now;
        contract.DueDate = DateTime.Now.AddDays(request.ExtensionDays);
        contract.InterestRate = request.NewInterestRate;
        contract.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        var result = await GetContractWithDetailsAsync(id);
        return ApiResponse<ContractDto>.Ok(MapToDto(result!), "Gia hạn hợp đồng thành công");
    }

    public async Task<ApiResponse<ContractDto>> RedeemAsync(int id, RedeemContractRequest request)
    {
        var contract = await GetContractWithDetailsAsync(id);
        if (contract == null)
            return ApiResponse<ContractDto>.Fail("Không tìm thấy hợp đồng");

        if (contract.Status != ContractStatus.Active && contract.Status != ContractStatus.Extended &&
            contract.Status != ContractStatus.Overdue)
            return ApiResponse<ContractDto>.Fail("Không thể chuộc hợp đồng này");

        var interestResult = await CalculateInterestAsync(id);
        if (!interestResult.Success)
            return ApiResponse<ContractDto>.Fail(interestResult.Message!);

        // Record full payment
        var payment = new PaymentHistory
        {
            ContractId = contract.Id,
            Amount = request.PaymentAmount,
            PrincipalAmount = contract.LoanAmount,
            InterestAmount = interestResult.Data!.RegularInterest,
            PenaltyAmount = interestResult.Data.PenaltyInterest,
            PaymentType = PaymentType.FullPayment,
            PaymentDate = DateTime.Now,
            Notes = request.Notes,
            CreatedAt = DateTime.Now
        };
        await _context.PaymentHistories.AddAsync(payment);

        // Update assets status
        foreach (var ca in contract.ContractAssets)
        {
            ca.Asset.Status = AssetStatus.Redeemed;
            ca.Asset.UpdatedAt = DateTime.Now;
        }

        // Close contract
        contract.Status = ContractStatus.Closed;
        contract.ClosedAt = DateTime.Now;
        contract.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        var result = await GetContractWithDetailsAsync(id);
        return ApiResponse<ContractDto>.Ok(MapToDto(result!), "Chuộc hàng thành công");
    }

    public async Task<ApiResponse<ContractDto>> ForfeitAsync(int id, ForfeitContractRequest request)
    {
        var contract = await GetContractWithDetailsAsync(id);
        if (contract == null)
            return ApiResponse<ContractDto>.Fail("Không tìm thấy hợp đồng");

        if (contract.Status != ContractStatus.Overdue)
            return ApiResponse<ContractDto>.Fail("Chỉ có thể tịch thu tài sản của hợp đồng quá hạn");

        // Update assets status
        foreach (var ca in contract.ContractAssets)
        {
            ca.Asset.Status = AssetStatus.Forfeited;
            ca.Asset.UpdatedAt = DateTime.Now;
        }

        // Update contract
        contract.Status = ContractStatus.Forfeited;
        contract.ClosedAt = DateTime.Now;
        contract.UpdatedAt = DateTime.Now;
        contract.Notes = string.IsNullOrEmpty(contract.Notes)
            ? request.Notes
            : $"{contract.Notes}\n[Tịch thu] {request.Notes}";

        await _context.SaveChangesAsync();

        var result = await GetContractWithDetailsAsync(id);
        return ApiResponse<ContractDto>.Ok(MapToDto(result!), "Tịch thu tài sản thành công");
    }

    public async Task<ApiResponse<List<ContractDto>>> PublicLookupAsync(PublicLookupRequest request)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.CitizenId == request.CitizenId && c.PhoneNumber == request.PhoneNumber);

        if (customer == null)
            return ApiResponse<List<ContractDto>>.Fail("Không tìm thấy thông tin");

        var contracts = await _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.ContractAssets).ThenInclude(ca => ca.Asset).ThenInclude(a => a.Category)
            .Where(c => c.CustomerId == customer.Id)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<ContractDto>>.Ok(contracts.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<ContractSoftCopyDto>> GetSoftCopyAsync(int id)
    {
        var contract = await GetContractWithDetailsAsync(id);
        if (contract == null)
            return ApiResponse<ContractSoftCopyDto>.Fail("Không tìm thấy hợp đồng");

        return ApiResponse<ContractSoftCopyDto>.Ok(new ContractSoftCopyDto
        {
            ContractCode = contract.ContractCode,
            Customer = MapCustomerToDto(contract.Customer),
            Assets = contract.ContractAssets.Select(ca => MapAssetToDto(ca.Asset)).ToList(),
            LoanAmount = contract.LoanAmount,
            InterestRate = contract.InterestRate,
            StartDate = contract.StartDate,
            DueDate = contract.DueDate,
            Notes = contract.Notes,
            CreatedAt = contract.CreatedAt
        });
    }

    public async Task<ApiResponse<List<ContractDto>>> GetMyContractsAsync(int customerId)
    {
        var contracts = await _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.ContractAssets).ThenInclude(ca => ca.Asset).ThenInclude(a => a.Category)
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<ContractDto>>.Ok(contracts.Select(MapToDto).ToList());
    }

    private async Task<Contract?> GetContractWithDetailsAsync(int id)
    {
        return await _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.ContractAssets).ThenInclude(ca => ca.Asset).ThenInclude(a => a.Category)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    private static ContractDto MapToDto(Contract contract) => new()
    {
        Id = contract.Id,
        ContractCode = contract.ContractCode,
        CustomerId = contract.CustomerId,
        Customer = contract.Customer != null ? MapCustomerToDto(contract.Customer) : null,
        LoanAmount = contract.LoanAmount,
        InterestRate = contract.InterestRate,
        OverdueInterestRate = contract.OverdueInterestRate,
        StartDate = contract.StartDate,
        DueDate = contract.DueDate,
        Status = (int)contract.Status,
        Notes = contract.Notes,
        ExtensionCount = contract.ExtensionCount,
        Assets = contract.ContractAssets.Select(ca => MapAssetToDto(ca.Asset)).ToList(),
        CreatedAt = contract.CreatedAt,
        ClosedAt = contract.ClosedAt
    };

    private static CustomerDto MapCustomerToDto(Customer customer) => new()
    {
        Id = customer.Id,
        FullName = customer.FullName,
        CitizenId = customer.CitizenId,
        PhoneNumber = customer.PhoneNumber,
        Address = customer.Address,
        Email = customer.Email,
        Status = (int)customer.Status,
        FaceImageData = customer.FaceImageData,
        Notes = customer.Notes,
        CreatedAt = customer.CreatedAt
    };

    private static AssetDto MapAssetToDto(Asset asset) => new()
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
