namespace Turnero.Utilities.Utilities;

/// <summary>
/// Shared date calculation utilities.
/// </summary>
public static class DateCalculations
{
    /// <summary>
    /// Calculates a human-readable age string from a birth date.
    /// Returns years, months, weeks, or days as appropriate.
    /// </summary>
    public static string CalcularEdad(DateTime fechaNacimiento, DateTime? fechaReferencia = null)
    {
        var hoy = fechaReferencia?.Date ?? DateTime.Today;

        if (fechaNacimiento.Date > hoy)
            throw new ArgumentException("La fecha de nacimiento no puede ser futura.");

        // AÑOS
        int años = hoy.Year - fechaNacimiento.Year;
        if (fechaNacimiento.AddYears(años) > hoy)
            años--;

        if (años >= 1)
            return $"{años} año{(años > 1 ? "s" : "")}";

        // MESES
        int meses = (hoy.Year - fechaNacimiento.Year) * 12 + hoy.Month - fechaNacimiento.Month;
        if (fechaNacimiento.AddMonths(meses) > hoy)
            meses--;

        if (meses >= 1)
            return $"{meses} mes{(meses > 1 ? "es" : "")}";

        // DÍAS / SEMANAS
        int dias = (hoy - fechaNacimiento.Date).Days;

        if (dias >= 7)
        {
            int semanas = dias / 7;
            return $"{semanas} semana{(semanas > 1 ? "s" : "")}";
        }

        return $"{dias} día{(dias != 1 ? "s" : "")}";
    }
}
