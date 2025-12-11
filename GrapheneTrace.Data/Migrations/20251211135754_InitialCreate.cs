using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GrapheneTrace.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Clinicians",
                columns: table => new
                {
                    ClinicianID = table.Column<int>(type: "int", nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinicians", x => x.ClinicianID);
                    table.ForeignKey(
                        name: "FK_Clinicians_Users_ClinicianID",
                        column: x => x.ClinicianID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientID = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RiskLevel = table.Column<int>(type: "int", nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MedicalRecordNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientID);
                    table.ForeignKey(
                        name: "FK_Patients_Users_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicianPatients",
                columns: table => new
                {
                    ClinicianPatientID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicianID = table.Column<int>(type: "int", nullable: false),
                    PatientID = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicianPatients", x => x.ClinicianPatientID);
                    table.ForeignKey(
                        name: "FK_ClinicianPatients_Clinicians_ClinicianID",
                        column: x => x.ClinicianID,
                        principalTable: "Clinicians",
                        principalColumn: "ClinicianID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicianPatients_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SensorFrames",
                columns: table => new
                {
                    FrameID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientID = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FrameData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorFrames", x => x.FrameID);
                    table.ForeignKey(
                        name: "FK_SensorFrames_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    AlertID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientID = table.Column<int>(type: "int", nullable: false),
                    FrameID = table.Column<int>(type: "int", nullable: true),
                    AlertType = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAcknowledged = table.Column<bool>(type: "bit", nullable: false),
                    AcknowledgedBy = table.Column<int>(type: "int", nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.AlertID);
                    table.ForeignKey(
                        name: "FK_Alerts_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alerts_SensorFrames_FrameID",
                        column: x => x.FrameID,
                        principalTable: "SensorFrames",
                        principalColumn: "FrameID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalNotes",
                columns: table => new
                {
                    NoteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientID = table.Column<int>(type: "int", nullable: false),
                    ClinicianID = table.Column<int>(type: "int", nullable: false),
                    FrameID = table.Column<int>(type: "int", nullable: true),
                    NoteText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoteType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalNotes", x => x.NoteID);
                    table.ForeignKey(
                        name: "FK_ClinicalNotes_Clinicians_ClinicianID",
                        column: x => x.ClinicianID,
                        principalTable: "Clinicians",
                        principalColumn: "ClinicianID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalNotes_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClinicalNotes_SensorFrames_FrameID",
                        column: x => x.FrameID,
                        principalTable: "SensorFrames",
                        principalColumn: "FrameID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FrameID = table.Column<int>(type: "int", nullable: true),
                    PatientID = table.Column<int>(type: "int", nullable: true),
                    AuthorID = table.Column<int>(type: "int", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParentCommentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentID);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentID",
                        column: x => x.ParentCommentID,
                        principalTable: "Comments",
                        principalColumn: "CommentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_SensorFrames_FrameID",
                        column: x => x.FrameID,
                        principalTable: "SensorFrames",
                        principalColumn: "FrameID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FrameMetrics",
                columns: table => new
                {
                    MetricID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FrameID = table.Column<int>(type: "int", nullable: false),
                    PeakPressureIndex = table.Column<double>(type: "float", nullable: false),
                    ContactAreaPercentage = table.Column<double>(type: "float", nullable: false),
                    AveragePressure = table.Column<double>(type: "float", nullable: false),
                    MaxPressureValue = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrameMetrics", x => x.MetricID);
                    table.ForeignKey(
                        name: "FK_FrameMetrics_SensorFrames_FrameID",
                        column: x => x.FrameID,
                        principalTable: "SensorFrames",
                        principalColumn: "FrameID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 11, 13, 57, 52, 599, DateTimeKind.Local).AddTicks(6068), "admin@graphenetrace.com", "System", true, "Administrator", "$2a$11$1xf9G3i0ubxlfzXhgITQZ../asOB5e6nIBcS2MdehmGQKy993y/Ie", 0, "admin" },
                    { 2, new DateTime(2025, 12, 11, 13, 57, 52, 916, DateTimeKind.Local).AddTicks(4534), "john.smith@hospital.com", "John", true, "Smith", "$2a$11$ghYPK/WJWwQ8wU569DlCOe4tatBiyDf6yOm4ZnT2LJ.Z2PgjAAa8K", 1, "dr.smith" },
                    { 3, new DateTime(2025, 12, 11, 13, 57, 53, 233, DateTimeKind.Local).AddTicks(9624), "mary.johnson@email.com", "Mary", true, "Johnson", "$2a$11$w9r/nITfZDOgtLwm/nnyWeLZatS97IF6QWLZ9TlH5.NYytjw.B5iK", 2, "patient001" }
                });

            migrationBuilder.InsertData(
                table: "Clinicians",
                columns: new[] { "ClinicianID", "Department", "LicenseNumber", "Specialization" },
                values: new object[] { 2, "Internal Medicine", "MC12345", "Wound Care Specialist" });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "PatientID", "AdmissionDate", "DateOfBirth", "MedicalRecordNumber", "RiskLevel" },
                values: new object[] { 3, new DateTime(2025, 12, 4, 13, 57, 53, 234, DateTimeKind.Local).AddTicks(2008), new DateTime(1950, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "MRN001", 2 });

            migrationBuilder.InsertData(
                table: "ClinicianPatients",
                columns: new[] { "ClinicianPatientID", "AssignedDate", "ClinicianID", "PatientID" },
                values: new object[] { 1, new DateTime(2025, 12, 4, 13, 57, 53, 234, DateTimeKind.Local).AddTicks(2305), 2, 3 });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_FrameID",
                table: "Alerts",
                column: "FrameID");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_PatientID_CreatedAt",
                table: "Alerts",
                columns: new[] { "PatientID", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_ClinicianID",
                table: "ClinicalNotes",
                column: "ClinicianID");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_FrameID",
                table: "ClinicalNotes",
                column: "FrameID");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_PatientID",
                table: "ClinicalNotes",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicianPatients_ClinicianID",
                table: "ClinicianPatients",
                column: "ClinicianID");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicianPatients_PatientID",
                table: "ClinicianPatients",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorID",
                table: "Comments",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_FrameID",
                table: "Comments",
                column: "FrameID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentID",
                table: "Comments",
                column: "ParentCommentID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PatientID",
                table: "Comments",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_FrameMetrics_FrameID",
                table: "FrameMetrics",
                column: "FrameID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorFrames_PatientID_Timestamp",
                table: "SensorFrames",
                columns: new[] { "PatientID", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "ClinicalNotes");

            migrationBuilder.DropTable(
                name: "ClinicianPatients");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "FrameMetrics");

            migrationBuilder.DropTable(
                name: "Clinicians");

            migrationBuilder.DropTable(
                name: "SensorFrames");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
