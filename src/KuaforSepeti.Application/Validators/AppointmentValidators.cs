namespace BakimZamani.Application.Validators;

using FluentValidation;
using BakimZamani.Application.DTOs.Appointment;

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.SalonId)
            .NotEmpty().WithMessage("Salon seÃ§imi gereklidir.");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Personel seÃ§imi gereklidir.");

        RuleFor(x => x.ServiceIds)
            .NotEmpty().WithMessage("En az bir hizmet seÃ§ilmelidir.");

        RuleFor(x => x.AppointmentDate)
            .Must(date => date >= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Randevu tarihi bugÃ¼n veya daha sonra olmalÄ±dÄ±r.");

        RuleFor(x => x.StartTime)
            .Must(time => time >= TimeSpan.FromHours(7) && time <= TimeSpan.FromHours(22))
            .WithMessage("Randevu saati 07:00 ile 22:00 arasÄ±nda olmalÄ±dÄ±r.");
    }
}

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty().WithMessage("Randevu seÃ§imi gereklidir.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Puan 1 ile 5 arasÄ±nda olmalÄ±dÄ±r.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Yorum en fazla 1000 karakter olabilir.");
    }
}

public class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentRequest>
{
    public CancelAppointmentRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Ä°ptal sebebi gereklidir.")
            .MaximumLength(500).WithMessage("Ä°ptal sebebi en fazla 500 karakter olabilir.");
    }
}

