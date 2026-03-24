using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PawnShopV2.Application.DTOs.Auth;
using PawnShopV2.Application.DTOs.Common;
using PawnShopV2.Application.Services.Interfaces;
using PawnShopV2.Domain.Entities;
using PawnShopV2.Domain.Enums;
using PawnShopV2.Infrastructure.Data;
using PawnShopV2.Infrastructure.Services;

namespace PawnShopV2.Infrastructure.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly PawnShopDbContext _context;
    private readonly JwtService _jwtService;

    public AuthService(PawnShopDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return ApiResponse<LoginResponse>.Fail("Tên đăng nhập hoặc mật khẩu không đúng");
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
        await _context.SaveChangesAsync();

        return ApiResponse<LoginResponse>.Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = MapToDto(user)
        });
    }

    public async Task<ApiResponse<UserDto>> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return ApiResponse<UserDto>.Fail("Tên đăng nhập đã tồn tại");
        }

        Customer? customer = null;
        if (!string.IsNullOrEmpty(request.CitizenId))
        {
            customer = await _context.Customers.FirstOrDefaultAsync(c => c.CitizenId == request.CitizenId);
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = HashPassword(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            Role = UserRole.Customer,
            CustomerId = customer?.Id,
            CreatedAt = DateTime.Now
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return ApiResponse<UserDto>.Ok(MapToDto(user), "Đăng ký thành công");
    }

    public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            return ApiResponse<LoginResponse>.Fail("Token không hợp lệ");
        }

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return ApiResponse<LoginResponse>.Fail("Token không hợp lệ");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiry <= DateTime.Now)
        {
            return ApiResponse<LoginResponse>.Fail("Refresh token không hợp lệ hoặc đã hết hạn");
        }

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
        await _context.SaveChangesAsync();

        return ApiResponse<LoginResponse>.Ok(new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            User = MapToDto(user)
        });
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return ApiResponse<bool>.Fail("Không tìm thấy người dùng");
        }

        if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            return ApiResponse<bool>.Fail("Mật khẩu hiện tại không đúng");
        }

        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Đổi mật khẩu thành công");
    }

    public async Task<ApiResponse<bool>> LogoutAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return ApiResponse<bool>.Fail("Không tìm thấy người dùng");
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Đăng xuất thành công");
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return ApiResponse<UserDto>.Fail("Không tìm thấy người dùng");
        }

        return ApiResponse<UserDto>.Ok(MapToDto(user));
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return ApiResponse<UserDto>.Fail("Tên đăng nhập đã tồn tại");
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = HashPassword(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            Role = (UserRole)request.Role,
            CustomerId = request.CustomerId,
            CreatedAt = DateTime.Now
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return ApiResponse<UserDto>.Ok(MapToDto(user), "Tạo người dùng thành công");
    }

    public async Task<ApiResponse<List<UserDto>>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<UserDto>>.Ok(users.Select(MapToDto).ToList());
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Role = (int)user.Role,
        FullName = user.FullName,
        Email = user.Email,
        CustomerId = user.CustomerId,
        CreatedAt = user.CreatedAt
    };
}
