using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232.LMS.Repositories.Entities;

[Table("Semester")]
public class Semester
{
    [Key]
    public int SemesterId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SemesterName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    // Navigation
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
