using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using PocSwagger.Attributes;
using PocSwagger.Filters;
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
                    (apiDescription, targetApiVersion) => IsIncluded(apiDescription, targetApiVersion),
                    vc =>
                    {
                        vc.Version("consumer2", "API – Consumer 2");
                        vc.Version("consumer1", "API – Consumer 1");
                    });

                c.DocumentFilter<PruneUnusedSchemasFilter>();

                // Resolve attribute-based XML comments if an XML doc file is present
                // c.IncludeXmlComments(GetXmlCommentsPath());
            })
            .EnableSwaggerUi(ui =>
            {
                ui.EnableDiscoveryUrlSelector();
            });
        }

        /// <summary>
        /// Returns <c>true</c> when <paramref name="apiDescription"/> should appear
        /// in the Swagger document for <paramref name="targetConsumer"/>.
        /// Resolution order: action attribute → controller attribute → exclude.
        /// </summary>
        private static bool IsIncluded(ApiDescription apiDescription, string targetConsumer)
        {
            // 1. Check action-level attribute
            var actionAttr = apiDescription.ActionDescriptor
                .GetCustomAttributes<ApiConsumerAttribute>()
                .FirstOrDefault();

            if (actionAttr != null)
                return actionAttr.Consumers.Contains(targetConsumer, StringComparer.OrdinalIgnoreCase);

            // 2. Fall back to controller-level attribute
            var controllerAttr = apiDescription.ActionDescriptor.ControllerDescriptor
                .GetCustomAttributes<ApiConsumerAttribute>()
                .FirstOrDefault();

            if (controllerAttr != null)
                return controllerAttr.Consumers.Contains(targetConsumer, StringComparer.OrdinalIgnoreCase);

            // 3. No attribute → exclude from all consumer documents
            return false;
        }
    }
}
