using PawnShopV2.Application.DTOs.Asset;
using PawnShopV2.Application.DTOs.Customer;

namespace PawnShopV2.Application.DTOs.Contract;

public class ContractDto
{
    public int Id { get; set; }
    public string ContractCode { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public CustomerDto? Customer { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal OverdueInterestRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public int Status { get; set; }
    public string? Notes { get; set; }
    public int ExtensionCount { get; set; }
    public List<AssetDto> Assets { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public class CreateContractRequest
{
    public int CustomerId { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public List<ContractAssetRequest> Assets { get; set; } = new();
}

public class ContractAssetRequest
{
    public int? AssetId { get; set; }
    public string? Name { get; set; }
    public int? CategoryId { get; set; }
    public string? Description { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string? ImageData { get; set; }
    public string? LicensePlate { get; set; }
    public string? IMEI { get; set; }
    public string? SerialNumber { get; set; }
}

public class UpdateContractRequest
{
    public decimal InterestRate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
}

public class ExtendContractRequest
{
    public int ExtensionDays { get; set; }
    public decimal NewInterestRate { get; set; }
    public bool CapitalizeInterest { get; set; }
    public string? Notes { get; set; }
}

public class RedeemContractRequest
{
    public decimal PaymentAmount { get; set; }
    public string? Notes { get; set; }
}

public class ForfeitContractRequest
{
    public string? Notes { get; set; }
}

public class InterestCalculationDto
{
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal OverdueInterestRate { get; set; }
    public int DaysInTerm { get; set; }
    public int DaysOverdue { get; set; }
    public decimal RegularInterest { get; set; }
    public decimal PenaltyInterest { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalPayment { get; set; }
}

public class PublicLookupRequest
{
    public string CitizenId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ContractSoftCopyDto
{
    public string ContractCode { get; set; } = string.Empty;
    public string ShopName { get; set; } = "Tiệm Cầm Đồ ABC";
    public string ShopAddress { get; set; } = "123 Đường XYZ, Quận 1, TP.HCM";
    public string ShopPhone { get; set; } = "028-1234-5678";
    public CustomerDto Customer { get; set; } = null!;
    public List<AssetDto> Assets { get; set; } = new();
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
