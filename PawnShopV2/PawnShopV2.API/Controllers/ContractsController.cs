using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawnShopV2.Application.DTOs.Contract;
using PawnShopV2.Application.Services.Interfaces;

namespace PawnShopV2.API.Controllers;

[ApiController]
[Route("api/v1/contracts")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [Authorize(Roles = "Owner")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] int? customerId = null, [FromQuery] int? status = null,
        [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var result = await _contractService.GetAllAsync(page, pageSize, customerId, status, fromDate, toDate);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _contractService.GetByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _contractService.GetByCodeAsync(code);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("by-customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var result = await _contractService.GetByCustomerIdAsync(customerId);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("due-soon")]
    public async Task<IActionResult> GetDueSoon([FromQuery] int days = 7)
    {
        var result = await _contractService.GetDueSoonAsync(days);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdue()
    {
        var result = await _contractService.GetOverdueAsync();
        return Ok(result);
    }

    [HttpGet("public-lookup")]
    public async Task<IActionResult> PublicLookup([FromQuery] string citizenId, [FromQuery] string phoneNumber)
    {
        var result = await _contractService.PublicLookupAsync(new PublicLookupRequest
        {
            CitizenId = citizenId,
            PhoneNumber = phoneNumber
        });
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}/soft-copy")]
    public async Task<IActionResult> GetSoftCopy(int id)
    {
        var result = await _contractService.GetSoftCopyAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("my-contracts")]
    public async Task<IActionResult> GetMyContracts()
    {
        var customerIdClaim = User.FindFirst("CustomerId");
        if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out var customerId))
            return BadRequest(new { Success = false, Message = "Không tìm thấy thông tin khách hàng" });

        var result = await _contractService.GetMyContractsAsync(customerId);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContractRequest request)
    {
        var result = await _contractService.CreateAsync(request);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContractRequest request)
    {
        var result = await _contractService.UpdateAsync(id, request);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _contractService.DeleteAsync(id);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{id}/calculate-interest")]
    public async Task<IActionResult> CalculateInterest(int id)
    {
        var result = await _contractService.CalculateInterestAsync(id);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("{id}/extend")]
    public async Task<IActionResult> Extend(int id, [FromBody] ExtendContractRequest request)
    {
        var result = await _contractService.ExtendAsync(id, request);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("{id}/redeem")]
    public async Task<IActionResult> Redeem(int id, [FromBody] RedeemContractRequest request)
    {
        var result = await _contractService.RedeemAsync(id, request);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("{id}/forfeit")]
    public async Task<IActionResult> Forfeit(int id, [FromBody] ForfeitContractRequest request)
    {
        var result = await _contractService.ForfeitAsync(id, request);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
