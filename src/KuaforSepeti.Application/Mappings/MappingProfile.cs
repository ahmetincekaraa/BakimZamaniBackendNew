namespace BakimZamani.Application.Mappings;

using AutoMapper;
using BakimZamani.Application.DTOs.Auth;
using BakimZamani.Application.DTOs.Salon;
using BakimZamani.Application.DTOs.Appointment;
using BakimZamani.Domain.Entities;
using System.Text.Json;

/// <summary>
/// AutoMapper profile for entity-DTO mappings.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserResponse>();
        CreateMap<RegisterRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        CreateMap<UpdateProfileRequest, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Salon mappings
        CreateMap<Salon, SalonListResponse>();
        CreateMap<Salon, SalonDetailResponse>()
            .AfterMap((src, dest) =>
            {
                if (!string.IsNullOrEmpty(src.GalleryImagesJson))
                {
                    dest.GalleryImages = JsonSerializer.Deserialize<List<string>>(src.GalleryImagesJson) ?? new List<string>();
                }
                else
                {
                    dest.GalleryImages = new List<string>();
                }
            });
        CreateMap<CreateSalonRequest, Salon>()
            .AfterMap((src, dest) =>
            {
                if (src.GalleryImages != null)
                {
                    dest.GalleryImagesJson = JsonSerializer.Serialize(src.GalleryImages);
                }
            });
        CreateMap<UpdateSalonRequest, Salon>()
            .AfterMap((src, dest) =>
            {
                if (src.GalleryImages != null)
                {
                    dest.GalleryImagesJson = JsonSerializer.Serialize(src.GalleryImages);
                }
            })
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Staff mappings
        CreateMap<Staff, StaffResponse>()
            .ForMember(dest => dest.ServiceIds, opt => opt.MapFrom(src => 
                src.StaffServices.Select(ss => ss.ServiceId).ToList()));
        CreateMap<CreateStaffRequest, Staff>();

        // Service mappings
        CreateMap<Service, ServiceResponse>();
        CreateMap<CreateServiceRequest, Service>();

        // WorkingHours mappings
        CreateMap<WorkingHours, WorkingHoursResponse>();
        CreateMap<WorkingHoursItem, WorkingHours>();

        // Appointment mappings
        CreateMap<Appointment, AppointmentResponse>()
            .ForMember(dest => dest.SalonName, opt => opt.MapFrom(src => src.Salon.Name))
            .ForMember(dest => dest.SalonAddress, opt => opt.MapFrom(src => src.Salon.Address))
            .ForMember(dest => dest.SalonLogoUrl, opt => opt.MapFrom(src => src.Salon.LogoUrl))
            .ForMember(dest => dest.StaffName, opt => opt.MapFrom(src => src.Staff.FullName))
            .ForMember(dest => dest.StaffProfileImageUrl, opt => opt.MapFrom(src => src.Staff.ProfileImageUrl))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName))
            .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer.PhoneNumber));
        CreateMap<CreateAppointmentRequest, Appointment>();

        // AppointmentService mappings
        CreateMap<AppointmentService, AppointmentServiceResponse>();

        // Review mappings
        CreateMap<Review, ReviewResponse>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FullName))
            .ForMember(dest => dest.CustomerProfileImageUrl, opt => opt.MapFrom(src => src.Customer.ProfileImageUrl));
        CreateMap<CreateReviewRequest, Review>();
    }
}


