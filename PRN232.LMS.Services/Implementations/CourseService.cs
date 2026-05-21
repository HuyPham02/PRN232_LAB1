using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly IGenericRepository<Course> _repository;

    public CourseService(IGenericRepository<Course> repository)
    {
        _repository = repository;
    }

    public async Task<CourseModel?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id, c => c.Semester!, c => c.Subject!, c => c.Enrollments);
        if (entity == null) return null;
        return MapToModel(entity, true, true, true);
    }

    public async Task<PagedResult<CourseModel>> GetAllAsync(QueryParameters query)
    {
        var expandFields = query.Expand?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLower()).ToList() ?? new List<string>();

        IQueryable<Course> q = _repository.GetAll();

        if (expandFields.Contains("semester"))
            q = q.Include(c => c.Semester);
        if (expandFields.Contains("subject"))
            q = q.Include(c => c.Subject);
        if (expandFields.Contains("enrollments"))
            q = q.Include(c => c.Enrollments);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(c => c.CourseName.ToLower().Contains(search));
        }

        var totalItems = await q.CountAsync();
        q = ApplySort(q, query.Sort);
        q = q.Skip((query.Page - 1) * query.Size).Take(query.Size);

        var entities = await q.ToListAsync();
        var models = entities.Select(e => MapToModel(e,
            expandFields.Contains("semester"),
            expandFields.Contains("subject"),
            expandFields.Contains("enrollments"))).ToList();

        return new PagedResult<CourseModel>
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

    public async Task<CourseModel> CreateAsync(CourseModel model)
    {
        var entity = new Course
        {
            CourseName = model.CourseName,
            SemesterId = model.SemesterId,
            SubjectId = model.SubjectId
        };
        var created = await _repository.AddAsync(entity);
        return MapToModel(created, false, false, false);
    }

    public async Task<CourseModel?> UpdateAsync(int id, CourseModel model)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.CourseName = model.CourseName;
        entity.SemesterId = model.SemesterId;
        entity.SubjectId = model.SubjectId;

        await _repository.UpdateAsync(entity);
        return MapToModel(entity, false, false, false);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;
        await _repository.DeleteAsync(entity);
        return true;
    }

    private static IQueryable<Course> ApplySort(IQueryable<Course> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return query.OrderBy(c => c.CourseId);

        var sortFields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        IOrderedQueryable<Course>? ordered = null;

        foreach (var field in sortFields)
        {
            var trimmed = field.Trim();
            var descending = trimmed.StartsWith('-');
            var propName = descending ? trimmed[1..] : trimmed;

            ordered = propName.ToLower() switch
            {
                "coursename" => descending
                    ? (ordered == null ? query.OrderByDescending(c => c.CourseName) : ordered.ThenByDescending(c => c.CourseName))
                    : (ordered == null ? query.OrderBy(c => c.CourseName) : ordered.ThenBy(c => c.CourseName)),
                "courseid" => descending
                    ? (ordered == null ? query.OrderByDescending(c => c.CourseId) : ordered.ThenByDescending(c => c.CourseId))
                    : (ordered == null ? query.OrderBy(c => c.CourseId) : ordered.ThenBy(c => c.CourseId)),
                _ => ordered ?? query.OrderBy(c => c.CourseId)
            };
        }

        return ordered ?? query.OrderBy(c => c.CourseId);
    }

    private static CourseModel MapToModel(Course entity, bool includeSemester, bool includeSubject, bool includeEnrollments)
    {
        var model = new CourseModel
        {
            CourseId = entity.CourseId,
            CourseName = entity.CourseName,
            SemesterId = entity.SemesterId,
            SubjectId = entity.SubjectId
        };

        if (includeSemester && entity.Semester != null)
        {
            model.Semester = new SemesterModel
            {
                SemesterId = entity.Semester.SemesterId,
                SemesterName = entity.Semester.SemesterName,
                StartDate = entity.Semester.StartDate,
                EndDate = entity.Semester.EndDate
            };
        }

        if (includeSubject && entity.Subject != null)
        {
            model.Subject = new SubjectModel
            {
                SubjectId = entity.Subject.SubjectId,
                SubjectCode = entity.Subject.SubjectCode,
                SubjectName = entity.Subject.SubjectName,
                Credit = entity.Subject.Credit
            };
        }

        if (includeEnrollments && entity.Enrollments != null)
        {
            model.Enrollments = entity.Enrollments.Select(e => new EnrollmentModel
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status
            }).ToList();
        }

        return model;
    }
}
