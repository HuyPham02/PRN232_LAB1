using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models.Request;
using PRN232.LMS.API.Models.Response;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>
    /// Get a course by ID with related semester, subject, and enrollments
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var model = await _courseService.GetByIdAsync(id);
        if (model == null)
            return NotFound(ApiResponse<CourseResponse>.ErrorResponse("Course not found"));

        return Ok(ApiResponse<CourseResponse>.SuccessResponse(MapToResponse(model)));
    }

    /// <summary>
    /// Get list of courses with search, sort, paging, field selection, and expansion
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<object>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? fields = null,
        [FromQuery] string? expand = null)
    {
        var query = new QueryParameters
        {
            Search = search, Sort = sort, Page = page, Size = size,
            Fields = fields, Expand = expand
        };

        var result = await _courseService.GetAllAsync(query);
        var responseItems = result.Items.Select(MapToResponse).ToList();

        if (!string.IsNullOrWhiteSpace(fields))
        {
            var selectedFields = fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim().ToLower()).ToHashSet();

            var filteredItems = responseItems.Select(item => SelectFields(item, selectedFields)).ToList();
            var filteredResult = new
            {
                items = filteredItems,
                pagination = new PaginationResponse
                {
                    Page = result.Pagination.Page, PageSize = result.Pagination.PageSize,
                    TotalItems = result.Pagination.TotalItems, TotalPages = result.Pagination.TotalPages
                }
            };
            return Ok(ApiResponse<object>.SuccessResponse(filteredResult));
        }

        var pagedResponse = new PagedResponse<CourseResponse>
        {
            Items = responseItems,
            Pagination = new PaginationResponse
            {
                Page = result.Pagination.Page, PageSize = result.Pagination.PageSize,
                TotalItems = result.Pagination.TotalItems, TotalPages = result.Pagination.TotalPages
            }
        };
        return Ok(ApiResponse<PagedResponse<CourseResponse>>.SuccessResponse(pagedResponse));
    }

    /// <summary>
    /// Create a new course
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CourseResponse>.ErrorResponse("Invalid request", ModelState));

        var model = new CourseModel
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId,
            SubjectId = request.SubjectId
        };

        var created = await _courseService.CreateAsync(model);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CourseResponse>.SuccessResponse(MapToResponse(created), "Course created successfully"));
    }

    /// <summary>
    /// Update an existing course
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CourseResponse>.ErrorResponse("Invalid request", ModelState));

        var model = new CourseModel
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId,
            SubjectId = request.SubjectId
        };

        var updated = await _courseService.UpdateAsync(id, model);
        if (updated == null)
            return NotFound(ApiResponse<CourseResponse>.ErrorResponse("Course not found"));

        return Ok(ApiResponse<CourseResponse>.SuccessResponse(MapToResponse(updated), "Course updated successfully"));
    }

    /// <summary>
    /// Delete a course
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _courseService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.ErrorResponse("Course not found"));

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Course deleted successfully"));
    }

    /// <summary>
    /// Get enrollments for a specific course with search, sort, paging, field selection, and expansion
    /// </summary>
    [HttpGet("{id}/enrollments")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<EnrollmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnrollments(int id,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? fields = null,
        [FromQuery] string? expand = null)
    {
        var query = new QueryParameters
        {
            Search = search, Sort = sort, Page = page, Size = size,
            Fields = fields, Expand = expand
        };

        var result = await _courseService.GetEnrollmentsByCourseIdAsync(id, query);
        if (result == null)
            return NotFound(ApiResponse<object>.ErrorResponse("Course not found"));

        var responseItems = result.Items.Select(MapEnrollmentToResponse).ToList();

        if (!string.IsNullOrWhiteSpace(fields))
        {
            var selectedFields = fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim().ToLower()).ToHashSet();

            var filteredItems = responseItems.Select(item => SelectEnrollmentFields(item, selectedFields)).ToList();
            var filteredResult = new
            {
                items = filteredItems,
                pagination = new PaginationResponse
                {
                    Page = result.Pagination.Page, PageSize = result.Pagination.PageSize,
                    TotalItems = result.Pagination.TotalItems, TotalPages = result.Pagination.TotalPages
                }
            };
            return Ok(ApiResponse<object>.SuccessResponse(filteredResult));
        }

        var pagedResponse = new PagedResponse<EnrollmentResponse>
        {
            Items = responseItems,
            Pagination = new PaginationResponse
            {
                Page = result.Pagination.Page, PageSize = result.Pagination.PageSize,
                TotalItems = result.Pagination.TotalItems, TotalPages = result.Pagination.TotalPages
            }
        };
        return Ok(ApiResponse<PagedResponse<EnrollmentResponse>>.SuccessResponse(pagedResponse));
    }

    private static CourseResponse MapToResponse(CourseModel model)
    {
        return new CourseResponse
        {
            CourseId = model.CourseId,
            CourseName = model.CourseName,
            SemesterId = model.SemesterId,
            SubjectId = model.SubjectId,
            Semester = model.Semester != null ? new SemesterResponse
            {
                SemesterId = model.Semester.SemesterId,
                SemesterName = model.Semester.SemesterName,
                StartDate = model.Semester.StartDate,
                EndDate = model.Semester.EndDate
            } : null,
            Subject = model.Subject != null ? new SubjectResponse
            {
                SubjectId = model.Subject.SubjectId,
                SubjectCode = model.Subject.SubjectCode,
                SubjectName = model.Subject.SubjectName,
                Credit = model.Subject.Credit
            } : null,
            Enrollments = model.Enrollments?.Any() == true
                ? model.Enrollments.Select(e => new EnrollmentResponse
                {
                    EnrollmentId = e.EnrollmentId,
                    StudentId = e.StudentId,
                    CourseId = e.CourseId,
                    EnrollDate = e.EnrollDate,
                    Status = e.Status
                }).ToList()
                : null
        };
    }

    private static Dictionary<string, object?> SelectFields(CourseResponse item, HashSet<string> fields)
    {
        var dict = new Dictionary<string, object?>();
        if (fields.Contains("courseid")) dict["courseId"] = item.CourseId;
        if (fields.Contains("coursename")) dict["courseName"] = item.CourseName;
        if (fields.Contains("semesterid")) dict["semesterId"] = item.SemesterId;
        if (fields.Contains("subjectid")) dict["subjectId"] = item.SubjectId;
        if (fields.Contains("semester")) dict["semester"] = item.Semester;
        if (fields.Contains("subject")) dict["subject"] = item.Subject;
        if (fields.Contains("enrollments")) dict["enrollments"] = item.Enrollments;
        return dict;
    }

    private static EnrollmentResponse MapEnrollmentToResponse(EnrollmentModel model)
    {
        return new EnrollmentResponse
        {
            EnrollmentId = model.EnrollmentId,
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status,
            Student = model.Student != null ? new StudentResponse
            {
                StudentId = model.Student.StudentId,
                FullName = model.Student.FullName,
                Email = model.Student.Email,
                DateOfBirth = model.Student.DateOfBirth
            } : null,
            Course = model.Course != null ? new CourseResponse
            {
                CourseId = model.Course.CourseId,
                CourseName = model.Course.CourseName,
                SemesterId = model.Course.SemesterId,
                SubjectId = model.Course.SubjectId,
                Semester = model.Course.Semester != null ? new SemesterResponse
                {
                    SemesterId = model.Course.Semester.SemesterId,
                    SemesterName = model.Course.Semester.SemesterName,
                    StartDate = model.Course.Semester.StartDate,
                    EndDate = model.Course.Semester.EndDate
                } : null
            } : null
        };
    }

    private static Dictionary<string, object?> SelectEnrollmentFields(EnrollmentResponse item, HashSet<string> fields)
    {
        var dict = new Dictionary<string, object?>();
        if (fields.Contains("enrollmentid")) dict["enrollmentId"] = item.EnrollmentId;
        if (fields.Contains("studentid")) dict["studentId"] = item.StudentId;
        if (fields.Contains("courseid")) dict["courseId"] = item.CourseId;
        if (fields.Contains("enrolldate")) dict["enrollDate"] = item.EnrollDate;
        if (fields.Contains("status")) dict["status"] = item.Status;
        if (fields.Contains("student")) dict["student"] = item.Student;
        if (fields.Contains("course")) dict["course"] = item.Course;
        return dict;
    }
}
