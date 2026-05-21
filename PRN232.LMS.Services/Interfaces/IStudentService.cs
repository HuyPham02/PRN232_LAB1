using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    Task<StudentModel?> GetByIdAsync(int id);
    Task<PagedResult<StudentModel>> GetAllAsync(QueryParameters query);
    Task<StudentModel> CreateAsync(StudentModel model);
    Task<StudentModel?> UpdateAsync(int id, StudentModel model);
    Task<bool> DeleteAsync(int id);
}
