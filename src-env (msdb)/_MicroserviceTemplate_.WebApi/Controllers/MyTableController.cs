using _MicroserviceTemplate_.Domain.MyTables;
using _MicroserviceTemplate_.WebApi.Authorization;
using _MicroserviceTemplate_.WebApi.Models.MyTables;
using AutoMapper;
using Benraz.Infrastructure.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _MicroserviceTemplate_.WebApi.Controllers
{
    /// <summary>
    /// My table controller.
    /// </summary>
    [ApiController]
    [Route("/v{version:ApiVersion}/[controller]")]
    public class MyTableController : ControllerBase
    {
        private readonly IMyTablesRepository _myTablesRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Creates controller.
        /// </summary>
        /// <param name="myTablesRepository">My tables repository.</param>
        /// <param name="mapper">Mapper.</param>
        public MyTableController(IMyTablesRepository myTablesRepository, IMapper mapper)
        {
            _myTablesRepository = myTablesRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns my table entry.
        /// </summary>
        /// <param name="id">My table identifier.</param>
        /// <returns>my table entry.</returns>
        [HttpGet("{id}")]
        [Authorize(_MicroserviceTemplate_Policies.MYTABLES_READ)]
        [ProducesResponseType(typeof(MyTableViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync([FromRoute] string id)
        {
            var myTableEntry = await _myTablesRepository.GetByIdAsync(id);
            if (myTableEntry == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<MyTableViewModel>(myTableEntry);
            return Ok(viewModel);
        }

        /// <summary>
        /// Returns my table entries.
        /// </summary>
        /// <returns>My table entries.</returns>
        [HttpGet]
        [Authorize(_MicroserviceTemplate_Policies.MYTABLES_READ)]
        [ProducesResponseType(typeof(IEnumerable<MyTableViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAsync()
        {
            var myTableEntries = await _myTablesRepository.GetAllAsync();
            var viewModels = _mapper.Map<IEnumerable<MyTableViewModel>>(myTableEntries);

            return Ok(viewModels);
        }

        /// <summary>
        /// Adds new my table entry.
        /// </summary>
        /// <param name="viewModel">My table entry.</param>
        /// <returns>My table identifier.</returns>
        [HttpPost]
        [Authorize(_MicroserviceTemplate_Policies.MYTABLES_ADD)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ServiceFilter(typeof(DRFilterAttribute))]
        public async Task<IActionResult> AddAsync([FromBody] AddMyTableViewModel viewModel)
        {
            if (viewModel == null)
            {
                return BadRequest();
            }

            var dbMyTableEntry = await _myTablesRepository.GetByIdAsync(viewModel.Id);
            if (dbMyTableEntry != null)
            {
                return BadRequest("My table entry with the same key already exists.");
            }

            var myTableEntry = _mapper.Map<MyTable>(viewModel);
            await _myTablesRepository.AddAsync(myTableEntry);

            return Ok(myTableEntry.Id);
        }

        /// <summary>
        /// Changes my table entry.
        /// </summary>
        /// <param name="id">My table identifier.</param>
        /// <param name="viewModel">My table entry.</param>
        /// <returns>Action result.</returns>
        [HttpPut("{id}")]
        [Authorize(_MicroserviceTemplate_Policies.MYTABLES_UPDATE)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ServiceFilter(typeof(DRFilterAttribute))]
        public async Task<IActionResult> ChangeAsync(
            [FromRoute] string id, [FromBody] ChangeMyTableViewModel viewModel)
        {
            if (viewModel == null)
            {
                return BadRequest();
            }

            var myTableEntry = await _myTablesRepository.GetByIdAsync(id);
            if (myTableEntry == null)
            {
                return NotFound();
            }

            _mapper.Map(viewModel, myTableEntry);
            await _myTablesRepository.ChangeAsync(myTableEntry);

            return NoContent();
        }

        /// <summary>
        /// Deletes my table entry.
        /// </summary>
        /// <param name="id">My table identifier.</param>
        /// <returns>Action result.</returns>
        [HttpDelete("{id}")]
        [Authorize(_MicroserviceTemplate_Policies.MYTABLES_DELETE)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ServiceFilter(typeof(DRFilterAttribute))]
        public async Task<IActionResult> DeleteAsync([FromRoute] string id)
        {
            var myTableEntry = await _myTablesRepository.GetByIdAsync(id);
            if (myTableEntry == null)
            {
                return NotFound();
            }

            await _myTablesRepository.RemoveAsync(myTableEntry);

            return NoContent();
        }
    }
}
