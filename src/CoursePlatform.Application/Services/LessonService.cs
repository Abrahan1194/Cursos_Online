using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Application.DTOs;
using CoursePlatform.Domain.Entities;

namespace CoursePlatform.Application.Services
{
    public class LessonService
    {
        private readonly IApplicationDbContext _context;

        public LessonService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateLessonAsync(CreateLessonDto dto, CancellationToken ct)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == dto.CourseId, ct);

            if (course == null) throw new Exception("Course not found");

            // Auto-calculate order
            int nextOrder = 1;
            if (course.Lessons.Any())
            {
                nextOrder = course.Lessons.Max(l => l.Order) + 1;
            }

            var lesson = new Lesson
            {
                Title = dto.Title,
                Content = dto.Content ?? string.Empty,
                CourseId = dto.CourseId,
                Order = nextOrder
            };

            _context.Lessons.Add(lesson);
            
            await _context.SaveChangesAsync(ct);
            return lesson.Id;
        }

        public async Task DeleteLessonAsync(Guid id, CancellationToken ct)
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == id, ct);
            if (lesson == null) throw new Exception("Lesson not found");

            lesson.IsDeleted = true;
            lesson.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            // Check if course has any active lessons left
            var hasActiveLessons = await _context.Lessons
                .AnyAsync(l => l.CourseId == lesson.CourseId && !l.IsDeleted, ct);

            if (!hasActiveLessons)
            {
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == lesson.CourseId, ct);
                if (course != null && course.Status == CourseStatus.Published)
                {
                    course.Status = CourseStatus.Draft;
                    course.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(ct);
                }
            }
        }

        public async Task UpdateLessonAsync(Guid id, string title, string? content, CancellationToken ct)
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == id, ct);
            if (lesson == null) throw new Exception("Lesson not found");

            lesson.Title = title;
            if (content != null) lesson.Content = content; // Only update if not null? Or allow clearing? For now let's say "if not null or if we pass it, we set it".
            // Since DTO has content, let's update it.
            // But wait, UpdateLessonDto signature in DTOs.cs is (string Title, string? Content).
            // Let's assume we update both.
            if (content != null) lesson.Content = content;
            
            lesson.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }

        public async Task ReorderLessonsAsync(Guid courseId, List<Guid> lessonIds, CancellationToken ct)
        {
            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId && !l.IsDeleted)
                .ToListAsync(ct);

            for (int i = 0; i < lessonIds.Count; i++)
            {
                var lesson = lessons.FirstOrDefault(l => l.Id == lessonIds[i]);
                if (lesson != null)
                {
                    lesson.Order = i + 1;
                    lesson.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync(ct);
        }
    }
}
