using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoursePlatform.Application.Services;
using CoursePlatform.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace CoursePlatform.API.Controllers
{
    [Route("api/lessons")]
    [ApiController]
    [Authorize]
    public class LessonsController : ControllerBase
    {
        private readonly LessonService _service;

        public LessonsController(LessonService service)
        {
            _service = service;
        }

        /// <summary>
        /// Creates a new lesson
        /// </summary>
        /// <param name="dto">Lesson creation details</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>Created lesson ID</returns>
        /// <response code="200">Lesson created successfully</response>
        /// <response code="400">Invalid data</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateLessonDto dto, CancellationToken ct)
        {
            try 
            {
                var id = await _service.CreateLessonAsync(dto, ct);
                return Ok(new { id });
            }
            catch (Exception ex) 
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a lesson (Soft Delete)
        /// </summary>
        /// <param name="id">Lesson ID</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>No Content</returns>
        /// <response code="204">Lesson deleted successfully</response>
        /// <response code="404">Lesson not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                await _service.DeleteLessonAsync(id, ct);
                return NoContent();
            }
             catch(Exception)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Updates a lesson
        /// </summary>
        /// <param name="id">Lesson ID</param>
        /// <param name="dto">Update details</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>No Content</returns>
        /// <response code="204">Lesson updated successfully</response>
        /// <response code="404">Lesson not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonDto dto, CancellationToken ct)
        {
            try
            {
                await _service.UpdateLessonAsync(id, dto.Title, dto.Content, ct);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Reorders lessons within a course
        /// </summary>
        /// <param name="dto">Reorder details</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>No Content</returns>
        /// <response code="204">Lessons reordered successfully</response>
        /// <response code="400">Invalid data</response>
        [HttpPost("reorder")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reorder([FromBody] ReorderLessonsDto dto, CancellationToken ct)
        {
            try
            {
                await _service.ReorderLessonsAsync(dto.CourseId, dto.LessonIds, ct);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
