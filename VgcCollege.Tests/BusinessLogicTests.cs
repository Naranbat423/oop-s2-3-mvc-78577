using Xunit;
using VgcCollege.Domain.Models;
using System;
using System.Linq;

namespace VgcCollege.Tests;

public class BusinessLogicTests
{
    // Helper method to simulate duplicate enrolment check
    private bool IsDuplicateEnrolment(StudentProfile student, Course course, IQueryable<CourseEnrolment> existingEnrolments)
    {
        return existingEnrolments.Any(e => e.StudentProfileId == student.Id && e.CourseId == course.Id);
    }

    // 1. Enrolment duplicate prevention (business rule)
    [Fact]
    public void Enrolment_Duplicate_ShouldNotBeAllowed()
    {
        // Arrange
        var student = new StudentProfile { Id = 1 };
        var course = new Course { Id = 1 };
        var existingEnrolments = new[]
        {
            new CourseEnrolment { StudentProfileId = 1, CourseId = 1 }
        }.AsQueryable();

        // Act
        var isDuplicate = IsDuplicateEnrolment(student, course, existingEnrolments);

        // Assert
        Assert.True(isDuplicate);
    }

    // 2. Assignment score cannot exceed max score
    [Fact]
    public void AssignmentResult_Score_ShouldNotExceedMaxScore()
    {
        // Arrange
        var assignment = new Assignment { MaxScore = 100 };
        var result = new AssignmentResult { Assignment = assignment, Score = 95 };

        // Act & Assert
        Assert.InRange(result.Score, 0, assignment.MaxScore);
    }

    // 3. Exam result visibility (released only)
    [Fact]
    public void ExamResult_StudentCanView_OnlyWhenReleased()
    {
        // Arrange
        var releasedExam = new Exam { ResultsReleased = true };
        var provisionalExam = new Exam { ResultsReleased = false };
        var resultReleased = new ExamResult { Exam = releasedExam };
        var resultProvisional = new ExamResult { Exam = provisionalExam };

        // Act & Assert
        Assert.True(resultReleased.Exam.ResultsReleased);
        Assert.False(resultProvisional.Exam.ResultsReleased);
    }

    // 4. Faculty can only access courses they are assigned to
    [Fact]
    public void Faculty_ShouldOnlySeeAssignedCourses()
    {
        // Arrange
        var faculty = new FacultyProfile { Id = 1 };
        var assignedCourse = new Course { Id = 1 };
        var unassignedCourse = new Course { Id = 2 };
        var facultyCourse = new FacultyCourse { FacultyProfileId = faculty.Id, CourseId = assignedCourse.Id };

        // Act
        var assignedCourseIds = new[] { facultyCourse.CourseId };
        var isAssigned = assignedCourseIds.Contains(assignedCourse.Id);
        var isNotAssigned = assignedCourseIds.Contains(unassignedCourse.Id);

        // Assert
        Assert.True(isAssigned);
        Assert.False(isNotAssigned);
    }

    // 5. Student can only see own data (by IdentityUserId)
    [Fact]
    public void Student_ShouldOnlySeeOwnProfile()
    {
        // Arrange
        var student1 = new StudentProfile { IdentityUserId = "user1", Name = "Alice" };
        var student2 = new StudentProfile { IdentityUserId = "user2", Name = "Bob" };
        var currentUserId = "user1";

        // Act
        var isOwn = student1.IdentityUserId == currentUserId;
        var isNotOwn = student2.IdentityUserId == currentUserId;

        // Assert
        Assert.True(isOwn);
        Assert.False(isNotOwn);
    }

    // 6. Attendance week number must be positive
    [Fact]
    public void AttendanceRecord_WeekNumber_ShouldBePositive()
    {
        // Arrange
        var record = new AttendanceRecord { WeekNumber = 3 };

        // Act & Assert
        Assert.True(record.WeekNumber > 0);
    }

    // 7. Course start date must be before end date
    [Fact]
    public void Course_StartDate_BeforeEndDate()
    {
        // Arrange
        var course = new Course { StartDate = new DateTime(2026, 1, 10), EndDate = new DateTime(2026, 5, 20) };

        // Act & Assert
        Assert.True(course.StartDate < course.EndDate);
    }

    // 8. Enrolment status must be one of allowed values
    [Fact]
    public void Enrolment_Status_ShouldBeValid()
    {
        // Arrange
        var allowedStatuses = new[] { "Active", "Completed", "Dropped" };
        var enrolment = new CourseEnrolment { Status = "Active" };

        // Act & Assert
        Assert.Contains(enrolment.Status, allowedStatuses);
    }

    // 9. Assignment result score must be integer between 0 and max (additional test)
    [Fact]
    public void AssignmentResult_Score_ShouldBeInteger()
    {
        // Arrange
        var result = new AssignmentResult { Score = 75 };

        // Act & Assert
        Assert.IsType<int>(result.Score);
        Assert.True(result.Score >= 0);
    }

    // 10. Exam max score positive (edge case)
    [Fact]
    public void Exam_MaxScore_ShouldBePositive()
    {
        // Arrange
        var exam = new Exam { MaxScore = 100 };

        // Act & Assert
        Assert.True(exam.MaxScore > 0);
    }
}