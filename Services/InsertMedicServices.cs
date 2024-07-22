namespace Turnero.Services;

public class InsertMedicServices(IMedicRepository medicRepository) : IInsertMedicServices
{
    private readonly IMedicRepository _medicRepository = medicRepository;

    public async Task Create(Medic medic)
    {
        try
        {
            //_ = Task.Run(async () =>
            //{
                //_logger.Info($"Creado Medico {medic.Id}");
            //});
            await _medicRepository.NewMedic(medic);
        }
        catch (Exception)
        {
            //_logger.Error(ex.Message, ex);
        }
    }
}
