using System;
using System.Web;
using System.Web.Http;
using PocSwagger.App_Start;

namespace PocSwagger
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var config = GlobalConfiguration.Configuration;
            WebApiConfig.Register(config);
            SwaggerConfig.Register(config);
            config.EnsureInitialized();
        }
    }
}
