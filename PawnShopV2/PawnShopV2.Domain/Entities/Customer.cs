using PawnShopV2.Domain.Enums;

namespace PawnShopV2.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string CitizenId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Email { get; set; }
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    public string? FaceImageData { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public User? User { get; set; }
}
