namespace BakimZamani.API.Controllers;

using BakimZamani.Application.DTOs.Appointment;
using BakimZamani.Application.DTOs.Common;
using BakimZamani.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Appointment management controller.
/// </summary>
public class AppointmentsController : BaseApiController
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    /// <summary>
    /// Get available time slots for a salon.
    /// </summary>
    [HttpGet("available-slots")]
    [ProducesResponseType(typeof(ApiResponse<List<AvailableSlotsResponse>>), 200)]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] GetAvailableSlotsRequest request)
    {
        var result = await _appointmentService.GetAvailableSlotsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Create a new appointment (customer).
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _appointmentService.CreateAppointmentAsync(CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetAppointment), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Get my appointments (customer).
    /// </summary>
    [Authorize]
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<AppointmentResponse>>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyAppointments([FromQuery] AppointmentFilterRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _appointmentService.GetMyAppointmentsAsync(CurrentUserId, request);
        return Ok(result);
    }

    /// <summary>
    /// Get salon appointments (salon owner/staff).
    /// </summary>
    [Authorize]
    [HttpGet("salon/{salonId}")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<AppointmentResponse>>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetSalonAppointments(string salonId, [FromQuery] AppointmentFilterRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _appointmentService.GetSalonAppointmentsAsync(salonId, CurrentUserId, request);
        return Ok(result);
    }

    /// <summary>
    /// Get appointment details.
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAppointment(string id)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _appointmentService.GetAppointmentByIdAsync(id, CurrentUserId);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Confirm appointment (salon owner/staff).
    /// </summary>
    [Authorize]
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ConfirmAppointment(string id, [FromBody] ConfirmAppointmentRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _appointmentService.ConfirmAppointmentAsync(id, CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Cancel appointment by customer.
    /// </summary>
    [Authorize]
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CancelAppointment(string id, [FromBody] CancelAppointmentRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _appointmentService.CancelAppointmentByCustomerAsync(id, CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Cancel appointment by salon.
    /// </summary>
    [Authorize]
    [HttpPost("{id}/cancel-by-salon")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CancelAppointmentBySalon(string id, [FromBody] CancelAppointmentRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _appointmentService.CancelAppointmentBySalonAsync(id, CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Complete appointment (salon owner/staff).
    /// </summary>
    [Authorize]
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CompleteAppointment(string id)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _appointmentService.CompleteAppointmentAsync(id, CurrentUserId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Mark appointment as no-show (salon owner/staff).
    /// </summary>
    [Authorize]
    [HttpPost("{id}/no-show")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> MarkAsNoShow(string id)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _appointmentService.MarkAsNoShowAsync(id, CurrentUserId);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Reschedule appointment.
    /// </summary>
    [Authorize]
    [HttpPost("{id}/reschedule")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RescheduleAppointment(string id, [FromBody] RescheduleAppointmentRequest request)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return Unauthorized();

        var result = await _appointmentService.RescheduleAppointmentAsync(id, CurrentUserId, request);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

