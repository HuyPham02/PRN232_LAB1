using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentModel?> GetByIdAsync(int id);
    Task<PagedResult<EnrollmentModel>> GetAllAsync(QueryParameters query);
    Task<EnrollmentModel> CreateAsync(EnrollmentModel model);
    Task<EnrollmentModel?> UpdateAsync(int id, EnrollmentModel model);
    Task<bool> DeleteAsync(int id);
}
