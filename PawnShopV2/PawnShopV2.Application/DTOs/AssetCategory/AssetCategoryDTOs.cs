namespace PawnShopV2.Application.DTOs.AssetCategory;

public class AssetCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultInterestRate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAssetCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultInterestRate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateAssetCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DefaultInterestRate { get; set; }
    public bool IsActive { get; set; }
}
