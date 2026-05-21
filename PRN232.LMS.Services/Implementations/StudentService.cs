using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IGenericRepository<Student> _repository;

    public StudentService(IGenericRepository<Student> repository)
    {
        _repository = repository;
    }

    public async Task<StudentModel?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id, s => s.Enrollments);
        if (entity == null) return null;

        return MapToModel(entity, includeEnrollments: true);
    }

    public async Task<PagedResult<StudentModel>> GetAllAsync(QueryParameters query)
    {
        var expandFields = query.Expand?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLower()).ToList() ?? new List<string>();

        IQueryable<Student> q = _repository.GetAll();

        // Expansion
        if (expandFields.Contains("enrollments"))
        {
            q = q.Include(s => s.Enrollments).ThenInclude(e => e.Course);
        }

        // Search
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(s => s.FullName.ToLower().Contains(search)
                          || s.Email.ToLower().Contains(search));
        }

        // Count before paging
        var totalItems = await q.CountAsync();

        // Sort
        q = ApplySort(q, query.Sort);

        // Paging
        q = q.Skip((query.Page - 1) * query.Size).Take(query.Size);

        var entities = await q.ToListAsync();
        var models = entities.Select(e => MapToModel(e, expandFields.Contains("enrollments"))).ToList();

        return new PagedResult<StudentModel>
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

    public async Task<StudentModel> CreateAsync(StudentModel model)
    {
        var entity = new Student
        {
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth
        };

        var created = await _repository.AddAsync(entity);
        return MapToModel(created, false);
    }

    public async Task<StudentModel?> UpdateAsync(int id, StudentModel model)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.FullName = model.FullName;
        entity.Email = model.Email;
        entity.DateOfBirth = model.DateOfBirth;

        await _repository.UpdateAsync(entity);
        return MapToModel(entity, false);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        await _repository.DeleteAsync(entity);
        return true;
    }

    private static IQueryable<Student> ApplySort(IQueryable<Student> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return query.OrderBy(s => s.StudentId);

        var sortFields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        IOrderedQueryable<Student>? ordered = null;

        foreach (var field in sortFields)
        {
            var trimmed = field.Trim();
            var descending = trimmed.StartsWith('-');
            var propName = descending ? trimmed[1..] : trimmed;

            ordered = propName.ToLower() switch
            {
                "fullname" => descending
                    ? (ordered == null ? query.OrderByDescending(s => s.FullName) : ordered.ThenByDescending(s => s.FullName))
                    : (ordered == null ? query.OrderBy(s => s.FullName) : ordered.ThenBy(s => s.FullName)),
                "email" => descending
                    ? (ordered == null ? query.OrderByDescending(s => s.Email) : ordered.ThenByDescending(s => s.Email))
                    : (ordered == null ? query.OrderBy(s => s.Email) : ordered.ThenBy(s => s.Email)),
                "dateofbirth" => descending
                    ? (ordered == null ? query.OrderByDescending(s => s.DateOfBirth) : ordered.ThenByDescending(s => s.DateOfBirth))
                    : (ordered == null ? query.OrderBy(s => s.DateOfBirth) : ordered.ThenBy(s => s.DateOfBirth)),
                "studentid" => descending
                    ? (ordered == null ? query.OrderByDescending(s => s.StudentId) : ordered.ThenByDescending(s => s.StudentId))
                    : (ordered == null ? query.OrderBy(s => s.StudentId) : ordered.ThenBy(s => s.StudentId)),
                _ => ordered ?? query.OrderBy(s => s.StudentId)
            };
        }

        return ordered ?? query.OrderBy(s => s.StudentId);
    }

    private static StudentModel MapToModel(Student entity, bool includeEnrollments)
    {
        var model = new StudentModel
        {
            StudentId = entity.StudentId,
            FullName = entity.FullName,
            Email = entity.Email,
            DateOfBirth = entity.DateOfBirth
        };

        if (includeEnrollments && entity.Enrollments != null)
        {
            model.Enrollments = entity.Enrollments.Select(e => new EnrollmentModel
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status,
                Course = e.Course != null ? new CourseModel
                {
                    CourseId = e.Course.CourseId,
                    CourseName = e.Course.CourseName,
                    SemesterId = e.Course.SemesterId,
                    SubjectId = e.Course.SubjectId
                } : null
            }).ToList();
        }

        return model;
    }
}
