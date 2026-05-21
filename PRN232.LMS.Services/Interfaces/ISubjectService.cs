using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<SubjectModel?> GetByIdAsync(int id);
    Task<PagedResult<SubjectModel>> GetAllAsync(QueryParameters query);
    Task<SubjectModel> CreateAsync(SubjectModel model);
    Task<SubjectModel?> UpdateAsync(int id, SubjectModel model);
    Task<bool> DeleteAsync(int id);
}
