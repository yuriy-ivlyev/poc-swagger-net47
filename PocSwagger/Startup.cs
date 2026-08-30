using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(PocSwagger.Startup))]

namespace PocSwagger
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}