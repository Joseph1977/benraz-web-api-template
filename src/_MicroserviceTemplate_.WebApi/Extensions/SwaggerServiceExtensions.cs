using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Benraz.Infrastructure.Web.Filters;
using Swashbuckle.AspNetCore.Filters;
using System.IO;
using System.Reflection;

namespace _MicroserviceTemplate_.WebApi.Extensions
{
    /// <summary>
    /// /Register the Swagger generator, defining one or more Swagger documents.
    /// </summary>
    public static class SwaggerServiceExtensions
    {
        /// <summary>
        /// Add swagger.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns></returns>
        public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            if (IsSwaggerDisabled(configuration))
                return services;

            services.AddSwaggerGen(options =>
            {
                var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
                }

                options.OperationFilter<SwaggerDefaultValues>();
                options.IncludeXmlComments(GetXmlCommentsFilePath());

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    Description =
                        "Standard Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
                });
                options.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            return services;
        }

        /// <summary>
        /// Use swagger.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <param name="apiVersionDescriptionProvider">API version description provider.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app, IApiVersionDescriptionProvider apiVersionDescriptionProvider, IConfiguration configuration)
        {
            if (IsSwaggerDisabled(configuration))
                return app;

            var SwaggerBasePath = configuration.GetValue<string>("SwaggerBasePath");
            app.UseSwagger(c =>
            {
#if !DEBUG
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Servers = new System.Collections.Generic.List<OpenApiServer>
                {
                    new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/{SwaggerBasePath}" }
                });
#endif
            });
            app.UseSwaggerUI(options =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            return app;
        }

        private static string GetXmlCommentsFilePath()
        {
            var basePath = PlatformServices.Default.Application.ApplicationBasePath;
            var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
            return Path.Combine(basePath, fileName);
        }

        private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = $"_MicroserviceTemplate_ {description.ApiVersion}",
                Version = description.ApiVersion.ToString(),
                Description = "_MicroserviceTemplate_ service",
                Contact = new OpenApiContact { Name = "Joseph Benraz" },
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }

        private static bool IsSwaggerDisabled(IConfiguration configuration)
        {
            return configuration.GetValue<bool>("DisableSwagger");
        }
    }
}