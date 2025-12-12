#nullable disable

namespace Turnero.Migrations
{
    /// <inheritdoc />
    public partial class MedicalHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "Turns",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Dni = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SocialWork = table.Column<string>(type: "text", nullable: true),
                    AffiliateNumber = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Allergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Begin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                name: "ContactInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    PostalCode = table.Column<string>(type: "text", nullable: false),
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
                name: "BreastCheck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreastCheck", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cancer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Breast = table.Column<bool>(type: "boolean", nullable: false),
                    Ovarian = table.Column<bool>(type: "boolean", nullable: false),
                    Uterine = table.Column<bool>(type: "boolean", nullable: false),
                    Prostate = table.Column<bool>(type: "boolean", nullable: false),
                    Colon = table.Column<bool>(type: "boolean", nullable: false),
                    Pancreatic = table.Column<bool>(type: "boolean", nullable: false),
                    Lung = table.Column<bool>(type: "boolean", nullable: false),
                    Melanoma = table.Column<bool>(type: "boolean", nullable: false),
                    None = table.Column<bool>(type: "boolean", nullable: false),
                    Other = table.Column<bool>(type: "boolean", nullable: false),
                    OtherDescription = table.Column<string>(type: "text", nullable: true),
                    FamiliarId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cancer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cardiovascular",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Hypertension = table.Column<bool>(type: "boolean", nullable: false),
                    Stroke = table.Column<bool>(type: "boolean", nullable: false),
                    MyocardialInfarction = table.Column<bool>(type: "boolean", nullable: false),
                    Arrhythmia = table.Column<bool>(type: "boolean", nullable: false),
                    Hypercholesterolemia = table.Column<bool>(type: "boolean", nullable: false),
                    None = table.Column<bool>(type: "boolean", nullable: false),
                    Other = table.Column<bool>(type: "boolean", nullable: false),
                    OtherDescription = table.Column<string>(type: "text", nullable: true),
                    FamiliarId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cardiovascular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Children",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NumberOfChildren = table.Column<int>(type: "integer", nullable: false),
                    Alive = table.Column<bool>(type: "boolean", nullable: false),
                    Diagnosis = table.Column<string>(type: "text", nullable: false),
                    FamilyBackgroundId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Children", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CholesterolDL",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CholesterolDL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColonoscopyCheck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColonoscopyCheck", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ECGCardiacCheck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECGCardiacCheck", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ECGCheck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECGCheck", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamsGenHis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneralHistoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamsGenHis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FluVaccine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FluVaccine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FluVaccine_ExamsGenHis_ExamsGenHisId",
                        column: x => x.ExamsGenHisId,
                        principalTable: "ExamsGenHis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GynecoCheck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GynecoCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GynecoCheck_ExamsGenHis_ExamsGenHisId",
                        column: x => x.ExamsGenHisId,
                        principalTable: "ExamsGenHis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hemoglobin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hemoglobin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hemoglobin_ExamsGenHis_ExamsGenHisId",
                        column: x => x.ExamsGenHisId,
                        principalTable: "ExamsGenHis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MammographyCheck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MammographyCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MammographyCheck_ExamsGenHis_ExamsGenHisId",
                        column: x => x.ExamsGenHisId,
                        principalTable: "ExamsGenHis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhysicCheck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhysicCheck_ExamsGenHis_ExamsGenHisId",
                        column: x => x.ExamsGenHisId,
                        principalTable: "ExamsGenHis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PneumoVaccine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PneumoVaccine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PneumoVaccine_ExamsGenHis_ExamsGenHisId",
                        column: x => x.ExamsGenHisId,
                        principalTable: "ExamsGenHis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProstaticCheck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProstaticCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProstaticCheck_ExamsGenHis_ExamsGenHisId",
                        column: x => x.ExamsGenHisId,
                        principalTable: "ExamsGenHis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PSACheck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PSACheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PSACheck_ExamsGenHis_ExamsGenHisId",
                        column: x => x.ExamsGenHisId,
                        principalTable: "ExamsGenHis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RectalCheck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RectalCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RectalCheck_ExamsGenHis_ExamsGenHisId",
                        column: x => x.ExamsGenHisId,
                        principalTable: "ExamsGenHis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RetinaCheck",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotApplicable = table.Column<bool>(type: "boolean", nullable: false),
                    Normal = table.Column<bool>(type: "boolean", nullable: false),
                    NotNormal = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: true),
                    ExamsGenHisId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetinaCheck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetinaCheck_ExamsGenHis_ExamsGenHisId",
                        column: x => x.ExamsGenHisId,
                        principalTable: "ExamsGenHis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Familiar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneralHistoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Familiar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Metabolic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiabetesMellitus = table.Column<bool>(type: "boolean", nullable: false),
                    ThyroidDisease = table.Column<bool>(type: "boolean", nullable: false),
                    Obesity = table.Column<bool>(type: "boolean", nullable: false),
                    None = table.Column<bool>(type: "boolean", nullable: false),
                    Other = table.Column<bool>(type: "boolean", nullable: false),
                    OtherDescription = table.Column<string>(type: "text", nullable: true),
                    FamiliarId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metabolic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Metabolic_Familiar_FamiliarId",
                        column: x => x.FamiliarId,
                        principalTable: "Familiar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Neurological",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Epilepsy = table.Column<bool>(type: "boolean", nullable: false),
                    Migraine = table.Column<bool>(type: "boolean", nullable: false),
                    Dementia = table.Column<bool>(type: "boolean", nullable: false),
                    Parkinson = table.Column<bool>(type: "boolean", nullable: false),
                    None = table.Column<bool>(type: "boolean", nullable: false),
                    Other = table.Column<bool>(type: "boolean", nullable: false),
                    OtherDescription = table.Column<string>(type: "text", nullable: true),
                    FamiliarId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Neurological", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Neurological_Familiar_FamiliarId",
                        column: x => x.FamiliarId,
                        principalTable: "Familiar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Psychiatric",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Depression = table.Column<bool>(type: "boolean", nullable: false),
                    Anxiety = table.Column<bool>(type: "boolean", nullable: false),
                    BipolarDisorder = table.Column<bool>(type: "boolean", nullable: false),
                    Schizophrenia = table.Column<bool>(type: "boolean", nullable: false),
                    None = table.Column<bool>(type: "boolean", nullable: false),
                    Other = table.Column<bool>(type: "boolean", nullable: false),
                    OtherDescription = table.Column<string>(type: "text", nullable: true),
                    FamiliarId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Psychiatric", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Psychiatric_Familiar_FamiliarId",
                        column: x => x.FamiliarId,
                        principalTable: "Familiar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyBackgrounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneralHistoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyBackgrounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Father",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Alive = table.Column<bool>(type: "boolean", nullable: false),
                    Diagnosis = table.Column<string>(type: "text", nullable: false),
                    FamilyBackgroundId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Father", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Father_FamilyBackgrounds_FamilyBackgroundId",
                        column: x => x.FamilyBackgroundId,
                        principalTable: "FamilyBackgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mother",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Alive = table.Column<bool>(type: "boolean", nullable: false),
                    Diagnosis = table.Column<string>(type: "text", nullable: false),
                    FamilyBackgroundId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mother", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mother_FamilyBackgrounds_FamilyBackgroundId",
                        column: x => x.FamilyBackgroundId,
                        principalTable: "FamilyBackgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Others",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Relation = table.Column<string>(type: "text", nullable: false),
                    Alive = table.Column<bool>(type: "boolean", nullable: false),
                    Diagnosis = table.Column<string>(type: "text", nullable: false),
                    FamilyBackgroundId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Others", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Others_FamilyBackgrounds_FamilyBackgroundId",
                        column: x => x.FamilyBackgroundId,
                        principalTable: "FamilyBackgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Siblings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NumberOfSiblings = table.Column<int>(type: "integer", nullable: false),
                    Alive = table.Column<bool>(type: "boolean", nullable: false),
                    Diagnosis = table.Column<string>(type: "text", nullable: false),
                    FamilyBackgroundId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Siblings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Siblings_FamilyBackgrounds_FamilyBackgroundId",
                        column: x => x.FamilyBackgroundId,
                        principalTable: "FamilyBackgrounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GeneralHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskFactors = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lifestyle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneralHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Smoking = table.Column<bool>(type: "boolean", nullable: false),
                    CigarettesPerDay = table.Column<int>(type: "integer", nullable: false),
                    YearsSmoking = table.Column<int>(type: "integer", nullable: false),
                    Alcohol = table.Column<bool>(type: "boolean", nullable: false),
                    AlcoholType = table.Column<string>(type: "text", nullable: false),
                    DrinksPerWeek = table.Column<int>(type: "integer", nullable: false),
                    Drugs = table.Column<bool>(type: "boolean", nullable: false),
                    DrugType = table.Column<string>(type: "text", nullable: false),
                    TimesPerWeek = table.Column<int>(type: "integer", nullable: false),
                    DangerousActivities = table.Column<bool>(type: "boolean", nullable: false),
                    DangerousActivitiesDescription = table.Column<string>(type: "text", nullable: false),
                    Excercise = table.Column<bool>(type: "boolean", nullable: false),
                    ExcerciseType = table.Column<string>(type: "text", nullable: false),
                    TimesPerWeekExcercise = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lifestyle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lifestyle_GeneralHistories_GeneralHistoryId",
                        column: x => x.GeneralHistoryId,
                        principalTable: "GeneralHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyBackgroundId = table.Column<Guid>(type: "uuid", nullable: true),
                    FamiliarId = table.Column<Guid>(type: "uuid", nullable: true),
                    LifestyleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Histories_Familiar_FamiliarId",
                        column: x => x.FamiliarId,
                        principalTable: "Familiar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Histories_FamilyBackgrounds_FamilyBackgroundId",
                        column: x => x.FamilyBackgroundId,
                        principalTable: "FamilyBackgrounds",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Histories_Lifestyle_LifestyleId",
                        column: x => x.LifestyleId,
                        principalTable: "Lifestyle",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Histories_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Turns_PatientId",
                table: "Turns",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Allergies_PatientId",
                table: "Allergies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_BreastCheck_ExamsGenHisId",
                table: "BreastCheck",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cancer_FamiliarId",
                table: "Cancer",
                column: "FamiliarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cardiovascular_FamiliarId",
                table: "Cardiovascular",
                column: "FamiliarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Children_FamilyBackgroundId",
                table: "Children",
                column: "FamilyBackgroundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CholesterolDL_ExamsGenHisId",
                table: "CholesterolDL",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ColonoscopyCheck_ExamsGenHisId",
                table: "ColonoscopyCheck",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactInfo_PatientId",
                table: "ContactInfo",
                column: "PatientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ECGCardiacCheck_ExamsGenHisId",
                table: "ECGCardiacCheck",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ECGCheck_ExamsGenHisId",
                table: "ECGCheck",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamsGenHis_GeneralHistoryId",
                table: "ExamsGenHis",
                column: "GeneralHistoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Familiar_GeneralHistoryId",
                table: "Familiar",
                column: "GeneralHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyBackgrounds_GeneralHistoryId",
                table: "FamilyBackgrounds",
                column: "GeneralHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Father_FamilyBackgroundId",
                table: "Father",
                column: "FamilyBackgroundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FluVaccine_ExamsGenHisId",
                table: "FluVaccine",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneralHistories_HistoryId",
                table: "GeneralHistories",
                column: "HistoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GynecoCheck_ExamsGenHisId",
                table: "GynecoCheck",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hemoglobin_ExamsGenHisId",
                table: "Hemoglobin",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Histories_FamiliarId",
                table: "Histories",
                column: "FamiliarId");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_FamilyBackgroundId",
                table: "Histories",
                column: "FamilyBackgroundId");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_LifestyleId",
                table: "Histories",
                column: "LifestyleId");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_PatientId",
                table: "Histories",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Lifestyle_GeneralHistoryId",
                table: "Lifestyle",
                column: "GeneralHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MammographyCheck_ExamsGenHisId",
                table: "MammographyCheck",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metabolic_FamiliarId",
                table: "Metabolic",
                column: "FamiliarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mother_FamilyBackgroundId",
                table: "Mother",
                column: "FamilyBackgroundId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Neurological_FamiliarId",
                table: "Neurological",
                column: "FamiliarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Others_FamilyBackgroundId",
                table: "Others",
                column: "FamilyBackgroundId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicCheck_ExamsGenHisId",
                table: "PhysicCheck",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PneumoVaccine_ExamsGenHisId",
                table: "PneumoVaccine",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProstaticCheck_ExamsGenHisId",
                table: "ProstaticCheck",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PSACheck_ExamsGenHisId",
                table: "PSACheck",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Psychiatric_FamiliarId",
                table: "Psychiatric",
                column: "FamiliarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RectalCheck_ExamsGenHisId",
                table: "RectalCheck",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetinaCheck_ExamsGenHisId",
                table: "RetinaCheck",
                column: "ExamsGenHisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Siblings_FamilyBackgroundId",
                table: "Siblings",
                column: "FamilyBackgroundId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Turns_Patients_PatientId",
                table: "Turns",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BreastCheck_ExamsGenHis_ExamsGenHisId",
                table: "BreastCheck",
                column: "ExamsGenHisId",
                principalTable: "ExamsGenHis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cancer_Familiar_FamiliarId",
                table: "Cancer",
                column: "FamiliarId",
                principalTable: "Familiar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cardiovascular_Familiar_FamiliarId",
                table: "Cardiovascular",
                column: "FamiliarId",
                principalTable: "Familiar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Children_FamilyBackgrounds_FamilyBackgroundId",
                table: "Children",
                column: "FamilyBackgroundId",
                principalTable: "FamilyBackgrounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CholesterolDL_ExamsGenHis_ExamsGenHisId",
                table: "CholesterolDL",
                column: "ExamsGenHisId",
                principalTable: "ExamsGenHis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ColonoscopyCheck_ExamsGenHis_ExamsGenHisId",
                table: "ColonoscopyCheck",
                column: "ExamsGenHisId",
                principalTable: "ExamsGenHis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ECGCardiacCheck_ExamsGenHis_ExamsGenHisId",
                table: "ECGCardiacCheck",
                column: "ExamsGenHisId",
                principalTable: "ExamsGenHis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ECGCheck_ExamsGenHis_ExamsGenHisId",
                table: "ECGCheck",
                column: "ExamsGenHisId",
                principalTable: "ExamsGenHis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamsGenHis_GeneralHistories_GeneralHistoryId",
                table: "ExamsGenHis",
                column: "GeneralHistoryId",
                principalTable: "GeneralHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Familiar_GeneralHistories_GeneralHistoryId",
                table: "Familiar",
                column: "GeneralHistoryId",
                principalTable: "GeneralHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyBackgrounds_GeneralHistories_GeneralHistoryId",
                table: "FamilyBackgrounds",
                column: "GeneralHistoryId",
                principalTable: "GeneralHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralHistories_Histories_HistoryId",
                table: "GeneralHistories",
                column: "HistoryId",
                principalTable: "Histories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Turns_Patients_PatientId",
                table: "Turns");

            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Patients_PatientId",
                table: "Histories");

            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Familiar_FamiliarId",
                table: "Histories");

            migrationBuilder.DropForeignKey(
                name: "FK_Histories_FamilyBackgrounds_FamilyBackgroundId",
                table: "Histories");

            migrationBuilder.DropForeignKey(
                name: "FK_Lifestyle_GeneralHistories_GeneralHistoryId",
                table: "Lifestyle");

            migrationBuilder.DropTable(
                name: "Allergies");

            migrationBuilder.DropTable(
                name: "BreastCheck");

            migrationBuilder.DropTable(
                name: "Cancer");

            migrationBuilder.DropTable(
                name: "Cardiovascular");

            migrationBuilder.DropTable(
                name: "Children");

            migrationBuilder.DropTable(
                name: "CholesterolDL");

            migrationBuilder.DropTable(
                name: "ColonoscopyCheck");

            migrationBuilder.DropTable(
                name: "ContactInfo");

            migrationBuilder.DropTable(
                name: "ECGCardiacCheck");

            migrationBuilder.DropTable(
                name: "ECGCheck");

            migrationBuilder.DropTable(
                name: "Father");

            migrationBuilder.DropTable(
                name: "FluVaccine");

            migrationBuilder.DropTable(
                name: "GynecoCheck");

            migrationBuilder.DropTable(
                name: "Hemoglobin");

            migrationBuilder.DropTable(
                name: "MammographyCheck");

            migrationBuilder.DropTable(
                name: "Metabolic");

            migrationBuilder.DropTable(
                name: "Mother");

            migrationBuilder.DropTable(
                name: "Neurological");

            migrationBuilder.DropTable(
                name: "Others");

            migrationBuilder.DropTable(
                name: "PhysicCheck");

            migrationBuilder.DropTable(
                name: "PneumoVaccine");

            migrationBuilder.DropTable(
                name: "ProstaticCheck");

            migrationBuilder.DropTable(
                name: "PSACheck");

            migrationBuilder.DropTable(
                name: "Psychiatric");

            migrationBuilder.DropTable(
                name: "RectalCheck");

            migrationBuilder.DropTable(
                name: "RetinaCheck");

            migrationBuilder.DropTable(
                name: "Siblings");

            migrationBuilder.DropTable(
                name: "ExamsGenHis");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Familiar");

            migrationBuilder.DropTable(
                name: "FamilyBackgrounds");

            migrationBuilder.DropTable(
                name: "GeneralHistories");

            migrationBuilder.DropTable(
                name: "Histories");

            migrationBuilder.DropTable(
                name: "Lifestyle");

            migrationBuilder.DropIndex(
                name: "IX_Turns_PatientId",
                table: "Turns");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "Turns");
        }
    }
}
