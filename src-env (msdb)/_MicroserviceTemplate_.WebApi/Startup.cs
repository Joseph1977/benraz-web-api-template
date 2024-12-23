using _MicroserviceTemplate_.Domain.Authorization;
using _MicroserviceTemplate_.Domain.Jobs;
using _MicroserviceTemplate_.Domain.MyTables;
using _MicroserviceTemplate_.EF;
using _MicroserviceTemplate_.EF.Repositories;
using _MicroserviceTemplate_.EF.Services;
using _MicroserviceTemplate_.WebApi.Authorization;
using _MicroserviceTemplate_.WebApi.Controllers;
using _MicroserviceTemplate_.WebApi.Extensions;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Benraz.Infrastructure.Authorization.Tokens;
using Benraz.Infrastructure.Common.AccessControl;
using Benraz.Infrastructure.Common.BackgroundQueue;
using Benraz.Infrastructure.Common.DataRedundancy;
using Benraz.Infrastructure.Gateways.BenrazAuthorization.Auth;
using Benraz.Infrastructure.Web.Authorization;
using Benraz.Infrastructure.Web.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
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
                Console.WriteLine($"We are running in debug: UseDeveloperExceptionPage");
            }
            else
            {
                app.UseHsts();
                var enforceHttps = Configuration.GetValue<bool>("EnforceHttps", true);
                Console.WriteLine($"Enforce HTTPS: {enforceHttps}");

                if (enforceHttps)
                {
                    app.UseHttpsRedirection();
                }
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
            if (IsCheckConnectionStringExists())
            {
                string connectionString = Configuration.GetValue<string>("ConnectionStrings");
                if (IsInjectDbCredentialsToConnectionString())
                {
                    connectionString +=
                        $";User Id={Configuration.GetValue<string>("AspNetCoreDbUserName")};Password={Configuration.GetValue<string>("AspNetCoreDbPassword")}";
                }

                services.AddDbContext<_MicroserviceTemplate_DbContext>(options =>
                    options.UseSqlServer(
                        connectionString,
                        o => o.EnableRetryOnFailure(3)
                    ));
            }
            else
                services.AddDbContext<_MicroserviceTemplate_DbContext>();
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IDrChecker, DrChecker>();
            services.AddScoped<DRFilterAttribute>();
            services.AddScoped<ErrorFilterAttribute>();

            services.AddTransient<IDbMigrationService, _MicroserviceTemplate_DbMigrationService>();

            services.AddTransient<IMyTablesRepository, MyTablesRepository>();

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

                    // Switch to using TokenHandlers
                    options.TokenHandlers.Clear();  // Clear existing handlers if any
                    options.TokenHandlers.Add(new JwtSecurityTokenHandler());  // Add JwtSecurityTokenHandler for handling JWTs

                    // Configure the TokenValidationParameters to specify how tokens should be validated
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew = TimeSpan.FromMinutes(5),
                        IssuerSigningKeyResolver = tokenValidationService.IssuerSigningKeyResolver,
                        ValidateAudience = true,
                        AudienceValidator = tokenValidationService.AudienceValidator,
                        ValidateIssuer = true,
                        IssuerValidator = tokenValidationService.IssuerValidator,

                        // Include additional validation settings as needed
                    };

                    options.Audience = Configuration["Jwt:Audience"];
                    options.Authority = Configuration["Jwt:Authority"];

                    var enforceHttps = Configuration.GetValue<bool>("EnforceHttps", true);
                    options.RequireHttpsMetadata = enforceHttps;
                });

            services
                .AddAuthorization(options =>
                {
                    options.AddClaimsPolicy(
                        _MicroserviceTemplate_Policies.MYTABLES_READ,
                        _MicroserviceTemplate_Claims.MYTABLES_READ);
                    options.AddClaimsPolicy(
                        _MicroserviceTemplate_Policies.MYTABLES_ADD,
                        _MicroserviceTemplate_Claims.MYTABLES_ADD);
                    options.AddClaimsPolicy(
                        _MicroserviceTemplate_Policies.MYTABLES_UPDATE,
                        _MicroserviceTemplate_Claims.MYTABLES_UPDATE);
                    options.AddClaimsPolicy(
                        _MicroserviceTemplate_Policies.MYTABLES_DELETE,
                        _MicroserviceTemplate_Claims.MYTABLES_DELETE);

                    options.AddPolicy(
                        _MicroserviceTemplate_Policies.JOB_EXECUTE,
                        builder => builder
                            .RequireRole(_MicroserviceTemplate_Roles.INTERNAL_SERVER)
                            .RequireClaim(CommonClaimTypes.CLAIM, _MicroserviceTemplate_Claims.JOB_EXECUTE));

                });
        }

        private static void AddVersioning(IServiceCollection services)
        {
            // Add API versioning          
            var apiVersioningBuilder = services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                // Use whatever reader you want
                //options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                new HeaderApiVersionReader("x-api-version"),
                                                new MediaTypeApiVersionReader("x-api-version"));
            }); // Nuget Package: Asp.Versioning.Mvc

            // Add versioned API explorer
            apiVersioningBuilder.AddApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            }); // Nuget Package: Asp.Versioning.Mvc.ApiExplorer

        }

        private void UseDatabaseMigrations(IApplicationBuilder app)
        {
            if (IsCheckConnectionStringExists())
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    scope.ServiceProvider.GetRequiredService<IDbMigrationService>().MigrateAsync().Wait();
                }
            }  
        }

        private bool IsInjectDbCredentialsToConnectionString()
        {
            return Configuration.GetValue<bool>("InjectDBCredentialFromEnvironment");
        }

        private bool IsCheckConnectionStringExists()
        {
            bool isExists = true;
            string connectionString = Configuration.GetValue<string>("ConnectionStrings");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                isExists = false;
            }
            return isExists;
        }
    }
}