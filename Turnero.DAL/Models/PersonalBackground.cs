namespace Turnero.DAL.Models;

public class PersonalBackground : PatientFKEntity
{
    [Display(Name = "Asma")]
    public bool Asthma { get; set; }
    [Display(Name = "Alergias")]
    public bool Allergy { get; set; }
    [Display(Name = "Neumonológicos")]
    public bool Pulmonologist { get; set; }
    [Display(Name = "Neumonías")]
    public bool Pneumonia { get; set; }
    [Display(Name = "Paperas")]
    public bool Mumps { get; set; }
    [Display(Name = "Psicologicos")]
    public bool Psicologicals { get; set; }
    [Display(Name = "Accidentes")]
    public bool Accidents { get; set; }
    [Display(Name = "Hematooncológicos")]
    public bool HematOnc { get; set; }
    [Display(Name = "Rubeola")]
    public bool Rubella { get; set; }
    [Display(Name = "Otítis")]
    public bool Otitis { get; set; }
    [Display(Name = "Sarampión")]
    public bool Measles { get; set; }
    [Display(Name = "Varicela")]
    public bool Chickenpox { get; set; }
    [Display(Name = "Infec. Urinarias")]
    public bool UrinaryInfections { get; set; }
    [Display(Name = "Cirugías")]
    public bool Surgeries { get; set; }
    [Display(Name = "Diabetes")]
    public bool Diabetes { get; set; }
    [Display(Name = "Digestivos")]
    public bool Digestive { get; set; }
    [Display(Name = "Otros - Especificar")]
    public string Other { get; set; } = string.Empty;
    public Patient Patient { get; set; }
}
