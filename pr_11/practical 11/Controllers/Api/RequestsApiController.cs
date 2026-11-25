using Microsoft.AspNetCore.Mvc;
using SmartCampusPortal.Data;
using SmartCampusPortal.Models;

namespace SmartCampusPortal.Controllers.Api;

[ApiController]
[Route("api/requests")]
public sealed class RequestsApiController(ServiceRequestRepository repository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ServiceRequest>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceRequest>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? status)
    {
        if (!string.IsNullOrWhiteSpace(status) && !ServiceRequestStatuses.IsValid(status))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid status",
                Detail = $"Status must be one of: {string.Join(", ", ServiceRequestStatuses.All)}."
            });
        }

        return Ok(await repository.GetRequestsAsync(search, status));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ServiceRequest>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequest>> GetById(int id)
    {
        var request = await repository.GetByIdAsync(id);
        return request is null ? NotFound() : Ok(request);
    }

    [HttpPost]
    [ProducesResponseType<ServiceRequest>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceRequest>> Create(ServiceRequestForm request)
    {
        var id = await repository.CreateAsync(request);
        var createdRequest = await repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("The newly created request could not be loaded.");
        return CreatedAtAction(nameof(GetById), new { id }, createdRequest);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType<ServiceRequest>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequest>> UpdateStatus(int id, UpdateRequestStatus update)
    {
        if (!ServiceRequestStatuses.IsValid(update.Status))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid status",
                Detail = $"Status must be one of: {string.Join(", ", ServiceRequestStatuses.All)}."
            });
        }

        if (!await repository.UpdateStatusAsync(id, update.Status))
        {
            return NotFound();
        }

        return Ok(await repository.GetByIdAsync(id));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        return await repository.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
