namespace PawnShopV2.Application.DTOs.Asset;

public class AssetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
    public decimal EstimatedValue { get; set; }
    public int Status { get; set; }
    public string? ImageData { get; set; }
    public string? LicensePlate { get; set; }
    public string? IMEI { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAssetRequest
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Description { get; set; }
    public decimal EstimatedValue { get; set; }
    public string? ImageData { get; set; }
    public string? LicensePlate { get; set; }
    public string? IMEI { get; set; }
    public string? SerialNumber { get; set; }
}

public class UpdateAssetRequest
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Description { get; set; }
    public decimal EstimatedValue { get; set; }
    public int Status { get; set; }
    public string? ImageData { get; set; }
    public string? LicensePlate { get; set; }
    public string? IMEI { get; set; }
    public string? SerialNumber { get; set; }
}
