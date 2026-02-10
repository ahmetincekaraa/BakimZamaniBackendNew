namespace KuaforSepeti.Infrastructure.Services;

using AutoMapper;
using KuaforSepeti.Application.DTOs.Appointment;
using KuaforSepeti.Application.DTOs.Common;
using KuaforSepeti.Application.Services.Interfaces;
using KuaforSepeti.Domain.Entities;
using KuaforSepeti.Domain.Enums;
using KuaforSepeti.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Appointment service implementation.
/// </summary>
public class AppointmentBookingService : IAppointmentService
{
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly IRepository<Domain.Entities.AppointmentService> _appointmentServiceRepository;
    private readonly IRepository<Salon> _salonRepository;
    private readonly IRepository<Staff> _staffRepository;
    private readonly IRepository<Service> _serviceRepository;
    private readonly IRepository<WorkingHours> _workingHoursRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public AppointmentBookingService(
        IRepository<Appointment> appointmentRepository,
        IRepository<Domain.Entities.AppointmentService> appointmentServiceRepository,
        IRepository<Salon> salonRepository,
        IRepository<Staff> staffRepository,
        IRepository<Service> serviceRepository,
        IRepository<WorkingHours> workingHoursRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationService notificationService)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentServiceRepository = appointmentServiceRepository;
        _salonRepository = salonRepository;
        _staffRepository = staffRepository;
        _serviceRepository = serviceRepository;
        _workingHoursRepository = workingHoursRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<List<AvailableSlotsResponse>>> GetAvailableSlotsAsync(GetAvailableSlotsRequest request)
    {
        var salon = await _salonRepository.GetByIdAsync(request.SalonId);
        if (salon == null)
        {
            return ApiResponse<List<AvailableSlotsResponse>>.Fail("Salon bulunamadı.");
        }

        // Calculate total duration
        var services = await _serviceRepository.Query()
            .Where(s => request.ServiceIds.Contains(s.Id))
            .ToListAsync();
        var totalDuration = services.Sum(s => s.DurationMinutes);

        var result = new List<AvailableSlotsResponse>();

        // Get working hours for the requested date
        var workingHours = await _workingHoursRepository.Query()
            .FirstOrDefaultAsync(wh => wh.SalonId == request.SalonId && wh.DayOfWeek == request.Date.DayOfWeek);

        if (workingHours == null || workingHours.IsClosed)
        {
            return ApiResponse<List<AvailableSlotsResponse>>.Ok(result, "Bu gün kapalı.");
        }

        // Get staff members
        var staffQuery = _staffRepository.Query().Where(s => s.SalonId == request.SalonId && s.IsActive);
        if (!string.IsNullOrEmpty(request.StaffId))
        {
            staffQuery = staffQuery.Where(s => s.Id == request.StaffId);
        }
        var staffList = await staffQuery.ToListAsync();

        foreach (var staff in staffList)
        {
            var slots = new List<TimeSlotResponse>();
            var currentTime = workingHours.OpenTime;
            var closeTime = workingHours.CloseTime;
            var slotDuration = 30; // 30 minute slots

            while (currentTime.Add(TimeSpan.FromMinutes(totalDuration)) <= closeTime)
            {
                // Skip break time
                if (workingHours.BreakStartTime.HasValue && workingHours.BreakEndTime.HasValue)
                {
                    if (currentTime >= workingHours.BreakStartTime && currentTime < workingHours.BreakEndTime)
                    {
                        currentTime = workingHours.BreakEndTime.Value;
                        continue;
                    }
                }

                // Check if slot is in the past
                var slotDateTime = request.Date.ToDateTime(TimeOnly.FromTimeSpan(currentTime));
                if (slotDateTime < DateTime.UtcNow.AddHours(1))
                {
                    currentTime = currentTime.Add(TimeSpan.FromMinutes(slotDuration));
                    continue;
                }

                // Check availability
                var isAvailable = await IsSlotAvailableAsync(
                    request.SalonId,
                    staff.Id,
                    request.Date,
                    currentTime,
                    totalDuration);

                slots.Add(new TimeSlotResponse
                {
                    StartTime = currentTime,
                    EndTime = currentTime.Add(TimeSpan.FromMinutes(totalDuration)),
                    IsAvailable = isAvailable
                });

                currentTime = currentTime.Add(TimeSpan.FromMinutes(slotDuration));
            }

            if (slots.Any())
            {
                result.Add(new AvailableSlotsResponse
                {
                    Date = request.Date,
                    StaffId = staff.Id,
                    StaffName = staff.FullName,
                    Slots = slots
                });
            }
        }

        return ApiResponse<List<AvailableSlotsResponse>>.Ok(result);
    }

