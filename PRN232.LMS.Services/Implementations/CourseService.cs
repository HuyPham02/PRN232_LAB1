using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly IGenericRepository<Course> _repository;
    private readonly IGenericRepository<Enrollment> _enrollmentRepository;

    public CourseService(IGenericRepository<Course> repository, IGenericRepository<Enrollment> enrollmentRepository)
    {
        _repository = repository;
        _enrollmentRepository = enrollmentRepository;
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

    public async Task<PagedResult<EnrollmentModel>?> GetEnrollmentsByCourseIdAsync(int courseId, QueryParameters query)
    {
        var course = await _repository.GetByIdAsync(courseId);
        if (course == null) return null;

        var expandFields = query.Expand?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLower()).ToList() ?? new List<string>();

        IQueryable<Enrollment> q = _enrollmentRepository.GetAll().Where(e => e.CourseId == courseId);

        if (expandFields.Contains("student"))
            q = q.Include(e => e.Student);
        if (expandFields.Contains("course"))
            q = q.Include(e => e.Course).ThenInclude(c => c!.Semester);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(e => e.Status.ToLower().Contains(search)
                          || (e.Student != null && e.Student.FullName.ToLower().Contains(search)));
        }

        var totalItems = await q.CountAsync();
        q = ApplyEnrollmentSort(q, query.Sort);
        q = q.Skip((query.Page - 1) * query.Size).Take(query.Size);

        var entities = await q.ToListAsync();
        var models = entities.Select(e => MapEnrollmentToModel(e, expandFields.Contains("student"), expandFields.Contains("course"))).ToList();

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

    private static IQueryable<Enrollment> ApplyEnrollmentSort(IQueryable<Enrollment> query, string? sort)
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

    private static EnrollmentModel MapEnrollmentToModel(Enrollment entity, bool includeStudent, bool includeCourse)
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
