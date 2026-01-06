using Microsoft.Extensions.DependencyInjection;
using CoursePlatform.Application.Services;

namespace CoursePlatform.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<CourseService>();
            services.AddScoped<LessonService>();
            return services;
        }
    }
}
