namespace KuaforSepeti.Infrastructure.Services;

using AutoMapper;
using KuaforSepeti.Application.DTOs.Common;
using KuaforSepeti.Application.DTOs.Salon;
using KuaforSepeti.Application.Services.Interfaces;
using KuaforSepeti.Domain.Entities;
using KuaforSepeti.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Salon service implementation.
/// </summary>
public class SalonService : ISalonService
{
    private readonly IRepository<Salon> _salonRepository;
    private readonly IRepository<Staff> _staffRepository;
    private readonly IRepository<Service> _serviceRepository;
    private readonly IRepository<WorkingHours> _workingHoursRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SalonService(
        IRepository<Salon> salonRepository,
        IRepository<Staff> staffRepository,
        IRepository<Service> serviceRepository,
        IRepository<WorkingHours> workingHoursRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _salonRepository = salonRepository;
        _staffRepository = staffRepository;
        _serviceRepository = serviceRepository;
        _workingHoursRepository = workingHoursRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PaginatedResult<SalonListResponse>>> SearchSalonsAsync(SalonSearchRequest request)
    {
        var query = _salonRepository.Query().Where(s => s.IsActive);

        // Search filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(s =>
                s.Name.Contains(request.SearchTerm) ||
                s.Address.Contains(request.SearchTerm) ||
                s.City.Contains(request.SearchTerm) ||
                s.District.Contains(request.SearchTerm));
        }

        if (!string.IsNullOrEmpty(request.City))
        {
            query = query.Where(s => s.City == request.City);
        }

        if (!string.IsNullOrEmpty(request.District))
        {
            query = query.Where(s => s.District == request.District);
        }

        if (request.TargetGender.HasValue)
        {
            query = query.Where(s => s.TargetGender == request.TargetGender.Value);
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "rating" => query.OrderByDescending(s => s.Rating),
            "name" => query.OrderBy(s => s.Name),
            _ => query.OrderByDescending(s => s.Rating)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var mappedItems = _mapper.Map<List<SalonListResponse>>(items);
        var result = new PaginatedResult<SalonListResponse>(mappedItems, totalCount, request.PageNumber, request.PageSize);

        return ApiResponse<PaginatedResult<SalonListResponse>>.Ok(result);
    }

    public async Task<ApiResponse<SalonDetailResponse>> GetSalonByIdAsync(string salonId)
    {
        var salon = await _salonRepository.Query()
            .Include(s => s.StaffMembers)
            .Include(s => s.Services)
            .Include(s => s.WorkingHours)
            .FirstOrDefaultAsync(s => s.Id == salonId);

        if (salon == null)
        {
            return ApiResponse<SalonDetailResponse>.Fail("Salon bulunamadı.");
        }

        var response = _mapper.Map<SalonDetailResponse>(salon);
        return ApiResponse<SalonDetailResponse>.Ok(response);
    }

    public async Task<ApiResponse<SalonDetailResponse>> CreateSalonAsync(string ownerId, CreateSalonRequest request)
    {
        var salon = _mapper.Map<Salon>(request);
        salon.OwnerId = ownerId;
        salon.IsActive = true;

        await _salonRepository.AddAsync(salon);
        await _unitOfWork.SaveChangesAsync();

        // Create default working hours
        await CreateDefaultWorkingHoursAsync(salon.Id);

        return await GetSalonByIdAsync(salon.Id);
    }

