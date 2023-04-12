using _MicroserviceTemplate_.Domain.Authorization;
using _MicroserviceTemplate_.Domain.Jobs;
using _MicroserviceTemplate_.Domain.Settings;
using _MicroserviceTemplate_.EF;
using _MicroserviceTemplate_.EF.Repositories;
using _MicroserviceTemplate_.EF.Services;
using _MicroserviceTemplate_.WebApi.Authorization;
using _MicroserviceTemplate_.WebApi.Controllers;
using _MicroserviceTemplate_.WebApi.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Benraz.Infrastructure.Authorization.Tokens;
using Benraz.Infrastructure.Common.AccessControl;
using Benraz.Infrastructure.Common.BackgroundQueue;
using Benraz.Infrastructure.Common.DataRedundancy;
using Benraz.Infrastructure.Gateways.BenrazAuthorization.Auth;
using Benraz.Infrastructure.Web.Authorization;
using Benraz.Infrastructure.Web.Filters;
using System;
using System.Reflection;

namespace _MicroserviceTemplate_.WebApi
{
    /// <summary>
    /// Startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Creates startup instance.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configures application services.
        /// </summary>
        /// <param name="services">Services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddCors();

            ConfigureSqlServerContext(services);

            services.AddAutoMapper(typeof(_MicroserviceTemplate_AutoMapperProfile));

            services.AddHttpClient();

            services
                .AddControllers()
                .AddApplicationPart(Assembly.GetAssembly(typeof(ITController)));

            AddVersioning(services);

            services.AddSwagger(Configuration);

            AddServices(services);
            AddAuthorization(services);
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <param name="env">Environment.</param>
        /// <param name="apiVersionDescriptionProvider">API version description provider.</param>
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            app.UseCors(options =>
            {
                options
                    .WithOrigins(Configuration.GetValue<string>("AllowedHosts"))
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            UseDatabaseMigrations(app);

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();

            app.UseSwagger(apiVersionDescriptionProvider, Configuration);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureSqlServerContext(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("_MicroserviceTemplate_");
            if (IsInjectDbCredentialsToConnectionString())
            {
                connectionString +=
                    $";User Id={Environment.GetEnvironmentVariable("ASPNETCORE_DB_USERNAME")};Password={Environment.GetEnvironmentVariable("ASPNETCORE_DB_PASSWORD")}";
            }

            services.AddDbContext<_MicroserviceTemplate_DbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    o => o.EnableRetryOnFailure(3)
                ));
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IDrChecker, DrChecker>();
            services.AddScoped<DRFilterAttribute>();
            services.AddScoped<ErrorFilterAttribute>();

            services.AddTransient<IDbMigrationService, _MicroserviceTemplate_DbMigrationService>();

            services.AddTransient<ISettingsEntriesRepository, SettingsEntriesRepository>();

            services.AddTransient<IEmptyRepeatableJobsService, EmptyRepeatableJobsService>();

            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<QueuedHostedService>();
        }

        /// <summary>
        /// Adds authorization.
        /// </summary>
        /// <param name="services">Services.</param>
        protected virtual void AddAuthorization(IServiceCollection services)
        {
            services.Configure<BenrazAuthorizationAuthGatewaySettings>(x =>
            {
                x.BaseUrl = Configuration.GetValue<string>("General:AuthorizationBaseUrl");
            });
            services.AddTransient<IBenrazAuthorizationAuthGateway, BenrazAuthorizationAuthGateway>();

            services.Configure<TokenValidationServiceSettings>(Configuration.GetSection("TokenValidation"));
            services.AddTransient<ITokenValidationService, TokenValidationService>();

            services.AddTransient<TokenValidator>();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var serviceProvider = services.BuildServiceProvider();

                    var tokenValidationService = serviceProvider.GetRequiredService<ITokenValidationService>();
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew = TimeSpan.FromMinutes(5),
                        IssuerSigningKeyResolver = tokenValidationService.IssuerSigningKeyResolver,
                        ValidateAudience = true,
                        AudienceValidator = tokenValidationService.AudienceValidator,
                        ValidateIssuer = true,
                        IssuerValidator = tokenValidationService.IssuerValidator,

                    };

                    var tokenValidator = serviceProvider.GetRequiredService<TokenValidator>();
                    options.SecurityTokenValidators.Clear();
                    options.SecurityTokenValidators.Add(tokenValidator);
                });

            services
                .AddAuthorization(options =>
                {
                    options.AddClaimsPolicy(
                        _MicroserviceTemplate_Policies.SETTINGS_READ,
                        _MicroserviceTemplate_Claims.SETTINGS_READ);
                    options.AddClaimsPolicy(
                        _MicroserviceTemplate_Policies.SETTINGS_ADD,
                        _MicroserviceTemplate_Claims.SETTINGS_ADD);
                    options.AddClaimsPolicy(
                        _MicroserviceTemplate_Policies.SETTINGS_UPDATE,
                        _MicroserviceTemplate_Claims.SETTINGS_UPDATE);
                    options.AddClaimsPolicy(
                        _MicroserviceTemplate_Policies.SETTINGS_DELETE,
                        _MicroserviceTemplate_Claims.SETTINGS_DELETE);

                    options.AddPolicy(
                        _MicroserviceTemplate_Policies.JOB_EXECUTE,
                        builder => builder
                            .RequireRole(_MicroserviceTemplate_Roles.INTERNAL_SERVER)
                            .RequireClaim(CommonClaimTypes.CLAIM, _MicroserviceTemplate_Claims.JOB_EXECUTE));

                });
        }

        private static void AddVersioning(IServiceCollection services)
        {
            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });
        }

        private void UseDatabaseMigrations(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<IDbMigrationService>().MigrateAsync().Wait();
            }
        }

        private bool IsInjectDbCredentialsToConnectionString()
        {
            return Configuration.GetValue<bool>("InjectDBCredentialFromEnvironment");
        }
    }
}