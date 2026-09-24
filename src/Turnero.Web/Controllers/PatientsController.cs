namespace Turnero.Web.Controllers;

[Authorize(Roles = RolesConstants.Medico)]
public class PatientsController(IInsertPatientService insertPatient,
    IGetPatientService getPatient,
    IGetParentsDataService getParents,
    IUpdatePatientService updatePatient,
    IPatientClinicalHistoryService clinicalHistoryService,
    IGetMedicsServices getMedics,
    IGetTimeTurnsServices getTimeTurns,
    IAntiforgery antiforgery) : TurneroBaseController(getMedics, getTimeTurns, antiforgery)
{
    public IActionResult Index()
    {
        return View(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> InitializePatients()
    {
        var (draw, pageSize, skip) = DataTablesHelper.GetDataTableParams(Request);
        var search = Request.Form["Columns[1][search][value]"].FirstOrDefault();
        var patients = await getPatient.SearchPatients(search);
        var data = patients.ToList();

        data = DataTablesHelper.ApplySorting(data, Request);
        var recordsTotal = data.Count;
        data = DataTablesHelper.ApplyPaging(data, pageSize, skip);

        return Ok(new { draw, recordsFiltered = recordsTotal, recordsTotal, data });
    }

    
    [HttpGet]
    public IActionResult Create()
    {
        return PartialView("_Create", new PatientFormViewModel
        {
            Bloodtypes = BloodTypeSelectList.Create()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null)
            return NotFound();
        var patient = await getPatient.GetPatientById(id.Value);
        var parents = await getParents.GetParentsData(id.Value);
        if (patient == null)
        {
            return NotFoundError("Patient", id.ToString());
        }
        var model = new PatientDetailsViewModel
        {
            Patient = patient,
            Parents = parents,
            Age = DateCalculations.CalcularEdad(patient.BirthDate)
        };
        return View("Details", model);
    }

    [HttpGet]
    public async Task<IActionResult> Pdf(Guid? id)
    {
        if (id is null || id == Guid.Empty)
        {
            return NotFound();
        }

        var history = await clinicalHistoryService.GetClinicalHistory(id.Value);
        if (history is null)
        {
            return NotFoundError("Patient", id.Value.ToString());
        }

        var patient = history.Patient!;
        var parent = history.Parent;
        var personalBackground = history.PersonalBackground;
        var perinatalBackground = history.PerinatalBackground;
        var congenitalErrors = history.CongenitalErrors;
        var allergies = history.Allergies;
        var vaccines = history.Vaccines;
        var permanentMedication = history.PermanentMedication;
        var growthCharts = history.GrowthCharts;
        var visits = history.Visits;
        var turns = history.Turns;

        var pdf = Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Margin(30);
                page.Header().Column(header =>
                {
                    header.Item().Text("Historia clínica del paciente").FontSize(20).Bold();
                    header.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor("666666");
                });
                page.Content().Column(column =>
                {
                    column.Spacing(8);
                    AddSection(column, "Datos personales", [
                        ("Nombre", patient.Name),
                        ("DNI", patient.Dni),
                        ("Fecha de nacimiento", patient.BirthDate.ToString("dd/MM/yyyy")),
                        ("Grupo sanguíneo", patient.BloodType.ToString()),
                        ("Obra social", patient.SocialWork),
                        ("Número de afiliado", patient.AffiliateNumber)
                    ]);
                    AddSection(column, "Contacto", [
                        ("Teléfono", patient.ContactInfo?.Phone),
                        ("Email", patient.ContactInfo?.Email),
                        ("Dirección", patient.ContactInfo?.Address),
                        ("Ciudad", patient.ContactInfo?.City),
                        ("Código postal", patient.ContactInfo?.PostalCode)
                    ]);
                    AddSection(column, "Antecedentes familiares", parent is null ? [] : [
                        ("Nombre del padre", parent.FatherName),
                        ("Nacimiento del padre", parent.FatherBirthDate.ToString("dd/MM/yyyy")),
                        ("Grupo sanguíneo del padre", parent.FatherBloodType.ToString()),
                        ("Trabajo del padre", parent.FatherWork),
                        ("Nombre de la madre", parent.MotherName),
                        ("Nacimiento de la madre", parent.MotherBirthDate.ToString("dd/MM/yyyy")),
                        ("Grupo sanguíneo de la madre", parent.MotherBloodType.ToString()),
                        ("Trabajo de la madre", parent.MotherWork),
                        ("Cantidad de hermanos", parent.BrothersCount.ToString())
                    ]);
                    AddSection(column, "Antecedentes personales", personalBackground is null ? [] : [
                        ("Asma", personalBackground.Asthma.ToString()),
                        ("Alergias", personalBackground.Allergy.ToString()),
                        ("Neumonológicos", personalBackground.Pulmonologist.ToString()),
                        ("Neumonías", personalBackground.Pneumonia.ToString()),
                        ("Paperas", personalBackground.Mumps.ToString()),
                        ("Psicológicos", personalBackground.Psicologicals.ToString()),
                        ("Accidentes", personalBackground.Accidents.ToString()),
                        ("Hematooncológicos", personalBackground.HematOnc.ToString()),
                        ("Rubéola", personalBackground.Rubella.ToString()),
                        ("Otitis", personalBackground.Otitis.ToString()),
                        ("Sarampión", personalBackground.Measles.ToString()),
                        ("Varicela", personalBackground.Chickenpox.ToString()),
                        ("Infecciones urinarias", personalBackground.UrinaryInfections.ToString()),
                        ("Cirugías", personalBackground.Surgeries.ToString()),
                        ("Diabetes", personalBackground.Diabetes.ToString()),
                        ("Digestivos", personalBackground.Digestive.ToString()),
                        ("Otros", personalBackground.Other)
                    ]);
                    AddSection(column, "Antecedentes perinatales", perinatalBackground is null ? [] : [
                        ("Gesta", perinatalBackground.Feat.ToString()),
                        ("Parto", perinatalBackground.Delivery.ToString()),
                        ("Cesárea", perinatalBackground.Cesarean.ToString()),
                        ("Aborto", perinatalBackground.Abort.ToString()),
                        ("Peso", perinatalBackground.Weight.ToString()),
                        ("Talla", perinatalBackground.Height.ToString()),
                        ("Perímetro cefálico", perinatalBackground.CefPer.ToString()),
                        ("Apgar 1'", perinatalBackground.Apgar1.ToString()),
                        ("Apgar 5'", perinatalBackground.Apgar5.ToString()),
                        ("Edad gestacional", perinatalBackground.GestAge.ToString()),
                        ("Patologías", perinatalBackground.Pathologies),
                        ("Errores congénitos", perinatalBackground.CongErrors)
                    ]);
                    AddSection(column, "Errores congénitos", congenitalErrors is null ? [] : [
                        ("Hipotiroidismo congénito", congenitalErrors.CongHypothyroidism.ToString()),
                        ("Resultado hipotiroidismo", congenitalErrors.ResultHypothyroidism),
                        ("Fenilcetonuria", congenitalErrors.Phenylalanine.ToString()),
                        ("Resultado fenilcetonuria", congenitalErrors.ResultPhenylalanine),
                        ("Fibrosis quística", congenitalErrors.FQP.ToString()),
                        ("Resultado fibrosis quística", congenitalErrors.ResultFQP),
                        ("Deficiencia de biotinidasa", congenitalErrors.Biotinidase.ToString()),
                        ("Resultado biotinidasa", congenitalErrors.ResultBiotinidase),
                        ("Galactosemia", congenitalErrors.Galactosemia.ToString()),
                        ("Resultado galactosemia", congenitalErrors.ResultGalactosemia),
                        ("17O hidroxiprogesterona", congenitalErrors.OHP.ToString()),
                        ("Resultado 17O hidroxiprogesterona", congenitalErrors.ResultOHP),
                        ("Otros", congenitalErrors.Other)
                    ]);

                    AddListSection(column, "Alergias", allergies.Select(item => string.Join(" | ", new string?[] {
                        item.Name,
                        $"Inicio: {item.Begin:dd/MM/yyyy}",
                        $"Fin: {(item.End.HasValue ? item.End.Value.ToString("dd/MM/yyyy") : "actual")}",
                        $"Gravedad: {item.Severity}",
                        $"Tipo: {item.Type}",
                        item.Description,
                        item.Comments
                    }.Where(value => !string.IsNullOrWhiteSpace(value)))));
                    AddListSection(column, "Vacunas", vaccines.Select(item => $"{item.Description} | Fecha: {(item.DateApplied?.ToString("dd/MM/yyyy") ?? "sin fecha")}"));
                    AddListSection(column, "Medicación permanente", permanentMedication.Select(item => item.Description));
                    AddListSection(column, "Tablas de crecimiento", growthCharts.Select(item => $"Edad: {item.Age} | Fecha: {item.Time} | Peso: {item.Weight} ({item.WPerc}) | Talla: {item.Height} ({item.HPerc}) | Perímetro cefálico: {item.HeadCircumference} ({item.HCPerc}) | IMC: {item.Bmi}"));
                    AddListSection(column, "Visitas", visits.Select(item => $"{item.VisitDate:dd/MM/yyyy} | Médico: {item.Medic?.Name ?? "sin asignar"} | Motivo: {item.Reason} | Diagnóstico: {item.Diagnosis} | Tratamiento: {item.Treatment} | Evolución: {item.EvolutionNotes} | Observaciones: {item.Observations}"));
                    AddListSection(column, "Turnos", turns.Select(item => $"{item.DateTurn:dd/MM/yyyy} {item.Time?.Time} | Médico: {item.Medic?.Name ?? "sin asignar"} | Motivo: {item.Reason} | Ingresado: {item.Accessed}"));
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Paciente: ");
                    text.Span(patient.Name ?? string.Empty).Bold();
                    text.Span(" | Página ");
                    text.CurrentPageNumber();
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"paciente_{patient.Dni ?? patient.Id.ToString()}.pdf");
    }

    static void AddSection(ColumnDescriptor column, string title, IEnumerable<(string Label, string? Value)> values)
    {
        var entries = values.Where(item => !string.IsNullOrWhiteSpace(item.Value)).ToList();
        if (entries.Count == 0)
        {
            return;
        }

        column.Item().Text(title).FontSize(13).Bold().FontColor("1F4E79");
        foreach (var entry in entries)
        {
            column.Item().Text(text =>
            {
                text.Span($"{entry.Label}: ").Bold();
                text.Span(entry.Value!);
            });
        }
    }

    static void AddListSection(ColumnDescriptor column, string title, IEnumerable<string?> values)
    {
        var entries = values.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        if (entries.Count == 0)
        {
            return;
        }

        column.Item().Text(title).FontSize(13).Bold().FontColor("1F4E79");
        foreach (var entry in entries)
        {
            column.Item().Text($"• {entry}");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Create(Patient patient)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await insertPatient.InsertPatient(patient);
            return Ok();
        }
        catch (Exception)
        {
            return Conflict();
        }
    }
    [HttpGet]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
            return NotFound();

        var patient = await getPatient.GetPatientById(id.Value);
        if (patient == null)
        {
            return NotFoundError("Patient", id.ToString());
        }
        return PartialView("_Edit", new PatientFormViewModel(patient)
        {
            Bloodtypes = BloodTypeSelectList.Create()
        });
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> Edit(Patient patient)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await updatePatient.UpdatePatient(patient);
            return Ok();
        }
        catch (Exception)
        {
            return Conflict();
        }
    }

    [HttpDelete, ActionName("Delete")]
    [Authorize(Roles = RolesConstants.Admin + ", " + RolesConstants.Medico)]
    [ValidateAntiForgeryToken]
    public async Task<StatusCodeResult> DeletePatient(Guid id)
    {
        try
        {
            await updatePatient.DeletePatient(id);
            return Ok();
        }
        catch (Exception)
        {
            return Conflict();
        }
    }
}
