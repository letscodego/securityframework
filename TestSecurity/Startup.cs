using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TestSecurity.Startup))]
namespace TestSecurity
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
