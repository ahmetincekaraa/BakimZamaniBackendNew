namespace KuaforSepeti.Infrastructure.Data.Configurations;

using KuaforSepeti.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(26);

        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(e => e.Email).IsUnique();

        builder.Property(e => e.PasswordHash).IsRequired();
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.Property(e => e.ProfileImageUrl).HasMaxLength(500);
        builder.Property(e => e.FcmToken).HasMaxLength(500);
        builder.Property(e => e.RefreshToken).HasMaxLength(500);
    }
}

public class SalonConfiguration : IEntityTypeConfiguration<Salon>
{
    public void Configure(EntityTypeBuilder<Salon> builder)
    {
        builder.ToTable("salons");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(26);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Address).IsRequired().HasMaxLength(500);
        builder.Property(e => e.City).IsRequired().HasMaxLength(50);
        builder.Property(e => e.District).IsRequired().HasMaxLength(50);
        builder.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(e => e.OwnerId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.Rating).HasPrecision(3, 2);
        builder.Property(e => e.LogoUrl).HasMaxLength(500);
        builder.Property(e => e.CoverImageUrl).HasMaxLength(500);

        builder.HasIndex(e => e.City);
        builder.HasIndex(e => e.District);
        builder.HasIndex(e => new { e.Latitude, e.Longitude });

        builder.HasOne(e => e.Owner)
            .WithMany(u => u.OwnedSalons)
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ApprovedByAdmin)
            .WithMany()
            .HasForeignKey(e => e.ApprovedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SuspendedByAdmin)
            .WithMany()
            .HasForeignKey(e => e.SuspendedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("staff");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(26);

        builder.Property(e => e.SalonId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.UserId).HasMaxLength(26);
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Title).HasMaxLength(100);
        builder.Property(e => e.Bio).HasMaxLength(500);
        builder.Property(e => e.ProfileImageUrl).HasMaxLength(500);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);

        builder.HasOne(e => e.Salon)
            .WithMany(s => s.StaffMembers)
            .HasForeignKey(e => e.SalonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(26);

        builder.Property(e => e.SalonId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Price).HasPrecision(10, 2);
        builder.Property(e => e.DiscountedPrice).HasPrecision(10, 2);
        builder.Property(e => e.ImageUrl).HasMaxLength(500);
        builder.Property(e => e.SearchKeywords).HasMaxLength(500);
        
        // Ignore Embedding - managed via raw SQL for pgvector compatibility
        builder.Ignore(e => e.Embedding);

        builder.HasOne(e => e.Salon)
            .WithMany(s => s.Services)
            .HasForeignKey(e => e.SalonId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(e => e.Name);
    }
}

public class StaffServiceConfiguration : IEntityTypeConfiguration<StaffService>
{
    public void Configure(EntityTypeBuilder<StaffService> builder)
    {
        builder.ToTable("staff_services");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(26);

        builder.Property(e => e.StaffId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.ServiceId).IsRequired().HasMaxLength(26);

        builder.HasIndex(e => new { e.StaffId, e.ServiceId }).IsUnique();

        builder.HasOne(e => e.Staff)
            .WithMany(s => s.StaffServices)
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Service)
            .WithMany(s => s.StaffServices)
            .HasForeignKey(e => e.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkingHoursConfiguration : IEntityTypeConfiguration<WorkingHours>
{
    public void Configure(EntityTypeBuilder<WorkingHours> builder)
    {
        builder.ToTable("working_hours");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(26);

        builder.Property(e => e.SalonId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.StaffId).HasMaxLength(26);

        builder.HasOne(e => e.Salon)
            .WithMany(s => s.WorkingHours)
            .HasForeignKey(e => e.SalonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Staff)
            .WithMany(s => s.WorkingHours)
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(26);

        builder.Property(e => e.SalonId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.StaffId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.CustomerId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.TotalPrice).HasPrecision(10, 2);
        builder.Property(e => e.CustomerNote).HasMaxLength(500);
        builder.Property(e => e.SalonNote).HasMaxLength(500);
        builder.Property(e => e.CancellationReason).HasMaxLength(500);

        builder.HasIndex(e => e.AppointmentDate);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.SalonId, e.AppointmentDate });
        builder.HasIndex(e => new { e.StaffId, e.AppointmentDate });
        builder.HasIndex(e => new { e.CustomerId, e.AppointmentDate });

        builder.HasOne(e => e.Salon)
            .WithMany(s => s.Appointments)
            .HasForeignKey(e => e.SalonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Staff)
            .WithMany(s => s.Appointments)
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Customer)
            .WithMany(u => u.Appointments)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AppointmentServiceConfiguration : IEntityTypeConfiguration<AppointmentService>
{
    public void Configure(EntityTypeBuilder<AppointmentService> builder)
    {
        builder.ToTable("appointment_services");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(26);

        builder.Property(e => e.AppointmentId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.ServiceId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.Price).HasPrecision(10, 2);
        builder.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);

        builder.HasOne(e => e.Appointment)
            .WithMany(a => a.AppointmentServices)
            .HasForeignKey(e => e.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Service)
            .WithMany(s => s.AppointmentServices)
            .HasForeignKey(e => e.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(26);

        builder.Property(e => e.SalonId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.CustomerId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.AppointmentId).HasMaxLength(26);
        builder.Property(e => e.Comment).HasMaxLength(1000);
        builder.Property(e => e.Reply).HasMaxLength(500);

        builder.HasIndex(e => new { e.SalonId, e.CreatedAt });

        builder.HasOne(e => e.Salon)
            .WithMany(s => s.Reviews)
            .HasForeignKey(e => e.SalonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Customer)
            .WithMany(u => u.Reviews)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(26);

        builder.Property(e => e.UserId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Message).IsRequired().HasMaxLength(500);
        builder.Property(e => e.RelatedEntityType).HasMaxLength(50);
        builder.Property(e => e.RelatedEntityId).HasMaxLength(26);

        builder.HasIndex(e => new { e.UserId, e.CreatedAt });
        builder.HasIndex(e => new { e.UserId, e.IsRead });

        builder.HasOne(e => e.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ServicePackageConfiguration : IEntityTypeConfiguration<ServicePackage>
{
    public void Configure(EntityTypeBuilder<ServicePackage> builder)
    {
        builder.ToTable("service_packages");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(26);

        builder.Property(e => e.SalonId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Price).HasPrecision(10, 2);
        builder.Property(e => e.OriginalPrice).HasPrecision(10, 2);
        builder.Property(e => e.ImageUrl).HasMaxLength(500);

        builder.HasIndex(e => e.SalonId);
        builder.HasIndex(e => e.IsActive);

        builder.HasOne(e => e.Salon)
            .WithMany(s => s.ServicePackages)
            .HasForeignKey(e => e.SalonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PackageServiceConfiguration : IEntityTypeConfiguration<PackageService>
{
    public void Configure(EntityTypeBuilder<PackageService> builder)
    {
        builder.ToTable("package_services");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(26);

        builder.Property(e => e.PackageId).IsRequired().HasMaxLength(26);
        builder.Property(e => e.ServiceId).IsRequired().HasMaxLength(26);

        builder.HasIndex(e => new { e.PackageId, e.ServiceId }).IsUnique();

        builder.HasOne(e => e.Package)
            .WithMany(p => p.PackageServices)
            .HasForeignKey(e => e.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Service)
            .WithMany()
            .HasForeignKey(e => e.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

