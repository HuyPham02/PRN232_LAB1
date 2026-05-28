using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models.Request;
using PRN232.LMS.API.Models.Response;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    /// <summary>
    /// Get a student by ID with related enrollments
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Student with enrollments</returns>
    /// <response code="200">Returns the student</response>
    /// <response code="404">Student not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var model = await _studentService.GetByIdAsync(id);
        if (model == null)
            return NotFound(ApiResponse<StudentResponse>.ErrorResponse("Student not found"));

        var response = MapToResponse(model);
        return Ok(ApiResponse<StudentResponse>.SuccessResponse(response));
    }

    /// <summary>
    /// Get list of students with search, sort, paging, field selection, and expansion
    /// </summary>
    /// <param name="search">Search by name or email</param>
    /// <param name="sort">Sort fields (e.g. fullName,-dateOfBirth)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="size">Page size (default: 10, max: 50)</param>
    /// <param name="fields">Select specific fields (e.g. studentId,fullName,email)</param>
    /// <param name="expand">Include related entities (e.g. enrollments)</param>
    /// <returns>Paged list of students</returns>
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

        var result = await _studentService.GetAllAsync(query);
        var responseItems = result.Items.Select(MapToResponse).ToList();

        // Apply field selection
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
                    Page = result.Pagination.Page,
                    PageSize = result.Pagination.PageSize,
                    TotalItems = result.Pagination.TotalItems,
                    TotalPages = result.Pagination.TotalPages
                }
            };

            return Ok(ApiResponse<object>.SuccessResponse(filteredResult));
        }

        var pagedResponse = new PagedResponse<StudentResponse>
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

        return Ok(ApiResponse<PagedResponse<StudentResponse>>.SuccessResponse(pagedResponse));
    }

    /// <summary>
    /// Create a new student
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<StudentResponse>.ErrorResponse("Invalid request", ModelState));

        var model = new StudentModel
        {
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };

        var created = await _studentService.CreateAsync(model);
        var response = MapToResponse(created);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<StudentResponse>.SuccessResponse(response, "Student created successfully"));
    }

    /// <summary>
    /// Update an existing student
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<StudentResponse>.ErrorResponse("Invalid request", ModelState));

        var model = new StudentModel
        {
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };

        var updated = await _studentService.UpdateAsync(id, model);
        if (updated == null)
            return NotFound(ApiResponse<StudentResponse>.ErrorResponse("Student not found"));

        return Ok(ApiResponse<StudentResponse>.SuccessResponse(MapToResponse(updated), "Student updated successfully"));
    }

    /// <summary>
    /// Delete a student
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _studentService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.ErrorResponse("Student not found"));

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Student deleted successfully"));
    }

    private static StudentResponse MapToResponse(StudentModel model)
    {
        return new StudentResponse
        {
            StudentId = model.StudentId,
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth
        };
    }

    private static Dictionary<string, object?> SelectFields(StudentResponse item, HashSet<string> fields)
    {
        var dict = new Dictionary<string, object?>();
        if (fields.Contains("studentid")) dict["studentId"] = item.StudentId;
        if (fields.Contains("fullname")) dict["fullName"] = item.FullName;
        if (fields.Contains("email")) dict["email"] = item.Email;
        if (fields.Contains("dateofbirth")) dict["dateOfBirth"] = item.DateOfBirth;
        return dict;
    }
}
