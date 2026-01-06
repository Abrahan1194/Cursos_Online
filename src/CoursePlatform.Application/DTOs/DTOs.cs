using System;
using System.Collections.Generic;

namespace CoursePlatform.Application.DTOs
{
    public record CourseSummaryDto(Guid Id, string Title, string Status, int TotalActiveLessons, DateTime LastModified, string AuthorId);
    public record CreateLessonDto(Guid CourseId, string Title, string? Content);
    public record LessonDto(Guid Id, string Title, string Content, int Order);
    public record CourseDetailDto(Guid Id, string Title, string Status, List<LessonDto> Lessons, string AuthorId);
    public record CreateCourseDto(string Title);
    public record UpdateLessonDto(string Title, string? Content);
    public record ReorderLessonsDto(Guid CourseId, List<Guid> LessonIds);
}
