using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Request;

public class CreateStudentRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }
}

public class UpdateStudentRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }
}

public class CreateEnrollmentRequest
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public DateTime EnrollDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}

public class UpdateEnrollmentRequest
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required]
    public DateTime EnrollDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}

public class CreateCourseRequest
{
    [Required]
    [MaxLength(100)]
    public string CourseName { get; set; } = string.Empty;

    [Required]
    public int SemesterId { get; set; }

    [Required]
    public int SubjectId { get; set; }
}

public class UpdateCourseRequest
{
    [Required]
    [MaxLength(100)]
    public string CourseName { get; set; } = string.Empty;

    [Required]
    public int SemesterId { get; set; }

    [Required]
    public int SubjectId { get; set; }
}

public class CreateSemesterRequest
{
    [Required]
    [MaxLength(100)]
    public string SemesterName { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}

public class UpdateSemesterRequest
{
    [Required]
    [MaxLength(100)]
    public string SemesterName { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}

public class CreateSubjectRequest
{
    [Required]
    [MaxLength(20)]
    public string SubjectCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    [Required]
    public int Credit { get; set; }
}

public class UpdateSubjectRequest
{
    [Required]
    [MaxLength(20)]
    public string SubjectCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    [Required]
    public int Credit { get; set; }
}
