namespace PRN232.LMS.API.Models.Response;

// Consistent API response wrapper
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Request processed successfully")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data, Errors = null };
    }

    public static ApiResponse<T> ErrorResponse(string message, object? errors = null)
    {
        return new ApiResponse<T> { Success = false, Message = message, Data = default, Errors = errors };
    }
}

// Response DTOs
public class StudentResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public List<EnrollmentResponse>? Enrollments { get; set; }
}

public class EnrollmentResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public StudentResponse? Student { get; set; }
    public CourseResponse? Course { get; set; }
}

public class CourseResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public int SubjectId { get; set; }
    public SemesterResponse? Semester { get; set; }
    public SubjectResponse? Subject { get; set; }
    public List<EnrollmentResponse>? Enrollments { get; set; }
}

public class SemesterResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CourseResponse>? Courses { get; set; }
}

public class SubjectResponse
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
    public List<CourseResponse>? Courses { get; set; }
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationResponse Pagination { get; set; } = new();
}

public class PaginationResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
