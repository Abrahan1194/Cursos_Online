using System.ComponentModel.DataAnnotations;

namespace CoursePlatform.Application.DTOs
{
    public class UpdateCourseDto
    {
        [Required]
        public string Title { get; set; }
    }
}
