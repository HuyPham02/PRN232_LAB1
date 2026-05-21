using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232.LMS.Repositories.Entities;

[Table("Enrollment")]
public class Enrollment
{
    [Key]
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrollDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    // Navigation
    [ForeignKey("StudentId")]
    public virtual Student? Student { get; set; }

    [ForeignKey("CourseId")]
    public virtual Course? Course { get; set; }
}
