using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoursePlatform.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.Migrate();
        }

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            // Ensure DB is created if using EnsureCreated for quick start or Migrations
            // Usually Program.cs calls Migrate, so we assume DB exists here.
            
            // HOTFIX: Ensure AuthorId column exists since Migration file might be missing in some environments
            try 
            {
                await context.Database.ExecuteSqlRawAsync("ALTER TABLE \"Courses\" ADD COLUMN IF NOT EXISTS \"AuthorId\" text DEFAULT '';");
            }
            catch (Exception ex)
            {
                // Log or ignore, but allow proceeding if it's just "column exists" error (though IF NOT EXISTS handles that)
                Console.WriteLine($"Warning: Failed to ensure AuthorId column: {ex.Message}");
            }

            string[] roles = { "Admin", "Instructor", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@test.com";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Password123!");
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to seed admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Seed Instructor
            var instructorEmail = "instructor@test.com";
            var instructorUser = await userManager.FindByEmailAsync(instructorEmail);
            if (instructorUser == null)
            {
                instructorUser = new IdentityUser
                {
                    UserName = instructorEmail,
                    Email = instructorEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(instructorUser, "Instructor123!");
            }
            if (!await userManager.IsInRoleAsync(instructorUser, "Instructor"))
            {
                await userManager.AddToRoleAsync(instructorUser, "Instructor");
            }

            // Seed Student (Previously User)
            var userEmail = "student@test.com";
            var normalUser = await userManager.FindByEmailAsync(userEmail);
            if (normalUser == null)
            {
                normalUser = new IdentityUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(normalUser, "User123!");
            }
            if (!await userManager.IsInRoleAsync(normalUser, "Student"))
            {
                await userManager.AddToRoleAsync(normalUser, "Student");
            }

            // Seed Sample Data
            if (!context.Courses.Any())
            {
                // Get Admin User ID for authorship
                var admin = await userManager.FindByEmailAsync("admin@test.com");
                var authorId = admin?.Id ?? Guid.Empty.ToString();

                var courseId = Guid.NewGuid();
                var course = new Domain.Entities.Course
                {
                    Id = courseId,
                    Title = "Curso Introductorio de .NET 9",
                    AuthorId = authorId,
                    Status = Domain.Entities.CourseStatus.Published,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Courses.Add(course);
                
                context.Lessons.AddRange(
                    new Domain.Entities.Lesson { CourseId = courseId, Title = "Introducción a .NET", Order = 1, CreatedAt = DateTime.UtcNow, IsDeleted = false },
                    new Domain.Entities.Lesson { CourseId = courseId, Title = "Configurando el Entorno", Order = 2, CreatedAt = DateTime.UtcNow, IsDeleted = false },
                    new Domain.Entities.Lesson { CourseId = courseId, Title = "Hola Mundo en C#", Order = 3, CreatedAt = DateTime.UtcNow, IsDeleted = false }
                );

                await context.SaveChangesAsync(System.Threading.CancellationToken.None);
            }
        }
    }
}
