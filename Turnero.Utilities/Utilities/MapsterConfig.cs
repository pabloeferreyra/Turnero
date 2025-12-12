namespace Turnero.Utilities.Utilities;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Patient, PatientDTO>.NewConfig();

        TypeAdapterConfig<Medic, MedicDto>.NewConfig();

        TypeAdapterConfig<Turn, TurnDTO>.NewConfig()
            .Map(dest => dest.Time, src => src.Time != null ? src.Time.Time : null)
            .Map(dest => dest.TimeId, src => src.TimeId)
            .Map(dest => dest.MedicName, src => src.Medic != null ? src.Medic.Name : null)
            .Map(dest => dest.MedicId, src => src.MedicId)
            .Ignore(dest => dest.IsMedic)
            .Map(dest => dest.Date, src => src.DateTurn.ToString("yyyy-MM-dd"));


        TypeAdapterConfig<TurnDTO, Turn>.NewConfig()
            .Map(dest => dest.Time.Id, src => src.TimeId)
            .Ignore(dest => dest.Time)
            .Map(dest => dest.Medic.Id, src => src.MedicId)
            .Ignore(dest => dest.Medic)
            .Map(dest => dest.DateTurn,
            src => !string.IsNullOrEmpty(src.Date) ? DateTime.ParseExact(src.Date, "yyyy-MM-dd", null) : DateTime.MinValue);

        TypeAdapterConfig<Visit, VisitDTO>.NewConfig()
            .Map(dest => dest.VisitDate, src => src.VisitDate.ToString("yyyy-MM-dd"))
            .Map(dest => dest.Medic, src => src.Medic != null ? src.Medic.Name : null);
    }
}