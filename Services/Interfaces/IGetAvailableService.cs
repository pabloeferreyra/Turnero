namespace Turnero.Services.Interfaces
{
    public interface IGetAvailableService
    {
        List<Available> GetAvailables();
        List<Available> GetAvailablesForMedic(Guid id);
        Available GetAvailable(Guid id);
    }
}
