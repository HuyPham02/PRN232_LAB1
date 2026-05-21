using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implementations;

public class SubjectService : ISubjectService
{
    private readonly IGenericRepository<Subject> _repository;

    public SubjectService(IGenericRepository<Subject> repository)
    {
        _repository = repository;
    }

    public async Task<SubjectModel?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id, s => s.Courses);
        if (entity == null) return null;
        return MapToModel(entity, true);
    }

    public async Task<PagedResult<SubjectModel>> GetAllAsync(QueryParameters query)
    {
        var expandFields = query.Expand?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLower()).ToList() ?? new List<string>();

        IQueryable<Subject> q = _repository.GetAll();

        if (expandFields.Contains("courses"))
            q = q.Include(s => s.Courses);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(s => s.SubjectCode.ToLower().Contains(search)
                          || s.SubjectName.ToLower().Contains(search));
        }

        var totalItems = await q.CountAsync();
        q = ApplySort(q, query.Sort);
        q = q.Skip((query.Page - 1) * query.Size).Take(query.Size);

        var entities = await q.ToListAsync();
        var models = entities.Select(e => MapToModel(e, expandFields.Contains("courses"))).ToList();

        return new PagedResult<SubjectModel>
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

    public async Task<SubjectModel> CreateAsync(SubjectModel model)
    {
        var entity = new Subject
        {
            SubjectCode = model.SubjectCode,
            SubjectName = model.SubjectName,
            Credit = model.Credit
        };
        var created = await _repository.AddAsync(entity);
        return MapToModel(created, false);
    }

    public async Task<SubjectModel?> UpdateAsync(int id, SubjectModel model)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.SubjectCode = model.SubjectCode;
        entity.SubjectName = model.SubjectName;
        entity.Credit = model.Credit;

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

    private static IQueryable<Subject> ApplySort(IQueryable<Subject> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return query.OrderBy(s => s.SubjectId);

        var sortFields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        IOrderedQueryable<Subject>? ordered = null;

        foreach (var field in sortFields)
        {
            var trimmed = field.Trim();
            var descending = trimmed.StartsWith('-');
            var propName = descending ? trimmed[1..] : trimmed;

            ordered = propName.ToLower() switch
            {
                "subjectcode" => descending
                    ? (ordered == null ? query.OrderByDescending(s => s.SubjectCode) : ordered.ThenByDescending(s => s.SubjectCode))
                    : (ordered == null ? query.OrderBy(s => s.SubjectCode) : ordered.ThenBy(s => s.SubjectCode)),
                "subjectname" => descending
                    ? (ordered == null ? query.OrderByDescending(s => s.SubjectName) : ordered.ThenByDescending(s => s.SubjectName))
                    : (ordered == null ? query.OrderBy(s => s.SubjectName) : ordered.ThenBy(s => s.SubjectName)),
                "credit" => descending
                    ? (ordered == null ? query.OrderByDescending(s => s.Credit) : ordered.ThenByDescending(s => s.Credit))
                    : (ordered == null ? query.OrderBy(s => s.Credit) : ordered.ThenBy(s => s.Credit)),
                _ => ordered ?? query.OrderBy(s => s.SubjectId)
            };
        }

        return ordered ?? query.OrderBy(s => s.SubjectId);
    }

    private static SubjectModel MapToModel(Subject entity, bool includeCourses)
    {
        var model = new SubjectModel
        {
            SubjectId = entity.SubjectId,
            SubjectCode = entity.SubjectCode,
            SubjectName = entity.SubjectName,
            Credit = entity.Credit
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
