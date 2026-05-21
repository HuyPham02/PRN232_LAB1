using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implementations;

public class SemesterService : ISemesterService
{
    private readonly IGenericRepository<Semester> _repository;

    public SemesterService(IGenericRepository<Semester> repository)
    {
        _repository = repository;
    }

    public async Task<SemesterModel?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id, s => s.Courses);
        if (entity == null) return null;
        return MapToModel(entity, true);
    }

    public async Task<PagedResult<SemesterModel>> GetAllAsync(QueryParameters query)
    {
        var expandFields = query.Expand?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLower()).ToList() ?? new List<string>();

        IQueryable<Semester> q = _repository.GetAll();

        if (expandFields.Contains("courses"))
            q = q.Include(s => s.Courses);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(s => s.SemesterName.ToLower().Contains(search));
        }

        var totalItems = await q.CountAsync();

        q = ApplySort(q, query.Sort);
        q = q.Skip((query.Page - 1) * query.Size).Take(query.Size);

        var entities = await q.ToListAsync();
        var models = entities.Select(e => MapToModel(e, expandFields.Contains("courses"))).ToList();

        return new PagedResult<SemesterModel>
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

    public async Task<SemesterModel> CreateAsync(SemesterModel model)
    {
        var entity = new Semester
        {
            SemesterName = model.SemesterName,
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };
        var created = await _repository.AddAsync(entity);
        return MapToModel(created, false);
    }

    public async Task<SemesterModel?> UpdateAsync(int id, SemesterModel model)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.SemesterName = model.SemesterName;
        entity.StartDate = model.StartDate;
        entity.EndDate = model.EndDate;

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

    private static IQueryable<Semester> ApplySort(IQueryable<Semester> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return query.OrderBy(s => s.SemesterId);

        var sortFields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        IOrderedQueryable<Semester>? ordered = null;

        foreach (var field in sortFields)
        {
            var trimmed = field.Trim();
            var descending = trimmed.StartsWith('-');
            var propName = descending ? trimmed[1..] : trimmed;

            ordered = propName.ToLower() switch
            {
                "semestername" => descending
                    ? (ordered == null ? query.OrderByDescending(s => s.SemesterName) : ordered.ThenByDescending(s => s.SemesterName))
                    : (ordered == null ? query.OrderBy(s => s.SemesterName) : ordered.ThenBy(s => s.SemesterName)),
                "startdate" => descending
                    ? (ordered == null ? query.OrderByDescending(s => s.StartDate) : ordered.ThenByDescending(s => s.StartDate))
                    : (ordered == null ? query.OrderBy(s => s.StartDate) : ordered.ThenBy(s => s.StartDate)),
                "enddate" => descending
                    ? (ordered == null ? query.OrderByDescending(s => s.EndDate) : ordered.ThenByDescending(s => s.EndDate))
                    : (ordered == null ? query.OrderBy(s => s.EndDate) : ordered.ThenBy(s => s.EndDate)),
                _ => ordered ?? query.OrderBy(s => s.SemesterId)
            };
        }

        return ordered ?? query.OrderBy(s => s.SemesterId);
    }

    private static SemesterModel MapToModel(Semester entity, bool includeCourses)
    {
        var model = new SemesterModel
        {
            SemesterId = entity.SemesterId,
            SemesterName = entity.SemesterName,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate
        };

        if (includeCourses && entity.Courses != null)
        {
            model.Courses = entity.Courses.Select(c => new CourseModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                SemesterId = c.SemesterId,
                SubjectId = c.SubjectId
            }).ToList();
        }

        return model;
    }
}
