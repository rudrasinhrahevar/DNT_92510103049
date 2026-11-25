using Microsoft.AspNetCore.Mvc;
using StudentManagementApi.Data;
using StudentManagementApi.Models;

namespace StudentManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StudentsController(StudentRepository repository) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<Student>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Student>>> GetAll([FromQuery] string? search)
    {
        return Ok(await repository.GetAllAsync(search));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<Student>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Student>> GetById(int id)
    {
        var student = await repository.GetByIdAsync(id);
        return student is null ? NotFound() : Ok(student);
    }

    [HttpPost]
    [ProducesResponseType<Student>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Student>> Create(StudentRequest request)
    {
        var student = await repository.CreateAsync(request);
        if (student is null)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Student already exists",
                Detail = "The student number and email address must both be unique.",
                Status = StatusCodes.Status409Conflict
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<Student>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Student>> Update(int id, StudentRequest request)
    {
        var student = await repository.UpdateAsync(id, request);
        if (student is not null)
        {
            return Ok(student);
        }

        if (await repository.GetByIdAsync(id) is null)
        {
            return NotFound();
        }

        return Conflict(new ProblemDetails
        {
            Title = "Student already exists",
            Detail = "The student number and email address must both be unique.",
            Status = StatusCodes.Status409Conflict
        });
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        return await repository.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
