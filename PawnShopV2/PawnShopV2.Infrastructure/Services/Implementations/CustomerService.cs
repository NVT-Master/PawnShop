using Microsoft.EntityFrameworkCore;
using PawnShopV2.Application.DTOs.Common;
using PawnShopV2.Application.DTOs.Customer;
using PawnShopV2.Application.Services.Interfaces;
using PawnShopV2.Domain.Entities;
using PawnShopV2.Domain.Enums;
using PawnShopV2.Infrastructure.Data;

namespace PawnShopV2.Infrastructure.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly PawnShopDbContext _context;

    public CustomerService(PawnShopDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<CustomerDto>>> GetAllAsync(int page, int pageSize, string? search = null)
    {
        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.FullName.Contains(search) ||
                                     c.CitizenId.Contains(search) ||
                                     c.PhoneNumber.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<CustomerDto>>.Ok(new PagedResult<CustomerDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<CustomerDto>> GetByIdAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return ApiResponse<CustomerDto>.Fail("Không tìm thấy khách hàng");
        }

        return ApiResponse<CustomerDto>.Ok(MapToDto(customer));
    }

    public async Task<ApiResponse<CustomerDto>> GetByCitizenIdAsync(string citizenId)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CitizenId == citizenId);
        if (customer == null)
        {
            return ApiResponse<CustomerDto>.Fail("Không tìm thấy khách hàng");
        }

        return ApiResponse<CustomerDto>.Ok(MapToDto(customer));
    }

    public async Task<ApiResponse<CustomerDto>> CreateAsync(CreateCustomerRequest request)
    {
        if (await _context.Customers.AnyAsync(c => c.CitizenId == request.CitizenId))
        {
            return ApiResponse<CustomerDto>.Fail("CCCD/CMND đã tồn tại trong hệ thống");
        }

        var customer = new Customer
        {
            FullName = request.FullName,
            CitizenId = request.CitizenId,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            Email = request.Email,
            FaceImageData = request.FaceImageData,
            Notes = request.Notes,
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.Now
        };

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();

        return ApiResponse<CustomerDto>.Ok(MapToDto(customer), "Tạo khách hàng thành công");
    }

    public async Task<ApiResponse<CustomerDto>> UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return ApiResponse<CustomerDto>.Fail("Không tìm thấy khách hàng");
        }

        customer.FullName = request.FullName;
        customer.PhoneNumber = request.PhoneNumber;
        customer.Address = request.Address;
        customer.Email = request.Email;
        customer.Status = (CustomerStatus)request.Status;
        customer.Notes = request.Notes;
        customer.UpdatedAt = DateTime.Now;

        if (!string.IsNullOrEmpty(request.FaceImageData))
        {
            customer.FaceImageData = request.FaceImageData;
        }

        await _context.SaveChangesAsync();

        return ApiResponse<CustomerDto>.Ok(MapToDto(customer), "Cập nhật khách hàng thành công");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Contracts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return ApiResponse<bool>.Fail("Không tìm thấy khách hàng");
        }

        if (customer.Contracts.Any(c => c.Status == ContractStatus.Active || c.Status == ContractStatus.Overdue))
        {
            return ApiResponse<bool>.Fail("Không thể xóa khách hàng đang có hợp đồng hoạt động");
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Xóa khách hàng thành công");
    }

    private static CustomerDto MapToDto(Customer customer) => new()
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
}
