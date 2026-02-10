namespace KuaforSepeti.API.Controllers;

using KuaforSepeti.Application.DTOs.Common;
using KuaforSepeti.Application.DTOs.Salon;
using KuaforSepeti.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Salon management controller.
/// </summary>
public class SalonsController : BaseApiController
{
    private readonly ISalonService _salonService;

    public SalonsController(ISalonService salonService)
    {
        _salonService = salonService;
    }

    /// <summary>
    /// Search and filter salons.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<SalonListResponse>>), 200)]
    public async Task<IActionResult> SearchSalons([FromQuery] SalonSearchRequest request)
    {
        var result = await _salonService.SearchSalonsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Get salon details.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SalonDetailResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSalon(string id)
    {
        var result = await _salonService.GetSalonByIdAsync(id);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Get my salons (for salon owners).
    /// </summary>
    [Authorize]
    [HttpGet("my-salons")]
    [ProducesResponseType(typeof(ApiResponse<List<SalonListResponse>>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMySalons()
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _salonService.GetMySalonsAsync(CurrentUserId);
        return Ok(result);
    }

    /// <summary>
    /// Create a new salon.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SalonDetailResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateSalon([FromBody] CreateSalonRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _salonService.CreateSalonAsync(CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetSalon), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Update salon.
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SalonDetailResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> UpdateSalon(string id, [FromBody] UpdateSalonRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _salonService.UpdateSalonAsync(id, CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete salon.
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> DeleteSalon(string id)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _salonService.DeleteSalonAsync(id, CurrentUserId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Get salon working hours.
    /// </summary>
    [HttpGet("{salonId}/working-hours")]
    [ProducesResponseType(typeof(ApiResponse<List<WorkingHoursResponse>>), 200)]
    public async Task<IActionResult> GetWorkingHours(string salonId)
    {
        var result = await _salonService.GetWorkingHoursAsync(salonId);
        return Ok(result);
    }

    /// <summary>
    /// Update salon working hours.
    /// </summary>
    [Authorize]
    [HttpPut("{salonId}/working-hours")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> UpdateWorkingHours(string salonId, [FromBody] UpdateWorkingHoursRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _salonService.UpdateWorkingHoursAsync(salonId, CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #region Staff Management

    /// <summary>
    /// Add staff to salon.
    /// </summary>
    [Authorize]
    [HttpPost("{salonId}/staff")]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> AddStaff(string salonId, [FromBody] CreateStaffRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _salonService.AddStaffAsync(salonId, CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Created("", result);
    }

    /// <summary>
    /// Update staff.
    /// </summary>
    [Authorize]
    [HttpPut("{salonId}/staff/{staffId}")]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateStaff(string salonId, string staffId, [FromBody] CreateStaffRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _salonService.UpdateStaffAsync(salonId, staffId, CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete staff.
    /// </summary>
    [Authorize]
    [HttpDelete("{salonId}/staff/{staffId}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeleteStaff(string salonId, string staffId)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _salonService.RemoveStaffAsync(salonId, staffId, CurrentUserId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion

    #region Service Management

    /// <summary>
    /// Add service to salon.
    /// </summary>
    [Authorize]
    [HttpPost("{salonId}/services")]
    [ProducesResponseType(typeof(ApiResponse<ServiceResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> AddService(string salonId, [FromBody] CreateServiceRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _salonService.AddServiceAsync(salonId, CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Created("", result);
    }

    /// <summary>
    /// Update service.
    /// </summary>
    [Authorize]
    [HttpPut("{salonId}/services/{serviceId}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateService(string salonId, string serviceId, [FromBody] CreateServiceRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _salonService.UpdateServiceAsync(salonId, serviceId, CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Delete service.
    /// </summary>
    [Authorize]
    [HttpDelete("{salonId}/services/{serviceId}")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> DeleteService(string salonId, string serviceId)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _salonService.RemoveServiceAsync(salonId, serviceId, CurrentUserId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion
}
