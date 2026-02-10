namespace KuaforSepeti.Application.Validators;

using FluentValidation;
using KuaforSepeti.Application.DTOs.Appointment;

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.SalonId)
            .NotEmpty().WithMessage("Salon seçimi gereklidir.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Personel seçimi gereklidir.");

        RuleFor(x => x.ServiceIds)
            .NotEmpty().WithMessage("En az bir hizmet seçilmelidir.");

        RuleFor(x => x.AppointmentDate)
            .Must(date => date >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Randevu tarihi bugün veya daha sonra olmalıdır.");

        RuleFor(x => x.StartTime)
            .Must(time => time >= TimeSpan.FromHours(7) && time <= TimeSpan.FromHours(22))
            .WithMessage("Randevu saati 07:00 ile 22:00 arasında olmalıdır.");
    }
}

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty().WithMessage("Randevu seçimi gereklidir.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Puan 1 ile 5 arasında olmalıdır.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Yorum en fazla 1000 karakter olabilir.");
    }
}

public class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentRequest>
{
    public CancelAppointmentRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("İptal sebebi gereklidir.")
            .MaximumLength(500).WithMessage("İptal sebebi en fazla 500 karakter olabilir.");
    }
}
