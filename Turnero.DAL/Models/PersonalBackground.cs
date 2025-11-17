namespace Turnero.DAL.Models;

public class PersonalBackground : PatientFKEntity
{
    public bool Asthma { get; set; }
    public bool Allergy { get; set; }
    public bool Pulmonologist { get; set; }
    public bool Pneumonia { get; set; }
    public bool Mumps { get; set; }
    public bool Psicologicals { get; set; }
    public bool Accidents { get; set; }
    public bool HematOnc { get; set; }
    public bool Rubella { get; set; }
    public bool Otitis { get; set; }
    public bool Measles { get; set; }
    public bool Chickenpox { get; set; }
    public bool UrinaryInfections { get; set; }
    public bool Surgeries { get; set; }
    public bool Diabetes { get; set; }
    public bool Digestive { get; set; }
    public string Other { get; set; } = string.Empty;
    public Patient Patient { get; set; }
}
