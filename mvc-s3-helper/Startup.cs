using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC_S3_Helper.Startup))]
namespace MVC_S3_Helper
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
