namespace BakimZamani.Application.Validators;

using FluentValidation;
using BakimZamani.Application.DTOs.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi gereklidir.")
            .EmailAddress().WithMessage("GeÃ§erli bir e-posta adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Åžifre gereklidir.")
            .MinimumLength(6).WithMessage("Åžifre en az 6 karakter olmalÄ±dÄ±r.")
            .Matches("[A-Z]").WithMessage("Åžifre en az bir bÃ¼yÃ¼k harf iÃ§ermelidir.")
            .Matches("[a-z]").WithMessage("Åžifre en az bir kÃ¼Ã§Ã¼k harf iÃ§ermelidir.")
            .Matches("[0-9]").WithMessage("Åžifre en az bir rakam iÃ§ermelidir.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad gereklidir.")
            .MaximumLength(100).WithMessage("Ad soyad en fazla 100 karakter olabilir.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarasÄ± gereklidir.")
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("GeÃ§erli bir telefon numarasÄ± giriniz.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi gereklidir.")
            .EmailAddress().WithMessage("GeÃ§erli bir e-posta adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Åžifre gereklidir.");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut ÅŸifre gereklidir.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni ÅŸifre gereklidir.")
            .MinimumLength(6).WithMessage("Åžifre en az 6 karakter olmalÄ±dÄ±r.")
            .Matches("[A-Z]").WithMessage("Åžifre en az bir bÃ¼yÃ¼k harf iÃ§ermelidir.")
            .Matches("[a-z]").WithMessage("Åžifre en az bir kÃ¼Ã§Ã¼k harf iÃ§ermelidir.")
            .Matches("[0-9]").WithMessage("Åžifre en az bir rakam iÃ§ermelidir.");
    }
}

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad gereklidir.")
            .MaximumLength(100).WithMessage("Ad soyad en fazla 100 karakter olabilir.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarasÄ± gereklidir.")
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("GeÃ§erli bir telefon numarasÄ± giriniz.");
    }
}

