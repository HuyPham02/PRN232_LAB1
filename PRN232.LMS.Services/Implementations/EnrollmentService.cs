using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IGenericRepository<Enrollment> _repository;

    public EnrollmentService(IGenericRepository<Enrollment> repository)
    {
        _repository = repository;
    }

    public async Task<EnrollmentModel?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id, e => e.Student!, e => e.Course!);
        if (entity == null) return null;
        return MapToModel(entity, true, true);
    }

    public async Task<PagedResult<EnrollmentModel>> GetAllAsync(QueryParameters query)
    {
        var expandFields = query.Expand?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLower()).ToList() ?? new List<string>();

        IQueryable<Enrollment> q = _repository.GetAll();

        // Expansion
        if (expandFields.Contains("student"))
            q = q.Include(e => e.Student);
        if (expandFields.Contains("course"))
            q = q.Include(e => e.Course).ThenInclude(c => c!.Semester);

        // Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(e => e.Status.ToLower().Contains(search)
                          || (e.Student != null && e.Student.FullName.ToLower().Contains(search))
                          || (e.Course != null && e.Course.CourseName.ToLower().Contains(search)));
        }

        var totalItems = await q.CountAsync();

        // Sort
        q = ApplySort(q, query.Sort);

        // Paging
        q = q.Skip((query.Page - 1) * query.Size).Take(query.Size);

        var entities = await q.ToListAsync();
        var models = entities.Select(e => MapToModel(e, expandFields.Contains("student"), expandFields.Contains("course"))).ToList();

        return new PagedResult<EnrollmentModel>
        {
            Items = models,
            Pagination = new PaginationMetadata
            {
                Page = query.Page,
                PageSize = query.Size,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)query.Size)
            }
        };
    }

    public async Task<EnrollmentModel> CreateAsync(EnrollmentModel model)
    {
        var entity = new Enrollment
        {
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status
        };

        var created = await _repository.AddAsync(entity);
        return MapToModel(created, false, false);
    }

    public async Task<EnrollmentModel?> UpdateAsync(int id, EnrollmentModel model)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.StudentId = model.StudentId;
        entity.CourseId = model.CourseId;
        entity.EnrollDate = model.EnrollDate;
        entity.Status = model.Status;

        await _repository.UpdateAsync(entity);
        return MapToModel(entity, false, false);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        await _repository.DeleteAsync(entity);
        return true;
    }

    private static IQueryable<Enrollment> ApplySort(IQueryable<Enrollment> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return query.OrderBy(e => e.EnrollmentId);

        var sortFields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        IOrderedQueryable<Enrollment>? ordered = null;

        foreach (var field in sortFields)
        {
            var trimmed = field.Trim();
            var descending = trimmed.StartsWith('-');
            var propName = descending ? trimmed[1..] : trimmed;

            ordered = propName.ToLower() switch
            {
                "enrolldate" => descending
                    ? (ordered == null ? query.OrderByDescending(e => e.EnrollDate) : ordered.ThenByDescending(e => e.EnrollDate))
                    : (ordered == null ? query.OrderBy(e => e.EnrollDate) : ordered.ThenBy(e => e.EnrollDate)),
                "status" => descending
                    ? (ordered == null ? query.OrderByDescending(e => e.Status) : ordered.ThenByDescending(e => e.Status))
                    : (ordered == null ? query.OrderBy(e => e.Status) : ordered.ThenBy(e => e.Status)),
                "enrollmentid" => descending
                    ? (ordered == null ? query.OrderByDescending(e => e.EnrollmentId) : ordered.ThenByDescending(e => e.EnrollmentId))
                    : (ordered == null ? query.OrderBy(e => e.EnrollmentId) : ordered.ThenBy(e => e.EnrollmentId)),
                "studentid" => descending
                    ? (ordered == null ? query.OrderByDescending(e => e.StudentId) : ordered.ThenByDescending(e => e.StudentId))
                    : (ordered == null ? query.OrderBy(e => e.StudentId) : ordered.ThenBy(e => e.StudentId)),
                _ => ordered ?? query.OrderBy(e => e.EnrollmentId)
            };
        }

        return ordered ?? query.OrderBy(e => e.EnrollmentId);
    }

    private static EnrollmentModel MapToModel(Enrollment entity, bool includeStudent, bool includeCourse)
    {
        var model = new EnrollmentModel
        {
            EnrollmentId = entity.EnrollmentId,
            StudentId = entity.StudentId,
            CourseId = entity.CourseId,
            EnrollDate = entity.EnrollDate,
            Status = entity.Status
        };

        if (includeStudent && entity.Student != null)
        {
            model.Student = new StudentModel
            {
                StudentId = entity.Student.StudentId,
                FullName = entity.Student.FullName,
                Email = entity.Student.Email,
                DateOfBirth = entity.Student.DateOfBirth
            };
        }

        if (includeCourse && entity.Course != null)
        {
            model.Course = new CourseModel
            {
                CourseId = entity.Course.CourseId,
                CourseName = entity.Course.CourseName,
                SemesterId = entity.Course.SemesterId,
                SubjectId = entity.Course.SubjectId,
                Semester = entity.Course.Semester != null ? new SemesterModel
                {
                    SemesterId = entity.Course.Semester.SemesterId,
                    SemesterName = entity.Course.Semester.SemesterName
                } : null
            };
        }

        return model;
    }
}
