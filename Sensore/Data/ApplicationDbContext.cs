using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sensore.Models;

namespace Sensore.Data
{

    /// Application DbContext.
    /// Inherits IdentityDbContext to include ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, and so on).
    /// Domain tables: Alerts, PatientAssignments, SensorMetrics.
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        /// Alerts table: stores generated alerts associated to a user account.
        public DbSet<Alert> Alerts => Set<Alert>();

        /// PatientAssignment table: links patients to clinicians and doctors.

        public DbSet<PatientAssignment> PatientAssignments => Set<PatientAssignment>();

        /// Time-series sensor metrics used to render charts and heatmaps.

        public DbSet<SensorMetric> SensorMetrics{ get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Alert -> IdentityUser (UserId)
            builder.Entity<Alert>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Alert>()
                .HasIndex(a => new { a.UserId, a.StartTs });

            // PatientAssignment relationships to IdentityUser
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