    public async Task<ApiResponse<AppointmentResponse>> CreateAppointmentAsync(string customerId, CreateAppointmentRequest request)
    {
        var salon = await _salonRepository.GetByIdAsync(request.SalonId);
        if (salon == null)
        {
            return ApiResponse<AppointmentResponse>.Fail("Salon bulunamadı.");
        }

        var staff = await _staffRepository.GetByIdAsync(request.StaffId);
        if (staff == null || staff.SalonId != request.SalonId)
        {
            return ApiResponse<AppointmentResponse>.Fail("Personel bulunamadı.");
        }

        // Get services and calculate totals
        var services = await _serviceRepository.Query()
            .Where(s => request.ServiceIds.Contains(s.Id))
            .ToListAsync();

        if (!services.Any())
        {
            return ApiResponse<AppointmentResponse>.Fail("En az bir hizmet seçmelisiniz.");
        }

        var totalDuration = services.Sum(s => s.DurationMinutes);
        var totalPrice = services.Sum(s => s.DiscountedPrice ?? s.Price);
        var endTime = request.StartTime.Add(TimeSpan.FromMinutes(totalDuration));

        // Check availability
        var isAvailable = await IsSlotAvailableAsync(request.SalonId, request.StaffId, request.AppointmentDate, request.StartTime, totalDuration);
        if (!isAvailable)
        {
            return ApiResponse<AppointmentResponse>.Fail("Seçilen saat dolu.");
        }

        // Create appointment
        var appointment = new Appointment
        {
            SalonId = request.SalonId,
            StaffId = request.StaffId,
            CustomerId = customerId,
            AppointmentDate = request.AppointmentDate,
            StartTime = request.StartTime,
            EndTime = endTime,
            TotalDurationMinutes = totalDuration,
            TotalPrice = totalPrice,
            Status = AppointmentStatus.Pending,
            CustomerNote = request.CustomerNote
        };

        await _appointmentRepository.AddAsync(appointment);

        // Add appointment services
        foreach (var service in services)
        {
            var appointmentService = new Domain.Entities.AppointmentService
            {
                AppointmentId = appointment.Id,
                ServiceId = service.Id,
                ServiceName = service.Name,
                Price = service.DiscountedPrice ?? service.Price,
                DurationMinutes = service.DurationMinutes
            };
            await _appointmentServiceRepository.AddAsync(appointmentService);
        }

        await _unitOfWork.SaveChangesAsync();

        // Send notification to salon
        await _notificationService.SendNotificationAsync(
            salon.OwnerId,
            "Yeni Randevu",
            $"Yeni bir randevu talebi var: {request.AppointmentDate:dd.MM.yyyy} {request.StartTime:hh\\:mm}",
            NotificationType.NewAppointment,
            "Appointment",
            appointment.Id);

        return await GetAppointmentByIdAsync(appointment.Id, customerId);
    }

