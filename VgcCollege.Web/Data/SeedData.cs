using Microsoft.AspNetCore.Identity;
using VgcCollege.Domain.Models;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace VgcCollege.Web.Data;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Create roles
        string[] roles = { "Admin", "Faculty", "Student" };
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // Admin user
        var adminEmail = "admin@vgc.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Faculty user
        var facultyEmail = "faculty@vgc.com";
        IdentityUser? facultyUser;
        if (await userManager.FindByEmailAsync(facultyEmail) == null)
        {
            facultyUser = new IdentityUser { UserName = facultyEmail, Email = facultyEmail, EmailConfirmed = true };
            await userManager.CreateAsync(facultyUser, "Faculty123!");
            await userManager.AddToRoleAsync(facultyUser, "Faculty");
        }
        else
            facultyUser = await userManager.FindByEmailAsync(facultyEmail);

        // Student users
        var student1Email = "student1@vgc.com";
        var student2Email = "student2@vgc.com";
        if (await userManager.FindByEmailAsync(student1Email) == null)
        {
            var s1 = new IdentityUser { UserName = student1Email, Email = student1Email, EmailConfirmed = true };
            await userManager.CreateAsync(s1, "Student123!");
            await userManager.AddToRoleAsync(s1, "Student");
        }
        if (await userManager.FindByEmailAsync(student2Email) == null)
        {
            var s2 = new IdentityUser { UserName = student2Email, Email = student2Email, EmailConfirmed = true };
            await userManager.CreateAsync(s2, "Student123!");
            await userManager.AddToRoleAsync(s2, "Student");
        }

        // Seed branches
        if (!context.Branches.Any())
        {
            var branches = new[]
            {
                new Branch { Name = "Dublin Campus", Address = "1 Main St, Dublin" },
                new Branch { Name = "Cork Campus", Address = "2 South Mall, Cork" },
                new Branch { Name = "Galway Campus", Address = "3 Eyre Square, Galway" }
            };
            await context.Branches.AddRangeAsync(branches);
            await context.SaveChangesAsync();
        }

        // Seed courses
        if (!context.Courses.Any())
        {
            var branches = await context.Branches.ToListAsync();
            var courses = new List<Course>();
            foreach (var branch in branches)
            {
                courses.Add(new Course
                {
                    Name = $"Software Development - {branch.Name}",
                    BranchId = branch.Id,
                    StartDate = DateTime.Today.AddMonths(-3),
                    EndDate = DateTime.Today.AddMonths(3)
                });
                courses.Add(new Course
                {
                    Name = $"Cybersecurity - {branch.Name}",
                    BranchId = branch.Id,
                    StartDate = DateTime.Today.AddMonths(-2),
                    EndDate = DateTime.Today.AddMonths(4)
                });
            }
            await context.Courses.AddRangeAsync(courses);
            await context.SaveChangesAsync();
        }

        // FacultyProfile
        if (!context.FacultyProfiles.Any())
        {
            var facultyProfile = new FacultyProfile
            {
                IdentityUserId = facultyUser?.Id,
                Name = "John Faculty",
                Email = facultyEmail,
                Phone = "0871234567"
            };
            await context.FacultyProfiles.AddAsync(facultyProfile);
            await context.SaveChangesAsync();

            // Assign faculty to all courses
            var allCourses = await context.Courses.ToListAsync();
            foreach (var course in allCourses)
            {
                await context.FacultyCourses.AddAsync(new FacultyCourse
                {
                    FacultyProfileId = facultyProfile.Id,
                    CourseId = course.Id
                });
            }
            await context.SaveChangesAsync();
        }

        // StudentProfiles
        if (!context.StudentProfiles.Any())
        {
            var student1 = await userManager.FindByEmailAsync(student1Email);
            var student2 = await userManager.FindByEmailAsync(student2Email);
            var students = new[]
            {
                new StudentProfile { IdentityUserId = student1!.Id, Name = "Alice Student", Email = student1Email, Phone = "0851112222", Address = "12 College View", StudentNumber = "S001" },
                new StudentProfile { IdentityUserId = student2!.Id, Name = "Bob Student", Email = student2Email, Phone = "0853334444", Address = "22 Campus Road", StudentNumber = "S002" }
            };
            await context.StudentProfiles.AddRangeAsync(students);
            await context.SaveChangesAsync();
        }

        // Enrolments
        if (!context.CourseEnrolments.Any())
        {
            var students = await context.StudentProfiles.ToListAsync();
            var courses = await context.Courses.ToListAsync();
            var enrolments = new List<CourseEnrolment>();
            foreach (var student in students)
            {
                enrolments.Add(new CourseEnrolment
                {
                    StudentProfileId = student.Id,
                    CourseId = courses[0].Id,
                    EnrolDate = DateTime.Today.AddMonths(-2),
                    Status = "Active"
                });
                enrolments.Add(new CourseEnrolment
                {
                    StudentProfileId = student.Id,
                    CourseId = courses[1].Id,
                    EnrolDate = DateTime.Today.AddMonths(-1),
                    Status = "Active"
                });
            }
            await context.CourseEnrolments.AddRangeAsync(enrolments);
            await context.SaveChangesAsync();
        }

        // Attendance (sample)
        if (!context.AttendanceRecords.Any())
        {
            var enrolments = await context.CourseEnrolments.ToListAsync();
            var faker = new Faker();
            var records = new List<AttendanceRecord>();
            foreach (var enrolment in enrolments)
            {
                for (int week = 1; week <= 8; week++)
                {
                    records.Add(new AttendanceRecord
                    {
                        CourseEnrolmentId = enrolment.Id,
                        WeekNumber = week,
                        Present = faker.Random.Bool(0.8f)
                    });
                }
            }
            await context.AttendanceRecords.AddRangeAsync(records);
            await context.SaveChangesAsync();
        }

        // Assignments and results (simplified)
        if (!context.Assignments.Any())
        {
            var courses = await context.Courses.ToListAsync();
            var students = await context.StudentProfiles.ToListAsync();
            var faker = new Faker();
            foreach (var course in courses)
            {
                var assignment = new Assignment
                {
                    CourseId = course.Id,
                    Title = "Assignment 1",
                    MaxScore = 100,
                    DueDate = DateTime.Today.AddDays(14)
                };
                await context.Assignments.AddAsync(assignment);
                await context.SaveChangesAsync();

                foreach (var student in students)
                {
                    await context.AssignmentResults.AddAsync(new AssignmentResult
                    {
                        AssignmentId = assignment.Id,
                        StudentProfileId = student.Id,
                        Score = faker.Random.Int(50, 100),
                        Feedback = faker.Lorem.Sentence()
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        // Exams and results
        if (!context.Exams.Any())
        {
            var courses = await context.Courses.ToListAsync();
            var students = await context.StudentProfiles.ToListAsync();
            var faker = new Faker();
            foreach (var course in courses)
            {
                var exam = new Exam
                {
                    CourseId = course.Id,
                    Title = "Final Exam",
                    Date = DateTime.Today.AddDays(30),
                    MaxScore = 100,
                    ResultsReleased = false
                };
                await context.Exams.AddAsync(exam);
                await context.SaveChangesAsync();

                foreach (var student in students)
                {
                    await context.ExamResults.AddAsync(new ExamResult
                    {
                        ExamId = exam.Id,
                        StudentProfileId = student.Id,
                        Score = faker.Random.Int(40, 95),
                        Grade = faker.PickRandom("A", "B", "C", "D", "F")
                    });
                }
            }
            await context.SaveChangesAsync();
        }
    }
}