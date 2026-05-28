using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Models.Request;
using PRN232.LMS.API.Models.Response;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    /// <summary>
    /// Get a subject by ID with related courses
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var model = await _subjectService.GetByIdAsync(id);
        if (model == null)
            return NotFound(ApiResponse<SubjectResponse>.ErrorResponse("Subject not found"));

        return Ok(ApiResponse<SubjectResponse>.SuccessResponse(MapToResponse(model)));
    }

    /// <summary>
    /// Get list of subjects with search, sort, paging, field selection, and expansion
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

        var result = await _subjectService.GetAllAsync(query);
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

        var pagedResponse = new PagedResponse<SubjectResponse>
        {
            Items = responseItems,
            Pagination = new PaginationResponse
            {
                Page = result.Pagination.Page, PageSize = result.Pagination.PageSize,
                TotalItems = result.Pagination.TotalItems, TotalPages = result.Pagination.TotalPages
            }
        };
        return Ok(ApiResponse<PagedResponse<SubjectResponse>>.SuccessResponse(pagedResponse));
    }

    /// <summary>
    /// Create a new subject
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<SubjectResponse>.ErrorResponse("Invalid request", ModelState));

        var model = new SubjectModel
        {
            SubjectCode = request.SubjectCode,
            SubjectName = request.SubjectName,
            Credit = request.Credit
        };

        var created = await _subjectService.CreateAsync(model);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<SubjectResponse>.SuccessResponse(MapToResponse(created), "Subject created successfully"));
    }

    /// <summary>
    /// Update an existing subject
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<SubjectResponse>.ErrorResponse("Invalid request", ModelState));

        var model = new SubjectModel
        {
            SubjectCode = request.SubjectCode,
            SubjectName = request.SubjectName,
            Credit = request.Credit
        };

        var updated = await _subjectService.UpdateAsync(id, model);
        if (updated == null)
            return NotFound(ApiResponse<SubjectResponse>.ErrorResponse("Subject not found"));

        return Ok(ApiResponse<SubjectResponse>.SuccessResponse(MapToResponse(updated), "Subject updated successfully"));
    }

    /// <summary>
    /// Delete a subject
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _subjectService.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.ErrorResponse("Subject not found"));

        return Ok(ApiResponse<object>.SuccessResponse(new { }, "Subject deleted successfully"));
    }

    private static SubjectResponse MapToResponse(SubjectModel model)
    {
        return new SubjectResponse
        {
            SubjectId = model.SubjectId,
            SubjectCode = model.SubjectCode,
            SubjectName = model.SubjectName,
            Credit = model.Credit
        };
    }

    private static Dictionary<string, object?> SelectFields(SubjectResponse item, HashSet<string> fields)
    {
        var dict = new Dictionary<string, object?>();
        if (fields.Contains("subjectid")) dict["subjectId"] = item.SubjectId;
        if (fields.Contains("subjectcode")) dict["subjectCode"] = item.SubjectCode;
        if (fields.Contains("subjectname")) dict["subjectName"] = item.SubjectName;
        if (fields.Contains("credit")) dict["credit"] = item.Credit;
        return dict;
    }
}
