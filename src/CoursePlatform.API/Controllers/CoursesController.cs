using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoursePlatform.Application.Services;
using CoursePlatform.Application.DTOs;
using CoursePlatform.Application.Common.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoursePlatform.API.Controllers
{
    [Route("api/courses")]
    [ApiController]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly CourseService _service;

        public CoursesController(CourseService service)
        {
            _service = service;
        }

        /// <summary>
        /// Creates a new course
        /// </summary>
        /// <param name="dto">Course creation details</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>Created course ID</returns>
        /// <response code="201">Course created successfully</response>
        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto, CancellationToken ct)
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value 
                         ?? User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var id = await _service.CreateCourseAsync(dto.Title, userId, ct);
            return CreatedAtAction(nameof(GetSummary), new { id }, new { id });
        }

        /// <summary>
        /// Updates a course (e.g. Title)
        /// </summary>
        /// <param name="id">Course ID</param>
        /// <param name="dto">Update details</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>No Content</returns>
        /// <response code="204">Course updated successfully</response>
        /// <response code="404">Course not found</response>
        /// <response code="403">Forbidden</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto, CancellationToken ct)
        {
             try
            {
                var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value 
                             ?? User.FindFirst("sub")?.Value;
                
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var isAdmin = User.IsInRole("Admin");

                await _service.UpdateCourseAsync(id, dto.Title, userId, isAdmin, ct);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message == "Course not found")
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException) 
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Retrieves a course summary by ID
        /// </summary>
        /// <param name="id">Course ID</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>Course summary</returns>
        /// <response code="200">Returns the course summary</response>
        /// <response code="404">Course not found</response>
        [HttpGet("{id}/summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSummary(Guid id, CancellationToken ct)
        {
            var result = await _service.GetSummaryAsync(id, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }
        
        /// <summary>
        /// Retrieves detailed course information by ID
        /// </summary>
        /// <param name="id">Course ID</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>Detailed course information</returns>
        /// <response code="200">Returns the course details</response>
        /// <response code="404">Course not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
             var result = await _service.GetCourseByIdAsync(id, ct);
             if (result == null) return NotFound();
             return Ok(result);
        }

        /// <summary>
        /// Publishes a course
        /// </summary>
        /// <param name="id">Course ID</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>No Content</returns>
        /// <response code="204">Course published successfully</response>
        /// <response code="400">Business rule violation (e.g., no active lessons)</response>
        /// <response code="404">Course not found</response>
        [HttpPatch("{id}/publish")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
        {
            try
            {
                var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value 
                             ?? User.FindFirst("sub")?.Value;
                
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var isAdmin = User.IsInRole("Admin");

                await _service.PublishCourseAsync(id, userId, isAdmin, ct);
                return NoContent();
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex) when (ex.Message == "Course not found")
            {
                return NotFound();
            }
             catch (UnauthorizedAccessException) 
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Unpublishes a course
        /// </summary>
        /// <param name="id">Course ID</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>No Content</returns>
        /// <response code="204">Course unpublished successfully</response>
        /// <response code="404">Course not found</response>
        /// <response code="403">Forbidden</response>
        [HttpPatch("{id}/unpublish")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Unpublish(Guid id, CancellationToken ct)
        {
            try
            {
                var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value 
                             ?? User.FindFirst("sub")?.Value;
                
                if (string.IsNullOrEmpty(userId)) return Unauthorized();
                
                var isAdmin = User.IsInRole("Admin");

                await _service.UnpublishCourseAsync(id, userId, isAdmin, ct);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message == "Course not found")
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException) 
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Deletes a course (Soft Delete) - Admin Only
        /// </summary>
        /// <param name="id">Course ID</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>No Content</returns>
        /// <response code="204">Course deleted successfully</response>
        /// <response code="404">Course not found</response>
        /// <response code="403">Forbidden (Non-Admin)</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
             try
            {
                var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value 
                             ?? User.FindFirst("sub")?.Value;
                
                if (string.IsNullOrEmpty(userId)) return Unauthorized(); // Should not happen for Admin but good practice to allow logic

                var isAdmin = User.IsInRole("Admin");

                await _service.DeleteCourseAsync(id, userId, isAdmin, ct);
                return NoContent();
            }
            catch (Exception ex) when (ex.Message == "Course not found")
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException) 
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Searches for courses with pagination and filtering
        /// </summary>
        /// <param name="q">Search query for title</param>
        /// <param name="status">Filter by status (Draft/Published)</param>
        /// <param name="page">Page number (default 1)</param>
        /// <param name="pageSize">Page size (default 10)</param>
        /// <param name="ct">Cancellation Token</param>
        /// <returns>Paginated list of courses</returns>
        /// <response code="200">Returns list of courses</response>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string? q = null, [FromQuery] string? status = null, [FromQuery] int? page = null, [FromQuery] int? pageSize = null, CancellationToken ct = default)
        {
            var result = await _service.SearchCoursesAsync(q, status, page ?? 1, pageSize ?? 10, ct);
            return Ok(result);
        }
    }
}
