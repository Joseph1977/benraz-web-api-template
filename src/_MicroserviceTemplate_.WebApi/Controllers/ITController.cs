using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace _MicroserviceTemplate_.WebApi.Controllers
{
    /// <summary>
    /// IT controller.
    /// </summary>
    [ApiController]
    public class ITController : ControllerBase
    {
        private readonly ILogger<ITController> _logger;

        /// <summary>
        /// Creates controller.
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ITController(ILogger<ITController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// For IT purposes: this API just returns OK status, to let everybody know that the service is up and running.
        /// </summary>
        /// <returns>Http status OK (200).</returns>
        [HttpGet("isAlive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult IsAlive()
        {
            //_logger.LogInformation("IsAlive called.");
            return Ok("OK");
        }
    }
}