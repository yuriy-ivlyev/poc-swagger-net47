using System;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using PocSwagger.Attributes;
using Swashbuckle.Application;

namespace PocSwagger.App_Start
{
    public static class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableSwagger(c =>
            {
                c.MultipleApiVersions(
                    (apiDesc, targetConsumer) => IsIncluded(apiDesc, targetConsumer),
                    versionBuilder =>
                    {
                        versionBuilder.Version("consumer1", "API – Consumer1");
                        versionBuilder.Version("consumer2", "API – Consumer2");
                    });

                c.DocumentFilter<PocSwagger.SwaggerExtensions.PruneUnusedSchemasFilter>();
            })
            .EnableSwaggerUi(c =>
            {
                c.EnableDiscoveryUrlSelector();
                c.InjectJavaScript(typeof(SwaggerConfig).Assembly, "PocSwagger.SwaggerExtensions.swagger-links.js");
            });
        }

        private static bool IsIncluded(ApiDescription apiDescription, string targetConsumer)
        {
            var actionDescriptor = apiDescription.ActionDescriptor;
            var method = actionDescriptor?.GetCustomAttributes<ApiConsumerAttribute>().FirstOrDefault();
            if (method != null)
                return method.Consumers.Contains(targetConsumer, StringComparer.OrdinalIgnoreCase);

            var controllerType = actionDescriptor?.ControllerDescriptor?.ControllerType;
            var controllerAttr = controllerType?.GetCustomAttribute<ApiConsumerAttribute>();
            if (controllerAttr != null)
                return controllerAttr.Consumers.Contains(targetConsumer, StringComparer.OrdinalIgnoreCase);

            return false;
        }
    }
}

