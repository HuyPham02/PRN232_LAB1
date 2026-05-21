using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models.Request;
using PRN232.LMS.API.Models.Response;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    /// <summary>
    /// Get an enrollment by ID with related student and course
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var model = await _enrollmentService.GetByIdAsync(id);
        if (model == null)
            return NotFound(ApiResponse<EnrollmentResponse>.ErrorResponse("Enrollment not found"));

        return Ok(ApiResponse<EnrollmentResponse>.SuccessResponse(MapToResponse(model)));
    }

    /// <summary>
    /// Get list of enrollments with search, sort, paging, field selection, and expansion
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
            Search = search,
            Sort = sort,
            Page = page,
            Size = size,
            Fields = fields,
            Expand = expand
        };

        var result = await _enrollmentService.GetAllAsync(query);
        var responseItems = result.Items.Select(MapToResponse).ToList();

        if (!string.IsNullOrWhiteSpace(fields))
        {
            var selectedFields = fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim().ToLower()).ToHashSet();

            var filteredItems = responseItems.Select(item => SelectFields(item, selectedFields)).ToList();

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                items = filteredItems,
                pagination = new PaginationResponse
                {
                    Page = result.Pagination.Page,
                    PageSize = result.Pagination.PageSize,
                    TotalItems = result.Pagination.TotalItems,
                    TotalPages = result.Pagination.TotalPages
                }
            }));
        }

        var pagedResponse = new PagedResponse<EnrollmentResponse>
        {
            Items = responseItems,
            Pagination = new PaginationResponse
            {
                Page = result.Pagination.Page,
                PageSize = result.Pagination.PageSize,
                TotalItems = result.Pagination.TotalItems,
                TotalPages = result.Pagination.TotalPages
            }
        };

        return Ok(ApiResponse<PagedResponse<EnrollmentResponse>>.SuccessResponse(pagedResponse));
    }

    /// <summary>
    /// Create a new enrollment
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<EnrollmentResponse>.ErrorResponse("Invalid request", ModelState));

        var model = new EnrollmentModel
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollDate = request.EnrollDate,
            Status = request.Status
        };

        var created = await _enrollmentService.CreateAsync(model);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<EnrollmentResponse>.SuccessResponse(MapToResponse(created), "Enrollment created successfully"));
    }

    /// <summary>
    /// Update an existing enrollment
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEnrollmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<EnrollmentResponse>.ErrorResponse("Invalid request", ModelState));

        var model = new EnrollmentModel
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollDate = request.EnrollDate,
            Status = request.Status
        };

        var updated = await _enrollmentService.UpdateAsync(id, model);
        if (updated == null)
            return NotFound(ApiResponse<EnrollmentResponse>.ErrorResponse("Enrollment not found"));

        return Ok(ApiResponse<EnrollmentResponse>.SuccessResponse(MapToResponse(updated), "Enrollment updated successfully"));
    }

    /// <summary>
    /// Delete an enrollment
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _enrollmentService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.ErrorResponse("Enrollment not found"));

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Enrollment deleted successfully"));
    }

    private static EnrollmentResponse MapToResponse(EnrollmentModel model)
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
                    SemesterName = model.Course.Semester.SemesterName
                } : null
            } : null
        };
    }

    private static Dictionary<string, object?> SelectFields(EnrollmentResponse item, HashSet<string> fields)
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
