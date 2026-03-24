using PawnShopV2.Domain.Enums;

namespace PawnShopV2.Domain.Entities;

public class Asset
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Description { get; set; }
    public decimal EstimatedValue { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Available;
    public string? ImageData { get; set; }
    public string? LicensePlate { get; set; }
    public string? IMEI { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public AssetCategory Category { get; set; } = null!;
    public ICollection<ContractAsset> ContractAssets { get; set; } = new List<ContractAsset>();
}
