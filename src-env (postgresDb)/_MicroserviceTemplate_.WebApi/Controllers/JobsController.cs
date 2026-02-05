using _MicroserviceTemplate_.Domain.Jobs;
using _MicroserviceTemplate_.WebApi.Authorization;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Benraz.Infrastructure.Common.BackgroundQueue;
using Benraz.Infrastructure.Common.DataRedundancy;
using Benraz.Infrastructure.Common.RepeatableJobs;
using System.Threading.Tasks;

namespace _MicroserviceTemplate_.WebApi.Controllers
{
    /// <summary>
    /// Jobs controller.
    /// </summary>
    [ApiController]
    [Route("/v{version:ApiVersion}/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IDrChecker _drChecker;
        private readonly ILogger<JobsController> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Creates controller.
        /// </summary>
        /// <param name="backgroundTaskQueue">Background queue.</param>
        /// <param name="serviceScopeFactory">Service factory.</param>
        /// <param name="drChecker">DR checker.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="mapper">Mapper.</param>
        public JobsController(
            IBackgroundTaskQueue backgroundTaskQueue,
            IServiceScopeFactory serviceScopeFactory,
            IDrChecker drChecker,
            ILogger<JobsController> logger,
            IMapper mapper)
        {
            _backgroundTaskQueue = backgroundTaskQueue;
            _serviceScopeFactory = serviceScopeFactory;
            _drChecker = drChecker;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Starts a job.
        /// </summary>
        /// <returns>Result.</returns>
        [HttpPost("execute")]
        [Authorize(_MicroserviceTemplate_Policies.JOB_EXECUTE)]
        public Task<IActionResult> ExecuteAsync()
        {
            return ProcessAsync<IEmptyRepeatableJobsService>(new EmptyRepeatableJobsService(), null);
        }

        private async Task<IActionResult> ProcessAsync<TService>(TService service, object args)
            where TService : IRepeatableJobService
        {
            if (!_drChecker.IsActiveDR())
            {
                var message = "DR is inactive. Operation rejected.";
                _logger.LogWarning(message);
                return BadRequest(message);
            }

            await service.CleanUpStaleAsync(args);

            if (await service.IsInProcessingAsync(args))
            {
                return BadRequest("Already in progress.");
            }

            if (await service.IsOnCooldownAsync(args))
            {
                return BadRequest("Not enough cooldown period.");
            }

            _logger.LogInformation("Job execution started.");
            _backgroundTaskQueue.QueueBackgroundWorkItem(async cancellationToken =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    await scope.ServiceProvider.GetService<TService>().ProcessAsync(args);
                }
            });

            return Ok();
        }
    }
}