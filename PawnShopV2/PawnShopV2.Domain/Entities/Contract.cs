using PawnShopV2.Domain.Enums;

namespace PawnShopV2.Domain.Entities;

public class Contract
{
    public int Id { get; set; }
    public string ContractCode { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal OverdueInterestRate { get; set; } = 0.5m;
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Active;
    public string? Notes { get; set; }
    public int ExtensionCount { get; set; } = 0;
    public int? OriginalContractId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public Contract? OriginalContract { get; set; }
    public ICollection<ContractAsset> ContractAssets { get; set; } = new List<ContractAsset>();
    public ICollection<PaymentHistory> PaymentHistories { get; set; } = new List<PaymentHistory>();
}
