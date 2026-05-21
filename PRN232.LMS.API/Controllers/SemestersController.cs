using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models.Request;
using PRN232.LMS.API.Models.Response;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemestersController(ISemesterService semesterService)
    {
        _semesterService = semesterService;
    }

    /// <summary>
    /// Get a semester by ID with related courses
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var model = await _semesterService.GetByIdAsync(id);
        if (model == null)
            return NotFound(ApiResponse<SemesterResponse>.ErrorResponse("Semester not found"));

        return Ok(ApiResponse<SemesterResponse>.SuccessResponse(MapToResponse(model)));
    }

    /// <summary>
    /// Get list of semesters with search, sort, paging, field selection, and expansion
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

        var result = await _semesterService.GetAllAsync(query);
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

        var pagedResponse = new PagedResponse<SemesterResponse>
        {
            Items = responseItems,
            Pagination = new PaginationResponse
            {
                Page = result.Pagination.Page, PageSize = result.Pagination.PageSize,
                TotalItems = result.Pagination.TotalItems, TotalPages = result.Pagination.TotalPages
            }
        };
        return Ok(ApiResponse<PagedResponse<SemesterResponse>>.SuccessResponse(pagedResponse));
    }

    /// <summary>
    /// Create a new semester
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSemesterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<SemesterResponse>.ErrorResponse("Invalid request", ModelState));

        var model = new SemesterModel
        {
            SemesterName = request.SemesterName,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var created = await _semesterService.CreateAsync(model);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<SemesterResponse>.SuccessResponse(MapToResponse(created), "Semester created successfully"));
    }

    /// <summary>
    /// Update an existing semester
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSemesterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<SemesterResponse>.ErrorResponse("Invalid request", ModelState));

        var model = new SemesterModel
        {
            SemesterName = request.SemesterName,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        var updated = await _semesterService.UpdateAsync(id, model);
        if (updated == null)
            return NotFound(ApiResponse<SemesterResponse>.ErrorResponse("Semester not found"));

        return Ok(ApiResponse<SemesterResponse>.SuccessResponse(MapToResponse(updated), "Semester updated successfully"));
    }

    /// <summary>
    /// Delete a semester
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _semesterService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.ErrorResponse("Semester not found"));

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Semester deleted successfully"));
    }

    private static SemesterResponse MapToResponse(SemesterModel model)
    {
        return new SemesterResponse
        {
            SemesterId = model.SemesterId,
            SemesterName = model.SemesterName,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Courses = model.Courses?.Any() == true
                ? model.Courses.Select(c => new CourseResponse
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    SemesterId = c.SemesterId,
                    SubjectId = c.SubjectId
                }).ToList()
                : null
        };
    }

    private static Dictionary<string, object?> SelectFields(SemesterResponse item, HashSet<string> fields)
    {
        var dict = new Dictionary<string, object?>();
        if (fields.Contains("semesterid")) dict["semesterId"] = item.SemesterId;
        if (fields.Contains("semestername")) dict["semesterName"] = item.SemesterName;
        if (fields.Contains("startdate")) dict["startDate"] = item.StartDate;
        if (fields.Contains("enddate")) dict["endDate"] = item.EndDate;
        if (fields.Contains("courses")) dict["courses"] = item.Courses;
        return dict;
    }
}
