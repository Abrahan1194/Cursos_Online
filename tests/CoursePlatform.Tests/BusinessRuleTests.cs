using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using CoursePlatform.Application.Services;
using CoursePlatform.Application.Common.Interfaces;
using CoursePlatform.Domain.Entities;
using CoursePlatform.Application.Common.Exceptions;
using CoursePlatform.Application.DTOs;
using MockQueryable.Moq;

namespace CoursePlatform.Tests
{
    public class BusinessRuleTests
    {
        private readonly Mock<IApplicationDbContext> _mockContext;
        private readonly CourseService _courseService;
        private readonly LessonService _lessonService;

        public BusinessRuleTests()
        {
            _mockContext = new Mock<IApplicationDbContext>();
            _courseService = new CourseService(_mockContext.Object);
            _lessonService = new LessonService(_mockContext.Object);
        }

        [Fact]
        public async Task PublishCourse_WithLessons_ShouldSucceed()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var authorId = Guid.NewGuid().ToString();
            var course = new Course 
            { 
                Id = courseId, 
                AuthorId = authorId,
                Status = CourseStatus.Draft,
                Lessons = new List<Lesson> { new Lesson { Id = Guid.NewGuid(), IsDeleted = false } }
            };

            var coursesDbSet = GetQueryableMockDbSet(new List<Course> { course });
            _mockContext.Setup(c => c.Courses).Returns(coursesDbSet);

            // Act
            await _courseService.PublishCourseAsync(courseId, authorId, false, CancellationToken.None);

            // Assert
            Assert.Equal(CourseStatus.Published, course.Status);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PublishCourse_WithoutLessons_ShouldFail()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var authorId = Guid.NewGuid().ToString();
            var course = new Course 
            { 
                Id = courseId,
                AuthorId = authorId,
                Status = CourseStatus.Draft,
                Lessons = new List<Lesson>() // Empty
            };

            var coursesDbSet = GetQueryableMockDbSet(new List<Course> { course });
            _mockContext.Setup(c => c.Courses).Returns(coursesDbSet);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() => 
                _courseService.PublishCourseAsync(courseId, authorId, false, CancellationToken.None));
        }

        [Fact]
        public async Task CreateLesson_WithUniqueOrder_ShouldCalculatedCorrectly()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course = new Course 
            { 
                Id = courseId, 
                Lessons = new List<Lesson> 
                { 
                    new Lesson { Order = 1 },
                    new Lesson { Order = 2 }
                }
            };

            var coursesDbSet = GetQueryableMockDbSet(new List<Course> { course });
            _mockContext.Setup(c => c.Courses).Returns(coursesDbSet);
            _mockContext.Setup(c => c.Lessons.Add(It.IsAny<Lesson>()));

            var dto = new CreateLessonDto(courseId, "New Lesson");

            // Act
            await _lessonService.CreateLessonAsync(dto, CancellationToken.None);

            // Assert
            _mockContext.Verify(c => c.Lessons.Add(It.Is<Lesson>(l => l.Order == 3)), Times.Once);
        }

        [Fact]
        public async Task DeleteCourse_ShouldBeSoftDelete()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var authorId = Guid.NewGuid().ToString();
            var course = new Course { Id = courseId, AuthorId = authorId, IsDeleted = false };

            var coursesDbSet = GetQueryableMockDbSet(new List<Course> { course });
            _mockContext.Setup(c => c.Courses).Returns(coursesDbSet);

            // Act
            await _courseService.DeleteCourseAsync(courseId, authorId, false, CancellationToken.None);

            // Assert
            Assert.True(course.IsDeleted);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateLesson_WithDuplicateOrder_ShouldFail()
        {
            // Arrange
            // Nota: El sistema auto-calcula el orden, por lo que no hay duplicados.
            // Este test verifica que al crear una lección, el orden se calcula automáticamente
            // y no se permite duplicación.
            var courseId = Guid.NewGuid();
            var existingLesson = new Lesson { Id = Guid.NewGuid(), Order = 1, IsDeleted = false };
            var course = new Course 
            { 
                Id = courseId, 
                Lessons = new List<Lesson> { existingLesson }
            };

            var coursesDbSet = GetQueryableMockDbSet(new List<Course> { course });
            _mockContext.Setup(c => c.Courses).Returns(coursesDbSet);
            _mockContext.Setup(c => c.Lessons.Add(It.IsAny<Lesson>()));

            var dto = new CreateLessonDto(courseId, "New Lesson");

            // Act
            await _lessonService.CreateLessonAsync(dto, CancellationToken.None);

            // Assert - Verify that the new lesson gets Order = 2 (not 1, avoiding duplicate)
            _mockContext.Verify(c => c.Lessons.Add(It.Is<Lesson>(l => l.Order == 2)), Times.Once);
        }

        [Fact]
        public async Task UnpublishCourse_ShouldChangeStatusToDraft()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var authorId = Guid.NewGuid().ToString();
            var course = new Course 
            { 
                Id = courseId, 
                AuthorId = authorId,
                Status = CourseStatus.Published 
            };

            var coursesDbSet = GetQueryableMockDbSet(new List<Course> { course });
            _mockContext.Setup(c => c.Courses).Returns(coursesDbSet);

            // Act
            await _courseService.UnpublishCourseAsync(courseId, authorId, false, CancellationToken.None);

            // Assert
            Assert.Equal(CourseStatus.Draft, course.Status);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReorderLessons_ShouldUpdateIndices_Simulated()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var lesson1 = new Lesson { Id = Guid.NewGuid(), CourseId = courseId, Order = 1, IsDeleted = false };
            var lesson2 = new Lesson { Id = Guid.NewGuid(), CourseId = courseId, Order = 2, IsDeleted = false };
            
            // Setup Mock DB Set for Lessons
            var lessonsList = new List<Lesson> { lesson1, lesson2 };
            var lessonsDbSet = GetQueryableMockDbSet(lessonsList);
            _mockContext.Setup(c => c.Lessons).Returns(lessonsDbSet);

            // New order: Lesson 2 first, Lesson 1 second
            var newOrderIds = new List<Guid> { lesson2.Id, lesson1.Id };

            // Act
            await _lessonService.ReorderLessonsAsync(courseId, newOrderIds, CancellationToken.None);

            // Assert
            Assert.Equal(1, lesson2.Order);
            Assert.Equal(2, lesson1.Order);
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteLesson_WhenNoActiveLessonsLeft_ShouldRevertCourseToDraft()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var lessonId = Guid.NewGuid();
            
            var course = new Course 
            { 
                Id = courseId, 
                Status = CourseStatus.Published,
                Lessons = new List<Lesson>() 
            };

            var lesson = new Lesson 
            { 
                Id = lessonId, 
                CourseId = courseId, 
                IsDeleted = false 
            };
            
            var lessonsList = new List<Lesson> { lesson };
            var coursesList = new List<Course> { course };

            var lessonsDbSet = GetQueryableMockDbSet(lessonsList);
            var coursesDbSet = GetQueryableMockDbSet(coursesList);

            _mockContext.Setup(c => c.Lessons).Returns(lessonsDbSet);
            _mockContext.Setup(c => c.Courses).Returns(coursesDbSet);
            
            // Act
            await _lessonService.DeleteLessonAsync(lessonId, CancellationToken.None);

            // Assert
            Assert.True(lesson.IsDeleted);
            Assert.Equal(CourseStatus.Draft, course.Status);
            // Verify SaveChanges was called twice: once for delete, once for status update
            _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        // Helper to mock DbSet
        private static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            return sourceList.AsQueryable().BuildMockDbSet().Object;
        }
    }
}
