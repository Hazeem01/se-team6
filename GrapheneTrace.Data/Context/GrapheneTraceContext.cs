using GrapheneTrace.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace GrapheneTrace.Data.Context
{
    public class GrapheneTraceContext : DbContext
    {
        public GrapheneTraceContext(DbContextOptions<GrapheneTraceContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Clinician> Clinicians { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<ClinicianPatient> ClinicianPatients { get; set; }
        public DbSet<SensorFrame> SensorFrames { get; set; }
        public DbSet<FrameMetric> FrameMetrics { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ClinicalNote> ClinicalNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Clinician configuration
            modelBuilder.Entity<Clinician>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithOne(u => u.Clinician)
                    .HasForeignKey<Clinician>(c => c.ClinicianID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Patient configuration
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasOne(p => p.User)
                    .WithOne(u => u.Patient)
                    .HasForeignKey<Patient>(p => p.PatientID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ClinicianPatient configuration
            modelBuilder.Entity<ClinicianPatient>(entity =>
            {
                entity.HasOne(cp => cp.Clinician)
                    .WithMany(c => c.ClinicianPatients)
                    .HasForeignKey(cp => cp.ClinicianID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cp => cp.Patient)
                    .WithMany(p => p.ClinicianPatients)
                    .HasForeignKey(cp => cp.PatientID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SensorFrame configuration
            modelBuilder.Entity<SensorFrame>(entity =>
            {
                entity.HasOne(sf => sf.Patient)
                    .WithMany(p => p.SensorFrames)
                    .HasForeignKey(sf => sf.PatientID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.PatientID, e.Timestamp });
            });

            // FrameMetric configuration
            modelBuilder.Entity<FrameMetric>(entity =>
            {
                entity.HasOne(fm => fm.SensorFrame)
                    .WithOne(sf => sf.FrameMetric)
                    .HasForeignKey<FrameMetric>(fm => fm.FrameID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Alert configuration
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasOne(a => a.Patient)
                    .WithMany(p => p.Alerts)
                    .HasForeignKey(a => a.PatientID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.SensorFrame)
                    .WithMany(sf => sf.Alerts)
                    .HasForeignKey(a => a.FrameID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.PatientID, e.CreatedAt });
            });

            // Comment configuration
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.SensorFrame)
                    .WithMany(sf => sf.Comments)
                    .HasForeignKey(c => c.FrameID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Patient)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(c => c.PatientID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Author)
                    .WithMany()
                    .HasForeignKey(c => c.AuthorID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ClinicalNote configuration
            modelBuilder.Entity<ClinicalNote>(entity =>
            {
                entity.HasOne(cn => cn.Patient)
                    .WithMany(p => p.ClinicalNotes)
                    .HasForeignKey(cn => cn.PatientID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cn => cn.Clinician)
                    .WithMany(c => c.ClinicalNotes)
                    .HasForeignKey(cn => cn.ClinicianID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cn => cn.SensorFrame)
                    .WithMany(sf => sf.ClinicalNotes)
                    .HasForeignKey(cn => cn.FrameID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserID = 1,
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = UserRole.Admin,
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = "admin@graphenetrace.com",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new User
                {
                    UserID = 2,
                    Username = "dr.smith",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor123!"),
                    Role = UserRole.Clinician,
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "john.smith@hospital.com",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new User
                {
                    UserID = 3,
                    Username = "patient001",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient123!"),
                    Role = UserRole.Patient,
                    FirstName = "Mary",
                    LastName = "Johnson",
                    Email = "mary.johnson@email.com",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                }
            );

            // Seed Clinician
            modelBuilder.Entity<Clinician>().HasData(
                new Clinician
                {
                    ClinicianID = 2,
                    Specialization = "Wound Care Specialist",
                    LicenseNumber = "MC12345",
                    Department = "Internal Medicine"
                }
            );

            // Seed Patient
            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    PatientID = 3,
                    DateOfBirth = new DateTime(1950, 5, 15),
                    RiskLevel = RiskLevel.High,
                    AdmissionDate = DateTime.Now.AddDays(-7),
                    MedicalRecordNumber = "MRN001"
                }
            );

            // Seed ClinicianPatient relationship
            modelBuilder.Entity<ClinicianPatient>().HasData(
                new ClinicianPatient
                {
                    ClinicianPatientID = 1,
                    ClinicianID = 2,
                    PatientID = 3,
                    AssignedDate = DateTime.Now.AddDays(-7)
                }
            );
        }
    }
}