using _MicroserviceTemplate_.Domain.Settings;
using _MicroserviceTemplate_.WebApi.Authorization;
using _MicroserviceTemplate_.WebApi.Models.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Benraz.Infrastructure.Web.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _MicroserviceTemplate_.WebApi.Controllers
{
    /// <summary>
    /// Settings controller.
    /// </summary>
    [ApiController]
    [Route("/v{version:ApiVersion}/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsEntriesRepository _settingsEntriesRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Creates controller.
        /// </summary>
        /// <param name="settingsEntriesRepository">Settings entries repository.</param>
        /// <param name="mapper">Mapper.</param>
        public SettingsController(ISettingsEntriesRepository settingsEntriesRepository, IMapper mapper)
        {
            _settingsEntriesRepository = settingsEntriesRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns settings entry.
        /// </summary>
        /// <param name="id">Settings entry identifier.</param>
        /// <returns>Settings entry.</returns>
        [HttpGet("{id}")]
        [Authorize(_MicroserviceTemplate_Policies.SETTINGS_READ)]
        [ProducesResponseType(typeof(SettingsEntryViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] string id)
        {
            var settingsEntry = await _settingsEntriesRepository.GetByIdAsync(id);
            if (settingsEntry == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<SettingsEntryViewModel>(settingsEntry);
            return Ok(viewModel);
        }

        /// <summary>
        /// Returns settings entries.
        /// </summary>
        /// <returns>Settings entries.</returns>
        [HttpGet]
        [Authorize(_MicroserviceTemplate_Policies.SETTINGS_READ)]
        [ProducesResponseType(typeof(IEnumerable<SettingsEntryViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAsync()
        {
            var settingsEntries = await _settingsEntriesRepository.GetAllAsync();
            var viewModels = _mapper.Map<IEnumerable<SettingsEntryViewModel>>(settingsEntries);

            return Ok(viewModels);
        }

        /// <summary>
        /// Adds new settings entry.
        /// </summary>
        /// <param name="viewModel">Settings entry.</param>
        /// <returns>Settings entry identifier.</returns>
        [HttpPost]
        [Authorize(_MicroserviceTemplate_Policies.SETTINGS_ADD)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ServiceFilter(typeof(DRFilterAttribute))]
        public async Task<IActionResult> AddAsync([FromBody] AddSettingsEntryViewModel viewModel)
        {
            if (viewModel == null)
            {
                return BadRequest();
            }

            var dbSettingsEntry = await _settingsEntriesRepository.GetByIdAsync(viewModel.Id);
            if (dbSettingsEntry != null)
            {
                return BadRequest("Settings entry with the same key already exists.");
            }

            var settingsEntry = _mapper.Map<SettingsEntry>(viewModel);
            await _settingsEntriesRepository.AddAsync(settingsEntry);

            return Ok(settingsEntry.Id);
        }

        /// <summary>
        /// Changes settings entry.
        /// </summary>
        /// <param name="id">Settings entry identifier.</param>
        /// <param name="viewModel">Settings entry.</param>
        /// <returns>Action result.</returns>
        [HttpPut("{id}")]
        [Authorize(_MicroserviceTemplate_Policies.SETTINGS_UPDATE)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ServiceFilter(typeof(DRFilterAttribute))]
        public async Task<IActionResult> ChangeAsync(
            [FromRoute] string id, [FromBody] ChangeSettingsEntryViewModel viewModel)
        {
            if (viewModel == null)
            {
                return BadRequest();
            }

            var settingsEntry = await _settingsEntriesRepository.GetByIdAsync(id);
            if (settingsEntry == null)
            {
                return NotFound();
            }

            _mapper.Map(viewModel, settingsEntry);
            await _settingsEntriesRepository.ChangeAsync(settingsEntry);

            return NoContent();
        }

        /// <summary>
        /// Deletes settings entry.
        /// </summary>
        /// <param name="id">Settings entry identifier.</param>
        /// <returns>Action result.</returns>
        [HttpDelete("{id}")]
        [Authorize(_MicroserviceTemplate_Policies.SETTINGS_DELETE)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ServiceFilter(typeof(DRFilterAttribute))]
        public async Task<IActionResult> DeleteAsync([FromRoute] string id)
        {
            var settingsEntry = await _settingsEntriesRepository.GetByIdAsync(id);
            if (settingsEntry == null)
            {
                return NotFound();
            }

            await _settingsEntriesRepository.RemoveAsync(settingsEntry);

            return NoContent();
        }
    }
}