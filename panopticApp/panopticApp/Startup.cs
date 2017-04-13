using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(panopticApp.Startup))]
namespace panopticApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
