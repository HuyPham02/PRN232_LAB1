using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Data;

public class LmsDbContext : DbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options) { }

    public DbSet<Semester> Semesters { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Semester)
            .WithMany(s => s.Courses)
            .HasForeignKey(c => c.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Subject)
            .WithMany(s => s.Courses)
            .HasForeignKey(c => c.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint on student email
        modelBuilder.Entity<Student>()
            .HasIndex(s => s.Email)
            .IsUnique();

        modelBuilder.Entity<Subject>()
            .HasIndex(s => s.SubjectCode)
            .IsUnique();

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // 5 Semesters
        var semesters = new List<Semester>
        {
            new() { SemesterId = 1, SemesterName = "Fall 2023", StartDate = new DateTime(2023, 9, 1), EndDate = new DateTime(2024, 1, 15) },
            new() { SemesterId = 2, SemesterName = "Spring 2024", StartDate = new DateTime(2024, 2, 1), EndDate = new DateTime(2024, 6, 15) },
            new() { SemesterId = 3, SemesterName = "Summer 2024", StartDate = new DateTime(2024, 7, 1), EndDate = new DateTime(2024, 8, 31) },
            new() { SemesterId = 4, SemesterName = "Fall 2024", StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2025, 1, 15) },
            new() { SemesterId = 5, SemesterName = "Spring 2025", StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 6, 15) },
        };
        modelBuilder.Entity<Semester>().HasData(semesters);

        // 10 Subjects
        var subjects = new List<Subject>
        {
            new() { SubjectId = 1, SubjectCode = "PRN211", SubjectName = "Basic Cross-Platform Application Programming With .NET", Credit = 3 },
            new() { SubjectId = 2, SubjectCode = "PRN221", SubjectName = "Advanced Cross-Platform Application Programming With .NET", Credit = 3 },
            new() { SubjectId = 3, SubjectCode = "PRN231", SubjectName = "Building Cross-Platform Back-End Application With .NET", Credit = 3 },
            new() { SubjectId = 4, SubjectCode = "PRN232", SubjectName = "Building Cross-Platform Back-End Application With .NET 2", Credit = 3 },
            new() { SubjectId = 5, SubjectCode = "DBD501", SubjectName = "Database Design", Credit = 3 },
            new() { SubjectId = 6, SubjectCode = "SWR302", SubjectName = "Software Requirement", Credit = 3 },
            new() { SubjectId = 7, SubjectCode = "SWD392", SubjectName = "Software Architecture and Design", Credit = 3 },
            new() { SubjectId = 8, SubjectCode = "MAS291", SubjectName = "Mathematics and Statistics", Credit = 3 },
            new() { SubjectId = 9, SubjectCode = "IOT102", SubjectName = "Internet of Things", Credit = 3 },
            new() { SubjectId = 10, SubjectCode = "WED201c", SubjectName = "Web Design", Credit = 3 },
        };
        modelBuilder.Entity<Subject>().HasData(subjects);

        // 20 Courses (spread across semesters and subjects)
        var courses = new List<Course>
        {
            new() { CourseId = 1, CourseName = "PRN211 - Fall 2023 - Section 1", SemesterId = 1, SubjectId = 1 },
            new() { CourseId = 2, CourseName = "PRN221 - Fall 2023 - Section 1", SemesterId = 1, SubjectId = 2 },
            new() { CourseId = 3, CourseName = "DBD501 - Fall 2023 - Section 1", SemesterId = 1, SubjectId = 5 },
            new() { CourseId = 4, CourseName = "SWR302 - Fall 2023 - Section 1", SemesterId = 1, SubjectId = 6 },
            new() { CourseId = 5, CourseName = "PRN231 - Spring 2024 - Section 1", SemesterId = 2, SubjectId = 3 },
            new() { CourseId = 6, CourseName = "PRN232 - Spring 2024 - Section 1", SemesterId = 2, SubjectId = 4 },
            new() { CourseId = 7, CourseName = "SWD392 - Spring 2024 - Section 1", SemesterId = 2, SubjectId = 7 },
            new() { CourseId = 8, CourseName = "MAS291 - Spring 2024 - Section 1", SemesterId = 2, SubjectId = 8 },
            new() { CourseId = 9, CourseName = "IOT102 - Summer 2024 - Section 1", SemesterId = 3, SubjectId = 9 },
            new() { CourseId = 10, CourseName = "WED201c - Summer 2024 - Section 1", SemesterId = 3, SubjectId = 10 },
            new() { CourseId = 11, CourseName = "PRN211 - Summer 2024 - Section 1", SemesterId = 3, SubjectId = 1 },
            new() { CourseId = 12, CourseName = "PRN211 - Fall 2024 - Section 1", SemesterId = 4, SubjectId = 1 },
            new() { CourseId = 13, CourseName = "PRN221 - Fall 2024 - Section 1", SemesterId = 4, SubjectId = 2 },
            new() { CourseId = 14, CourseName = "PRN231 - Fall 2024 - Section 1", SemesterId = 4, SubjectId = 3 },
            new() { CourseId = 15, CourseName = "DBD501 - Fall 2024 - Section 1", SemesterId = 4, SubjectId = 5 },
            new() { CourseId = 16, CourseName = "SWR302 - Fall 2024 - Section 1", SemesterId = 4, SubjectId = 6 },
            new() { CourseId = 17, CourseName = "PRN232 - Spring 2025 - Section 1", SemesterId = 5, SubjectId = 4 },
            new() { CourseId = 18, CourseName = "SWD392 - Spring 2025 - Section 1", SemesterId = 5, SubjectId = 7 },
            new() { CourseId = 19, CourseName = "MAS291 - Spring 2025 - Section 1", SemesterId = 5, SubjectId = 8 },
            new() { CourseId = 20, CourseName = "IOT102 - Spring 2025 - Section 1", SemesterId = 5, SubjectId = 9 },
        };
        modelBuilder.Entity<Course>().HasData(courses);

        // 50 Students
        var firstNames = new[] { "Nguyen Van", "Tran Thi", "Le Hoang", "Pham Minh", "Hoang Duc", "Vo Thanh", "Dang Quoc", "Bui Xuan", "Do Thi", "Ngo Quang" };
        var lastNames = new[] { "An", "Binh", "Cuong", "Dung", "Em", "Phuc", "Gia", "Hung", "Ich", "Khanh", "Lam", "Minh", "Nhat", "Oanh", "Phat" };
        var students = new List<Student>();
        var random = new Random(42);
        for (int i = 1; i <= 50; i++)
        {
            students.Add(new Student
            {
                StudentId = i,
                FullName = $"{firstNames[(i - 1) % firstNames.Length]} {lastNames[(i - 1) % lastNames.Length]}",
                Email = $"student{i:D3}@fpt.edu.vn",
                DateOfBirth = new DateTime(2000 + (i % 5), (i % 12) + 1, (i % 28) + 1)
            });
        }
        modelBuilder.Entity<Student>().HasData(students);

        // 500 Enrollments
        var statuses = new[] { "Active", "Completed", "Dropped", "Pending" };
        var enrollments = new List<Enrollment>();
        int enrollId = 1;
        for (int i = 1; i <= 50; i++) // each student
        {
            // Each student enrolls in 10 courses
            var courseIds = Enumerable.Range(1, 20).OrderBy(_ => random.Next()).Take(10).ToList();
            foreach (var courseId in courseIds)
            {
                enrollments.Add(new Enrollment
                {
                    EnrollmentId = enrollId++,
                    StudentId = i,
                    CourseId = courseId,
                    EnrollDate = new DateTime(2023, 9, 1).AddDays(random.Next(0, 600)),
                    Status = statuses[random.Next(statuses.Length)]
                });
            }
        }
        modelBuilder.Entity<Enrollment>().HasData(enrollments);
    }
}
