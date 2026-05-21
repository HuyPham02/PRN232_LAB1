namespace PRN232.LMS.Services.Models;

// Business Models - used for processing in Service Layer

public class SemesterModel
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CourseModel> Courses { get; set; } = new();
}

public class SubjectModel
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
    public List<CourseModel> Courses { get; set; } = new();
}

public class CourseModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public int SubjectId { get; set; }
    public SemesterModel? Semester { get; set; }
    public SubjectModel? Subject { get; set; }
    public List<EnrollmentModel> Enrollments { get; set; } = new();
}

public class StudentModel
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public List<EnrollmentModel> Enrollments { get; set; } = new();
}

public class EnrollmentModel
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public StudentModel? Student { get; set; }
    public CourseModel? Course { get; set; }
}

// Query parameters model
public class QueryParameters
{
    private int _pageSize = 10;
    private const int MaxPageSize = 50;

    public string? Search { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;

    public int Size
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string? Fields { get; set; }
    public string? Expand { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationMetadata Pagination { get; set; } = new();
}

public class PaginationMetadata
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
