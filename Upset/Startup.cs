using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Upset.Startup))]
namespace Upset
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}
