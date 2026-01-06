using System;
using System.Collections.Generic;
using CoursePlatform.Domain.Common;

namespace CoursePlatform.Domain.Entities
{
    public enum CourseStatus
    {
        Draft,
        Published
    }

    public class Course : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public CourseStatus Status { get; set; } = CourseStatus.Draft;
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