    public async Task<ApiResponse<SalonDetailResponse>> UpdateSalonAsync(string salonId, string ownerId, UpdateSalonRequest request)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null)
        {
            return ApiResponse<SalonDetailResponse>.Fail("Salon bulunamadı.");
        }

        if (salon.OwnerId != ownerId)
        {
            return ApiResponse<SalonDetailResponse>.Fail("Bu işlem için yetkiniz yok.");
        }

        _mapper.Map(request, salon);
        await _unitOfWork.SaveChangesAsync();

        return await GetSalonByIdAsync(salonId);
    }

    public async Task<ApiResponse> DeleteSalonAsync(string salonId, string ownerId)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null)
        {
            return ApiResponse.Fail("Salon bulunamadı.");
        }

        if (salon.OwnerId != ownerId)
        {
            return ApiResponse.Fail("Bu işlem için yetkiniz yok.");
        }

        _salonRepository.Delete(salon);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Salon silindi.");
    }

    public async Task<ApiResponse<List<SalonListResponse>>> GetMySalonsAsync(string ownerId)
    {
        var salons = await _salonRepository.Query()
            .Where(s => s.OwnerId == ownerId)
            .ToListAsync();

        var response = _mapper.Map<List<SalonListResponse>>(salons);
        return ApiResponse<List<SalonListResponse>>.Ok(response);
    }

    public async Task<ApiResponse<StaffResponse>> AddStaffAsync(string salonId, string ownerId, CreateStaffRequest request)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null || salon.OwnerId != ownerId)
        {
            return ApiResponse<StaffResponse>.Fail("Salon bulunamadı veya yetkiniz yok.");
        }

        var staff = _mapper.Map<Staff>(request);
        staff.SalonId = salonId;
        staff.IsActive = true;

        await _staffRepository.AddAsync(staff);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<StaffResponse>(staff);
        return ApiResponse<StaffResponse>.Ok(response, "Personel eklendi.");
    }

    public async Task<ApiResponse<StaffResponse>> UpdateStaffAsync(string salonId, string staffId, string ownerId, CreateStaffRequest request)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null || salon.OwnerId != ownerId)
        {
            return ApiResponse<StaffResponse>.Fail("Salon bulunamadı veya yetkiniz yok.");
        }

        var staff = await _staffRepository.GetByIdAsync(staffId);
        if (staff == null || staff.SalonId != salonId)
        {
            return ApiResponse<StaffResponse>.Fail("Personel bulunamadı.");
        }

        _mapper.Map(request, staff);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<StaffResponse>(staff);
        return ApiResponse<StaffResponse>.Ok(response, "Personel güncellendi.");
    }

    public async Task<ApiResponse> RemoveStaffAsync(string salonId, string staffId, string ownerId)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null || salon.OwnerId != ownerId)
        {
            return ApiResponse.Fail("Salon bulunamadı veya yetkiniz yok.");
        }

        var staff = await _staffRepository.GetByIdAsync(staffId);
        if (staff == null || staff.SalonId != salonId)
        {
            return ApiResponse.Fail("Personel bulunamadı.");
        }

        _staffRepository.Delete(staff);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Personel kaldırıldı.");
    }

    public async Task<ApiResponse<ServiceResponse>> AddServiceAsync(string salonId, string ownerId, CreateServiceRequest request)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null || salon.OwnerId != ownerId)
        {
            return ApiResponse<ServiceResponse>.Fail("Salon bulunamadı veya yetkiniz yok.");
        }

        var service = _mapper.Map<Service>(request);
        service.SalonId = salonId;
        service.IsActive = true;

        await _serviceRepository.AddAsync(service);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<ServiceResponse>(service);
        return ApiResponse<ServiceResponse>.Ok(response, "Hizmet eklendi.");
    }

    public async Task<ApiResponse<ServiceResponse>> UpdateServiceAsync(string salonId, string serviceId, string ownerId, CreateServiceRequest request)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null || salon.OwnerId != ownerId)
        {
            return ApiResponse<ServiceResponse>.Fail("Salon bulunamadı veya yetkiniz yok.");
        }

        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service == null || service.SalonId != salonId)
        {
            return ApiResponse<ServiceResponse>.Fail("Hizmet bulunamadı.");
        }

        _mapper.Map(request, service);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<ServiceResponse>(service);
        return ApiResponse<ServiceResponse>.Ok(response, "Hizmet güncellendi.");
    }

    public async Task<ApiResponse> RemoveServiceAsync(string salonId, string serviceId, string ownerId)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null || salon.OwnerId != ownerId)
        {
            return ApiResponse.Fail("Salon bulunamadı veya yetkiniz yok.");
        }

        var service = await _serviceRepository.GetByIdAsync(serviceId);
        if (service == null || service.SalonId != salonId)
        {
            return ApiResponse.Fail("Hizmet bulunamadı.");
        }

        _serviceRepository.Delete(service);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse.Ok("Hizmet kaldırıldı.");
    }

    public async Task<ApiResponse<List<WorkingHoursResponse>>> GetWorkingHoursAsync(string salonId)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null)
        {
            return ApiResponse<List<WorkingHoursResponse>>.Fail("Salon bulunamadı.");
        }

        var workingHours = await _workingHoursRepository.Query()
            .Where(wh => wh.SalonId == salonId)
            .OrderBy(wh => wh.DayOfWeek)
            .ToListAsync();

        var response = _mapper.Map<List<WorkingHoursResponse>>(workingHours);
        return ApiResponse<List<WorkingHoursResponse>>.Ok(response);
    }

    public async Task<ApiResponse<List<WorkingHoursResponse>>> UpdateWorkingHoursAsync(string salonId, string ownerId, UpdateWorkingHoursRequest request)
    {
        var salon = await _salonRepository.GetByIdAsync(salonId);
        if (salon == null || salon.OwnerId != ownerId)
        {
            return ApiResponse<List<WorkingHoursResponse>>.Fail("Salon bulunamadı veya yetkiniz yok.");
        }

        // Delete existing working hours for this salon
        var existingHours = await _workingHoursRepository.Query()
            .Where(wh => wh.SalonId == salonId)
            .ToListAsync();

        foreach (var hour in existingHours)
        {
            _workingHoursRepository.HardDelete(hour);
        }

        // Add new working hours
        foreach (var item in request.WorkingHours)
        {
            var workingHours = new WorkingHours
            {
                SalonId = salonId,
                StaffId = item.StaffId,
                DayOfWeek = item.DayOfWeek,
                OpenTime = item.OpenTime ?? TimeSpan.FromHours(9),
                CloseTime = item.CloseTime ?? TimeSpan.FromHours(18),
                IsClosed = item.IsClosed,
                BreakStartTime = item.BreakStartTime,
                BreakEndTime = item.BreakEndTime
            };
            await _workingHoursRepository.AddAsync(workingHours);
        }

        await _unitOfWork.SaveChangesAsync();

        var newHours = await _workingHoursRepository.Query()
            .Where(wh => wh.SalonId == salonId)
            .ToListAsync();

        var response = _mapper.Map<List<WorkingHoursResponse>>(newHours);
        return ApiResponse<List<WorkingHoursResponse>>.Ok(response, "Çalışma saatleri güncellendi.");
    }

    #region Private Methods

    private async Task CreateDefaultWorkingHoursAsync(string salonId)
    {
        var defaultOpenTime = new TimeSpan(9, 0, 0);
        var defaultCloseTime = new TimeSpan(18, 0, 0);

        for (var day = DayOfWeek.Sunday; day <= DayOfWeek.Saturday; day++)
        {
            var workingHours = new WorkingHours
            {
                SalonId = salonId,
                DayOfWeek = day,
                OpenTime = defaultOpenTime,
                CloseTime = defaultCloseTime,
                IsClosed = day == DayOfWeek.Sunday
            };
            await _workingHoursRepository.AddAsync(workingHours);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    #endregion
}
