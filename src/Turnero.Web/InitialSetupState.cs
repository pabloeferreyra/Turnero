namespace Turnero.Web;

public sealed class InitialSetupState
{
    private int _isRequired = 1;

    public bool IsRequired => Volatile.Read(ref _isRequired) == 1;

    public SemaphoreSlim Gate { get; } = new(1, 1);

    public void Initialize(bool isRequired) =>
        Volatile.Write(ref _isRequired, isRequired ? 1 : 0);

    public void Complete() =>
        Interlocked.Exchange(ref _isRequired, 0);
}
