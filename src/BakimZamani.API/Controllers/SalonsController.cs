namespace BakimZamani.API.Controllers;

using BakimZamani.Application.DTOs.Common;
using BakimZamani.Application.DTOs.Salon;
using BakimZamani.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

/// <summary>
/// Salon management controller.
/// </summary>
public class SalonsController : BaseApiController
{
    private readonly ISalonService _salonService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SalonsController(ISalonService salonService, IWebHostEnvironment webHostEnvironment)
    {
        _salonService = salonService;
        _webHostEnvironment = webHostEnvironment;
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

    /// <summary>
    /// Register a new salon with files (photos, license).
    /// </summary>
    [Authorize]
    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<SalonDetailResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RegisterSalon([FromForm] CreateSalonRegisterRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        // 1. Upload Business License
        string? licenseUrl = null;
        if (request.BusinessLicense != null)
        {
            licenseUrl = await SaveFileAsync(request.BusinessLicense, "licenses");
        }

        // 2. Upload Salon Photos
        var photoUrls = new List<string>();
        if (request.SalonPhotos != null)
        {
            foreach (var photo in request.SalonPhotos)
            {
                var url = await SaveFileAsync(photo, "salon_photos");
                if (url != null) photoUrls.Add(url);
            }
        }

        // 3. Map to CreateSalonRequest
        var createRequest = new CreateSalonRequest
        {
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            City = request.City,
            District = request.District,
            PhoneNumber = request.PhoneNumber,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            TargetGender = request.TargetGender,
            TaxNumber = request.TaxNumber,
            BusinessLicenseUrl = licenseUrl,
            GalleryImages = photoUrls
        };

        var result = await _salonService.CreateSalonAsync(CurrentUserId, createRequest);

        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetSalon), new { id = result.Data?.Id }, result);
    }

    private async Task<string?> SaveFileAsync(IFormFile file, string folderName)
    {
        if (file == null || file.Length == 0)
            return null;

        // Ensure wwwroot exists
        var webRootPath = _webHostEnvironment.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            // If WebRootPath is null (e.g. Worker Service), use ContentRootPath/wwwroot
            webRootPath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
        }

        var uploadsPath = Path.Combine(webRootPath, "uploads", folderName);
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative URL
        return $"/uploads/{folderName}/{fileName}";
    }
}

