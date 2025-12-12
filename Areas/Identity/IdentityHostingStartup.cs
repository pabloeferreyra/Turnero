[assembly: HostingStartup(typeof(Turnero.Areas.Identity.IdentityHostingStartup))]
namespace Turnero.Areas.Identity;

public class IdentityHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
        });
    }
}