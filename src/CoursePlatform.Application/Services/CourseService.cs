using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.DTOs;
using CoursePlatform.Domain.Entities;
using System.Collections.Generic;

namespace CoursePlatform.Application.Services
{
    public class CourseService
    {
        private readonly IApplicationDbContext _context;

        public CourseService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateCourseAsync(string title, string authorId, CancellationToken ct)
        {
            var course = new Course 
            { 
                Title = title, 
                AuthorId = authorId,
                Status = CourseStatus.Draft 
            };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync(ct);
            return course.Id;
        }

        public async Task UpdateCourseAsync(Guid id, string title, string userId, bool isAdmin, CancellationToken ct)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (course == null) throw new Exception("Course not found");

            if (!isAdmin && course.AuthorId != userId)
            {
                throw new UnauthorizedAccessException("Not authorized to update this course.");
            }

            course.Title = title;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        public async Task PublishCourseAsync(Guid id, string userId, bool isAdmin, CancellationToken ct)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (course == null) throw new Exception("Course not found");

            if (!isAdmin && course.AuthorId != userId)
            {
                throw new UnauthorizedAccessException("Not authorized to publish this course.");
            }

            // Rule: At least one active lesson
            if (!course.Lessons.Any(l => !l.IsDeleted))
            {
                throw new BusinessRuleException("Cannot publish a course with 0 active lessons.");
            }

            course.Status = CourseStatus.Published;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        public async Task UnpublishCourseAsync(Guid id, string userId, bool isAdmin, CancellationToken ct)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (course == null) throw new Exception("Course not found");

            if (!isAdmin && course.AuthorId != userId)
            {
                throw new UnauthorizedAccessException("Not authorized to unpublish this course.");
            }

            course.Status = CourseStatus.Draft;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteCourseAsync(Guid id, string userId, bool isAdmin, CancellationToken ct)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (course == null) throw new Exception("Course not found");

            if (!isAdmin && course.AuthorId != userId)
            {
                throw new UnauthorizedAccessException("Not authorized to delete this course.");
            }

            course.IsDeleted = true;
            course.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        public async Task<CourseSummaryDto?> GetSummaryAsync(Guid id, CancellationToken ct)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id, ct);
             
            if (course == null) return null;

            var lastModified = course.UpdatedAt ?? course.CreatedAt;
            if (course.Lessons.Any())
            {
                var maxLessonUpdate = course.Lessons.Max(l => l.UpdatedAt ?? l.CreatedAt);
                if (maxLessonUpdate > lastModified)
                {
                    lastModified = maxLessonUpdate;
                }
            }

            return new CourseSummaryDto(
                course.Id,
                course.Title,
                course.Status.ToString(),
                course.Lessons.Count(l => !l.IsDeleted),
                lastModified,
                course.AuthorId
            );
        }

        public async Task<List<CourseSummaryDto>> SearchCoursesAsync(string? query, string? status, int page, int pageSize, CancellationToken ct)
        {
            var q = _context.Courses.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                q = q.Where(c => c.Title.Contains(query));
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<CourseStatus>(status, true, out var statusEnum))
            {
                q = q.Where(c => c.Status == statusEnum);
            }

            // Pagination defaults
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var courses = await q
                .Include(c => c.Lessons)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return courses.Select(c => {
                var lastModified = c.UpdatedAt ?? c.CreatedAt;
                 if (c.Lessons.Any())
                {
                    var maxLessonUpdate = c.Lessons.Max(l => l.UpdatedAt ?? l.CreatedAt);
                    if (maxLessonUpdate > lastModified)
                    {
                        lastModified = maxLessonUpdate;
                    }
                }
                return new CourseSummaryDto(c.Id, c.Title, c.Status.ToString(), c.Lessons.Count(l => !l.IsDeleted), lastModified, c.AuthorId);
            }).ToList();
        }
        
        public async Task<CourseDetailDto?> GetCourseByIdAsync(Guid id, CancellationToken ct)
        {
            var c = await _context.Courses.Include(x => x.Lessons).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c == null) return null;
            
            return new CourseDetailDto(c.Id, c.Title, c.Status.ToString(), 
                c.Lessons.Where(l => !l.IsDeleted).OrderBy(l => l.Order).Select(l => new LessonDto(l.Id, l.Title, l.Content, l.Order)).ToList(), c.AuthorId);
        }
    }
}
