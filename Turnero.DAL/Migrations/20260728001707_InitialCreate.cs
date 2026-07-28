using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Turnero.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    UserGuid = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Dni = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: false),
                    SocialWork = table.Column<string>(type: "text", nullable: true),
                    AffiliateNumber = table.Column<string>(type: "text", nullable: true),
                    BloodType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeTurns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Time = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTurns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Allergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Begin = table.Column<DateOnly>(type: "date", nullable: false),
                    End = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Occurrency = table.Column<int>(type: "integer", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Allergies_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CongErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CongHypothyroidism = table.Column<bool>(type: "boolean", nullable: false),
                    ResultHypothyroidism = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    Phenylalanine = table.Column<bool>(type: "boolean", nullable: false),
                    ResultPhenylalanine = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    FQP = table.Column<bool>(type: "boolean", nullable: false),
                    ResultFQP = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    Other = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    Biotinidase = table.Column<bool>(type: "boolean", nullable: false),
                    ResultBiotinidase = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    Galactosemia = table.Column<bool>(type: "boolean", nullable: false),
                    ResultGalactosemia = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    OHP = table.Column<bool>(type: "boolean", nullable: false),
                    ResultOHP = table.Column<string>(type: "text", nullable: true, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CongErrors_Patients_Id",
                        column: x => x.Id,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactInfo_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrowthCharts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Age = table.Column<decimal>(type: "numeric(6,3)", precision: 6, scale: 3, nullable: false),
                    Time = table.Column<string>(type: "text", nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(13,3)", precision: 13, scale: 3, nullable: false),
                    WPerc = table.Column<string>(type: "text", nullable: true),
                    Height = table.Column<decimal>(type: "numeric(13,3)", precision: 13, scale: 3, nullable: false),
                    HPerc = table.Column<string>(type: "text", nullable: true),
                    HeadCircumference = table.Column<decimal>(type: "numeric(13,3)", precision: 13, scale: 3, nullable: false),
                    HCPerc = table.Column<string>(type: "text", nullable: true),
                    Bmi = table.Column<decimal>(type: "numeric(13,3)", precision: 13, scale: 3, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrowthCharts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrowthCharts_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParentsData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FatherName = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    FatherBirthDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValue: new DateOnly(1, 1, 1)),
                    FatherBloodType = table.Column<int>(type: "integer", nullable: false),
                    FatherWork = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    MotherName = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    MotherBirthDate = table.Column<DateOnly>(type: "date", nullable: false, defaultValue: new DateOnly(1, 1, 1)),
                    MotherBloodType = table.Column<int>(type: "integer", nullable: false),
                    MotherWork = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    BrothersCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentsData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentsData_Patients_Id",
                        column: x => x.Id,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerinatalBackground",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Feat = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Delivery = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Cesarean = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Abort = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Weight = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Height = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CefPer = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Apgar1 = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Apgar5 = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GestAge = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Pathologies = table.Column<string>(type: "text", nullable: true),
                    CongErrors = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerinatalBackground", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerinatalBackground_Patients_Id",
                        column: x => x.Id,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PermMeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermMeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermMeds_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonalBackground",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Asthma = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Allergy = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Pulmonologist = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Pneumonia = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Mumps = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Psicologicals = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Accidents = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HematOnc = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Rubella = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Otitis = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Measles = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Chickenpox = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UrinaryInfections = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Surgeries = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Diabetes = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Digestive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Other = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalBackground", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalBackground_Patients_Id",
                        column: x => x.Id,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vaccines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DateApplied = table.Column<DateOnly>(type: "date", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vaccines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vaccines_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    Diagnosis = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    DiagDescription = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    Treatment = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvolutionNotes = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    LabResults = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    OtherStudies = table.Column<string>(type: "text", nullable: true, defaultValue: ""),
                    Observations = table.Column<string>(type: "text", nullable: true, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visits_Medics_MedicId",
                        column: x => x.MedicId,
                        principalTable: "Medics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Visits_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Dni = table.Column<long>(type: "bigint", nullable: false),
                    MedicId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateTurn = table.Column<DateTime>(type: "date", nullable: false),
                    TimeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SocialWork = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Accessed = table.Column<bool>(type: "boolean", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turns_Medics_MedicId",
                        column: x => x.MedicId,
                        principalTable: "Medics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Turns_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Turns_TimeTurns_TimeId",
                        column: x => x.TimeId,
                        principalTable: "TimeTurns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Allergies_PatientId",
                table: "Allergies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfo_PatientId",
                table: "ContactInfo",
                column: "PatientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrowthCharts_PatientId",
                table: "GrowthCharts",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PermMeds_PatientId",
                table: "PermMeds",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_MedicId",
                table: "Turns",
                column: "MedicId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_PatientId",
                table: "Turns",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Turns_TimeId",
                table: "Turns",
                column: "TimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Vaccines_PatientId",
                table: "Vaccines",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_MedicId",
                table: "Visits",
                column: "MedicId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_PatientId",
                table: "Visits",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Allergies");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CongErrors");

            migrationBuilder.DropTable(
                name: "ContactInfo");

            migrationBuilder.DropTable(
                name: "GrowthCharts");

            migrationBuilder.DropTable(
                name: "ParentsData");

            migrationBuilder.DropTable(
                name: "PerinatalBackground");

            migrationBuilder.DropTable(
                name: "PermMeds");

            migrationBuilder.DropTable(
                name: "PersonalBackground");

            migrationBuilder.DropTable(
                name: "Turns");

            migrationBuilder.DropTable(
                name: "Vaccines");

            migrationBuilder.DropTable(
                name: "Visits");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "TimeTurns");

            migrationBuilder.DropTable(
                name: "Medics");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
