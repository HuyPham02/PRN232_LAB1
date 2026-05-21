using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<SemesterModel?> GetByIdAsync(int id);
    Task<PagedResult<SemesterModel>> GetAllAsync(QueryParameters query);
    Task<SemesterModel> CreateAsync(SemesterModel model);
    Task<SemesterModel?> UpdateAsync(int id, SemesterModel model);
    Task<bool> DeleteAsync(int id);
}
