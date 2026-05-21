using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ICourseService
{
    Task<CourseModel?> GetByIdAsync(int id);
    Task<PagedResult<CourseModel>> GetAllAsync(QueryParameters query);
    Task<CourseModel> CreateAsync(CourseModel model);
    Task<CourseModel?> UpdateAsync(int id, CourseModel model);
    Task<bool> DeleteAsync(int id);
}
