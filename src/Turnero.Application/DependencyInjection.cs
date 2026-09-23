using Microsoft.Extensions.DependencyInjection;

namespace Turnero.Application;

/// <summary>
/// Registro de dependencias de la capa Application.
/// La presentación solo necesita invocar services.AddApplication().
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Turns
        services.AddScoped<IInsertTurnsServices, InsertTurnsServices>();
        services.AddScoped<IUpdateTurnsServices, UpdateTurnsServices>();
        services.AddScoped<IGetTurnsServices, GetTurnsServices>();
        services.AddScoped<IGetTurnDTOServices, GetTurnDTOServices>();
        services.AddScoped<IGetDashboardDataService, GetDashboardDataService>();

        // Medics
        services.AddScoped<IInsertMedicServices, InsertMedicServices>();
        services.AddScoped<IUpdateMedicServices, UpdateMedicServices>();
        services.AddScoped<IGetMedicsServices, GetMedicsServices>();

        // TimeTurns
        services.AddScoped<IInsertTimeTurnServices, InsertTimeTurnServices>();
        services.AddScoped<IDeleteTimeTurnServices, DeleteTimeTurnServices>();
        services.AddScoped<IGetTimeTurnsServices, GetTimeTurnsServices>();

        // Patients
        services.AddScoped<IInsertPatientService, InsertPatientService>();
        services.AddScoped<IGetPatientService, GetPatientService>();
        services.AddScoped<IUpdatePatientService, UpdatePatientService>();

        // Visits
        services.AddScoped<IGetVisitService, GetVisitService>();
        services.AddScoped<IInsertVisitService, InsertVisitService>();

        // Allergies
        services.AddScoped<IGetAllergiesServices, GetAllergiesServices>();
        services.AddScoped<IInsertAllergiesServices, InsertAllergiesServices>();
        services.AddScoped<IUpdateAllergiesServices, UpdateAllergiesServices>();
        services.AddScoped<IDeleteAllergiesServices, DeleteAllergiesServices>();

        // ParentsData
        services.AddScoped<IGetParentsDataService, GetParentsDataService>();
        services.AddScoped<IUpdateParentsDataService, UpdateParentsDataService>();
        services.AddScoped<IDeleteParentsDataService, DeleteParentsDataService>();

        // PersonalBackground
        services.AddScoped<IGetPersonalBackgroundService, GetPersonalBackgroundService>();
        services.AddScoped<IUpdatePersonalBackgroundService, UpdatePersonalBackgroundService>();

        // PerinatalBackground
        services.AddScoped<IGetPerinatalBackgroundService, GetPerinatalBackgroundService>();
        services.AddScoped<IUpdatePerinatalBackgroundService, UpdatePerinatalBackgroundService>();

        // Vaccines
        services.AddScoped<IGetVaccinesServices, GetVaccinesServices>();
        services.AddScoped<IUpdateVaccinesServices, UpdateVaccinesServices>();
        services.AddScoped<IInsertVaccinesServices, InsertVaccinesServices>();
        services.AddScoped<IDeleteVacinesServices, DeleteVacinesServices>();

        // PermMed
        services.AddScoped<IGetPermMedService, GetPermMedService>();
        services.AddScoped<IInsertPermMedService, InsertPermMedService>();
        services.AddScoped<IDeletePermMedService, DeletePermMedService>();

        // GrowthChart
        services.AddScoped<IGetGrowthChartService, GetGrowthChartService>();
        services.AddScoped<IUpdateGrowthChartService, UpdateGrowthChartService>();
        services.AddScoped<IInsertGrowthChartService, InsertGrowthChartService>();
        services.AddScoped<IDeleteGrowthChartService, DeleteGrowthChartService>();

        // CongErrors
        services.AddScoped<IGetCongErrorService, GetCongErrorService>();
        services.AddScoped<IUpdateCongErrorService, UpdateCongErrorService>();

        // Cross-cutting
        services.AddScoped<ICachedQueryService, CachedQueryService>();

        return services;
    }
}
