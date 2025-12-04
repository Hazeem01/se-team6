using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sensore.Models;

namespace Sensore.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Alert> Alerts => Set<Alert>();
        public DbSet<PatientAssignment> PatientAssignments => Set<PatientAssignment>();

        public DbSet<SensorMetric> SensorMetrics{ get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Alert>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Alert>()
                .HasIndex(a => new { a.UserId, a.StartTs });

            builder.Entity<PatientAssignment>()
                .HasOne(pa => pa.Patient)
                .WithMany()
                .HasForeignKey(pa => pa.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PatientAssignment>()
                .HasOne(pa => pa.Clinician)
                .WithMany()
                .HasForeignKey(pa => pa.ClinicianId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PatientAssignment>()
                .HasOne(pa => pa.Doctor)
                .WithMany()
                .HasForeignKey(pa => pa.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
