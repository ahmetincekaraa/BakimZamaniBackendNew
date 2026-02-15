namespace BakimZamani.Application.Validators;

using FluentValidation;
using BakimZamani.Application.DTOs.Salon;

public class CreateSalonRequestValidator : AbstractValidator<CreateSalonRequest>
{
    public CreateSalonRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Salon adÄ± gereklidir.")
            .MaximumLength(100).WithMessage("Salon adÄ± en fazla 100 karakter olabilir.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres gereklidir.")
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Åžehir gereklidir.")
            .MaximumLength(50).WithMessage("Åžehir en fazla 50 karakter olabilir.");

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("Ä°lÃ§e gereklidir.")
            .MaximumLength(50).WithMessage("Ä°lÃ§e en fazla 50 karakter olabilir.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarasÄ± gereklidir.")
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("GeÃ§erli bir telefon numarasÄ± giriniz.");
    }
}

public class CreateStaffRequestValidator : AbstractValidator<CreateStaffRequest>
{
    public CreateStaffRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Personel adÄ± gereklidir.")
            .MaximumLength(100).WithMessage("Personel adÄ± en fazla 100 karakter olabilir.");
    }
}

public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
{
    public CreateServiceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hizmet adÄ± gereklidir.")
            .MaximumLength(100).WithMessage("Hizmet adÄ± en fazla 100 karakter olabilir.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan bÃ¼yÃ¼k olmalÄ±dÄ±r.");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(5, 480).WithMessage("SÃ¼re 5 ile 480 dakika arasÄ±nda olmalÄ±dÄ±r.");

        RuleFor(x => x.DiscountedPrice)
            .LessThan(x => x.Price).When(x => x.DiscountedPrice.HasValue)
            .WithMessage("Ä°ndirimli fiyat normal fiyattan dÃ¼ÅŸÃ¼k olmalÄ±dÄ±r.");
    }
}

