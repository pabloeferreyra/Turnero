namespace Turnero.Utilities.Utilities;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Medic, MedicDto>.NewConfig();

        TypeAdapterConfig<Turn, TurnDTO>.NewConfig()
            .Map(dest => dest.Time, src => src.Time)
            .Map(dest => dest.TimeId, src => src.Time.Id)
            .Map(dest => dest.MedicName, src => src.Medic)
            .Map(dest => dest.MedicId, src => src.Medic)
            .Ignore(dest => dest.IsMedic)
            .Map(dest => dest.Date, src => src.DateTurn.ToString("dd/MM/yyyy"));

        TypeAdapterConfig<TurnDTO, Turn>.NewConfig()
            .Map(dest => dest.TimeId, src => src.Time)
            .Ignore(dest => dest.Time)
            .Map(dest => dest.MedicId, src => src.MedicId)
            .Ignore(dest => dest.Medic)
            .Map(dest => dest.DateTurn, src => DateTime.Parse(src.Date ?? DateTime.MinValue.ToString("dd/MM/yyyy")));
    }
}