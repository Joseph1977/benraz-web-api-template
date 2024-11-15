using _MicroserviceTemplate_.WebApi.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Common;
using NLog.Web;
using System;
using System.IO;
using System.Reflection;

namespace _MicroserviceTemplate_.WebApi
{
    /// <summary>
    /// Program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point.
        /// </summary>
        /// <param name="args">Arguments.</param>
        public static void Main(string[] args)
        {
            Logger logger = null;
            try
            {
                var environment = Environment.GetEnvironmentVariable("AspNetCore_Environment");
                var config = new ConfigurationBuilder()
                    .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                    .AddEnvironmentVariables()
                    .Add(new CustomEnvironmentVariableConfigurationSource())
                    .Build();

                logger = InitLogger(config);

                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger?.Error(ex, "Fatal error occured, stopping application...");
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        /// <summary>
        /// Creates and returns web host builder instance.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>Web host builder.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables();
                    builder.Add(new CustomEnvironmentVariableConfigurationSource());
                })
                .UseStartup<Startup>()
                .ConfigureLogging(logging => logging.ClearProviders())
                .UseNLog(new NLogAspNetCoreOptions
                {
                    CaptureMessageProperties = true,
                    CaptureMessageTemplates = true
                });
        }

        private static Logger InitLogger(IConfigurationRoot config)
        {
            var logFolder = config.GetValue<string>("General:LogFolder");
            InternalLogger.LogFile = Path.Combine(logFolder, "nlog-internal.log");

            var logFactory = NLog.LogManager.Setup()
                .LoadConfigurationFromFile("nlog.config") // Load configuration from nlog.config file
                .LoadConfiguration(configBuilder =>
                {
                    var config = configBuilder.Configuration;
                    config.Variables["logFolder"] = logFolder;
                })
                .GetCurrentClassLogger();

            logFactory.Info("Starting application...");

            return logFactory;
        }
    }
}