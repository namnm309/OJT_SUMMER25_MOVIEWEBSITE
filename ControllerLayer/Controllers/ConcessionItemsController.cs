using ApplicationLayer.DTO.ConcessionManagement;
using ApplicationLayer.Services.ConcessionManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControllerLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConcessionItemsController : ControllerBase
    {
        private readonly IConcessionItemService _concessionItemService;

        public ConcessionItemsController(IConcessionItemService concessionItemService)
        {
            _concessionItemService = concessionItemService;
        }

        /// <summary>
        /// Lấy danh sách tất cả các concession item
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ConcessionItemDto>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var items = await _concessionItemService.GetAllAsync();
            return Ok(items);
        }

        /// <summary>
        /// Lấy danh sách concession item với phân trang và tìm kiếm
        /// </summary>
        [HttpGet("paginated")]
        [ProducesResponseType(typeof(GetConcessionItemsResponse), 200)]
        public async Task<IActionResult> GetPaginated([FromQuery] GetConcessionItemsRequest request)
        {
            var response = await _concessionItemService.GetPaginatedAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Lấy concession item theo ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ConcessionItemDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _concessionItemService.GetByIdAsync(id);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        /// <summary>
        /// Lấy danh sách concession item đang hoạt động
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(IEnumerable<ConcessionItemDto>), 200)]
        public async Task<IActionResult> GetActive()
        {
            var items = await _concessionItemService.GetActiveItemsAsync();
            return Ok(items);
        }

        /// <summary>
        /// Tạo mới concession item
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")] // Tùy chỉnh theo yêu cầu phân quyền của bạn
        [ProducesResponseType(typeof(ConcessionItemDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateConcessionItemDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdItem = await _concessionItemService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = createdItem.Id }, createdItem);
        }

        /// <summary>
        /// Cập nhật concession item
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Tùy chỉnh theo yêu cầu phân quyền của bạn
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateConcessionItemDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _concessionItemService.UpdateAsync(id, updateDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Xóa concession item
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Tùy chỉnh theo yêu cầu phân quyền của bạn
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _concessionItemService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
