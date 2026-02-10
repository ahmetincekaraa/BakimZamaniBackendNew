namespace KuaforSepeti.Application.Validators;

using FluentValidation;
using KuaforSepeti.Application.DTOs.Salon;

public class CreateSalonRequestValidator : AbstractValidator<CreateSalonRequest>
{
    public CreateSalonRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Salon adı gereklidir.")
            .MaximumLength(100).WithMessage("Salon adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres gereklidir.")
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Şehir gereklidir.")
            .MaximumLength(50).WithMessage("Şehir en fazla 50 karakter olabilir.");

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("İlçe gereklidir.")
            .MaximumLength(50).WithMessage("İlçe en fazla 50 karakter olabilir.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası gereklidir.")
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz.");
    }
}

public class CreateStaffRequestValidator : AbstractValidator<CreateStaffRequest>
{
    public CreateStaffRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Personel adı gereklidir.")
            .MaximumLength(100).WithMessage("Personel adı en fazla 100 karakter olabilir.");
    }
}

public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
{
    public CreateServiceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hizmet adı gereklidir.")
            .MaximumLength(100).WithMessage("Hizmet adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır.");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(5, 480).WithMessage("Süre 5 ile 480 dakika arasında olmalıdır.");

        RuleFor(x => x.DiscountedPrice)
            .LessThan(x => x.Price).When(x => x.DiscountedPrice.HasValue)
            .WithMessage("İndirimli fiyat normal fiyattan düşük olmalıdır.");
    }
}
