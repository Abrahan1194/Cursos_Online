using Microsoft.EntityFrameworkCore;
using CoursePlatform.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace CoursePlatform.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Course> Courses { get; }
        DbSet<Lesson> Lessons { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
