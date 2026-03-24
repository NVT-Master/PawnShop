using PawnShopV2.Domain.Enums;

namespace PawnShopV2.Domain.Entities;

public class PaymentHistory
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public decimal Amount { get; set; }
    public decimal? PrincipalAmount { get; set; }
    public decimal? InterestAmount { get; set; }
    public decimal? PenaltyAmount { get; set; }
    public PaymentType PaymentType { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Contract Contract { get; set; } = null!;
}
