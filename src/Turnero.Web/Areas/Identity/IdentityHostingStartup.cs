[assembly: HostingStartup(typeof(Turnero.Web.Areas.Identity.IdentityHostingStartup))]
namespace Turnero.Web.Areas.Identity;

public class IdentityHostingStartup : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
        });
    }
}