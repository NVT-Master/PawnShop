namespace PawnShopV2.Domain.Entities;

public class ContractAsset
{
    public int ContractId { get; set; }
    public int AssetId { get; set; }
    public decimal? AssetValueAtContract { get; set; }

    // Navigation
    public Contract Contract { get; set; } = null!;
    public Asset Asset { get; set; } = null!;
}