    public async Task<ApiResponse<PaginatedResult<AppointmentResponse>>> GetMyAppointmentsAsync(string customerId, AppointmentFilterRequest request)
    {
        var query = _appointmentRepository.Query()
            .Include(a => a.Salon)
            .Include(a => a.Staff)
            .Include(a => a.AppointmentServices)
            .Where(a => a.CustomerId == customerId);

        // Apply filters
        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.AppointmentDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.AppointmentDate <= request.ToDate.Value);
        }

        query = query.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var mappedItems = _mapper.Map<List<AppointmentResponse>>(items);
        var result = new PaginatedResult<AppointmentResponse>(mappedItems, totalCount, request.PageNumber, request.PageSize);

        return ApiResponse<PaginatedResult<AppointmentResponse>>.Ok(result);
    }

    public async Task<ApiResponse<AppointmentResponse>> GetAppointmentByIdAsync(string appointmentId, string userId)
    {
        var appointment = await _appointmentRepository.Query()
            .Include(a => a.Salon)
            .Include(a => a.Staff)
            .Include(a => a.Customer)
            .Include(a => a.AppointmentServices)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return ApiResponse<AppointmentResponse>.Fail("Randevu bulunamadı.");
        }

        // Check authorization
        if (appointment.CustomerId != userId && appointment.Salon.OwnerId != userId)
        {
            return ApiResponse<AppointmentResponse>.Fail("Bu randevuyu görüntüleme yetkiniz yok.");
        }

        var response = _mapper.Map<AppointmentResponse>(appointment);
        return ApiResponse<AppointmentResponse>.Ok(response);
    }

    public async Task<ApiResponse<PaginatedResult<AppointmentResponse>>> GetSalonAppointmentsAsync(string salonId, string userId, AppointmentFilterRequest request)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null || salon.OwnerId != userId)
        {
            return ApiResponse<PaginatedResult<AppointmentResponse>>.Fail("Salon bulunamadı veya yetkiniz yok.");
        }

        var query = _appointmentRepository.Query()
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.AppointmentServices)
            .Where(a => a.SalonId == salonId);

        // Apply filters
        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.AppointmentDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.AppointmentDate <= request.ToDate.Value);
        }

        query = query.OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var mappedItems = _mapper.Map<List<AppointmentResponse>>(items);
        var result = new PaginatedResult<AppointmentResponse>(mappedItems, totalCount, request.PageNumber, request.PageSize);

        return ApiResponse<PaginatedResult<AppointmentResponse>>.Ok(result);
    }

    public async Task<ApiResponse> ConfirmAppointmentAsync(string appointmentId, string userId, ConfirmAppointmentRequest request)
    {
        var appointment = await _appointmentRepository.Query()
            .Include(a => a.Salon)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return ApiResponse.Fail("Randevu bulunamadı.");
        }

        if (appointment.Salon.OwnerId != userId)
        {
            return ApiResponse.Fail("Bu işlem için yetkiniz yok.");
        }

        if (appointment.Status != AppointmentStatus.Pending)
        {
            return ApiResponse.Fail("Sadece bekleyen randevular onaylanabilir.");
        }

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.ConfirmedAt = DateTime.UtcNow;
        appointment.SalonNote = request.SalonNote;
        await _unitOfWork.SaveChangesAsync();

        // Send notification to customer
        await _notificationService.SendNotificationAsync(
            appointment.CustomerId,
            "Randevu Onaylandı",
            $"Randevunuz onaylandı: {appointment.AppointmentDate:dd.MM.yyyy} {appointment.StartTime:hh\\:mm}",
            NotificationType.AppointmentConfirmed,
            "Appointment",
            appointment.Id);

        return ApiResponse.Ok("Randevu onaylandı.");
    }

    public async Task<ApiResponse> CancelAppointmentByCustomerAsync(string appointmentId, string customerId, CancelAppointmentRequest request)
    {
        var appointment = await _appointmentRepository.Query()
            .Include(a => a.Salon)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return ApiResponse.Fail("Randevu bulunamadı.");
        }

        if (appointment.CustomerId != customerId)
        {
            return ApiResponse.Fail("Bu işlem için yetkiniz yok.");
        }

        if (appointment.Status == AppointmentStatus.Completed || 
            appointment.Status == AppointmentStatus.CancelledByCustomer ||
            appointment.Status == AppointmentStatus.CancelledBySalon)
        {
            return ApiResponse.Fail("Bu randevu iptal edilemez.");
        }

        appointment.Status = AppointmentStatus.CancelledByCustomer;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancellationReason = request.Reason;
        await _unitOfWork.SaveChangesAsync();

        // Send notification to salon
        await _notificationService.SendNotificationAsync(
            appointment.Salon.OwnerId,
            "Randevu İptal Edildi",
            $"Müşteri randevuyu iptal etti: {appointment.AppointmentDate:dd.MM.yyyy} {appointment.StartTime:hh\\:mm}",
            NotificationType.AppointmentCancelled,
            "Appointment",
            appointment.Id);

        return ApiResponse.Ok("Randevu iptal edildi.");
    }

    public async Task<ApiResponse> CancelAppointmentBySalonAsync(string appointmentId, string userId, CancelAppointmentRequest request)
    {
        var appointment = await _appointmentRepository.Query()
            .Include(a => a.Salon)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return ApiResponse.Fail("Randevu bulunamadı.");
        }

        if (appointment.Salon.OwnerId != userId)
        {
            return ApiResponse.Fail("Bu işlem için yetkiniz yok.");
        }

        appointment.Status = AppointmentStatus.CancelledBySalon;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancellationReason = request.Reason;
        await _unitOfWork.SaveChangesAsync();

        // Send notification to customer
        await _notificationService.SendNotificationAsync(
            appointment.CustomerId,
            "Randevu İptal Edildi",
            $"Salon randevunuzu iptal etti: {appointment.AppointmentDate:dd.MM.yyyy} {appointment.StartTime:hh\\:mm}",
            NotificationType.AppointmentCancelled,
            "Appointment",
            appointment.Id);

        return ApiResponse.Ok("Randevu iptal edildi.");
    }

    public async Task<ApiResponse> CompleteAppointmentAsync(string appointmentId, string userId)
    {
        var appointment = await _appointmentRepository.Query()
            .Include(a => a.Salon)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return ApiResponse.Fail("Randevu bulunamadı.");
        }

        if (appointment.Salon.OwnerId != userId)
        {
            return ApiResponse.Fail("Bu işlem için yetkiniz yok.");
        }

        if (appointment.Status != AppointmentStatus.Confirmed)
        {
            return ApiResponse.Fail("Sadece onaylanmış randevular tamamlanabilir.");
        }

        appointment.Status = AppointmentStatus.Completed;
        appointment.CompletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Randevu tamamlandı.");
    }

    public async Task<ApiResponse> MarkAsNoShowAsync(string appointmentId, string userId)
    {
        var appointment = await _appointmentRepository.Query()
            .Include(a => a.Salon)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return ApiResponse.Fail("Randevu bulunamadı.");
        }

        if (appointment.Salon.OwnerId != userId)
        {
            return ApiResponse.Fail("Bu işlem için yetkiniz yok.");
        }

        appointment.Status = AppointmentStatus.NoShow;
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Randevu 'gelmedi' olarak işaretlendi.");
    }

    public async Task<ApiResponse<AppointmentResponse>> RescheduleAppointmentAsync(string appointmentId, string userId, RescheduleAppointmentRequest request)
    {
        var appointment = await _appointmentRepository.Query()
            .Include(a => a.Salon)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return ApiResponse<AppointmentResponse>.Fail("Randevu bulunamadı.");
        }

        // Check authorization
        if (appointment.CustomerId != userId && appointment.Salon.OwnerId != userId)
        {
            return ApiResponse<AppointmentResponse>.Fail("Bu işlem için yetkiniz yok.");
        }

        var staffId = request.StaffId ?? appointment.StaffId;

        // Check if new slot is available
        var isAvailable = await IsSlotAvailableAsync(
            appointment.SalonId,
            staffId,
            request.NewDate,
            request.NewStartTime,
            appointment.TotalDurationMinutes,
            appointmentId);

        if (!isAvailable)
        {
            return ApiResponse<AppointmentResponse>.Fail("Seçilen saat dolu.");
        }

        appointment.AppointmentDate = request.NewDate;
        appointment.StartTime = request.NewStartTime;
        appointment.EndTime = request.NewStartTime.Add(TimeSpan.FromMinutes(appointment.TotalDurationMinutes));
        if (!string.IsNullOrEmpty(request.StaffId))
        {
            appointment.StaffId = request.StaffId;
        }
        await _unitOfWork.SaveChangesAsync();

        return await GetAppointmentByIdAsync(appointmentId, userId);
    }

    #region Private Methods

    private async Task<bool> IsSlotAvailableAsync(string salonId, string staffId, DateOnly date, TimeSpan startTime, int durationMinutes, string? excludeAppointmentId = null)
    {
        var endTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));

        var query = _appointmentRepository.Query()
            .Where(a => a.SalonId == salonId
                && a.StaffId == staffId
                && a.AppointmentDate == date
                && a.Status != AppointmentStatus.CancelledByCustomer
                && a.Status != AppointmentStatus.CancelledBySalon
                && a.Status != AppointmentStatus.NoShow
                && ((a.StartTime < endTime && a.EndTime > startTime)));

        if (!string.IsNullOrEmpty(excludeAppointmentId))
        {
            query = query.Where(a => a.Id != excludeAppointmentId);
        }

        return !await query.AnyAsync();
    }

    #endregion
}
