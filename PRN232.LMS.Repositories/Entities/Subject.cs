using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232.LMS.Repositories.Entities;

[Table("Subject")]
public class Subject
{
    [Key]
    public int SubjectId { get; set; }

    [Required]
    [MaxLength(20)]
    public string SubjectCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string SubjectName { get; set; } = string.Empty;

    public int Credit { get; set; }

    // Navigation
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
