using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentApi.Application.Features.Students;

namespace StudentApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class StudentsController(ISender sender, ILogger<StudentsController> logger) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StudentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> GetAll(CancellationToken cancellationToken)
    {
        var students = await sender.Send(new GetStudentsQuery(), cancellationToken);
        logger.LogInformation("Retrieved {StudentCount} students", students.Count);
        return Ok(students);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var student = await sender.Send(new GetStudentQuery(id), cancellationToken);
        if (student is null)
        {
            logger.LogWarning("Student with ID {StudentId} was not found", id);
        }

        return student is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Student not found",
                Detail = $"Student with ID {id} not found."
            })
            : Ok(student);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<StudentDto>> Create([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateStudentCommand(request.Name, request.DateOfBirth, request.Phone), cancellationToken);
        logger.LogInformation("Created student with ID {StudentId}", result.Id);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await sender.Send(new DeleteStudentCommand(id), cancellationToken);
        if (!deleted)
        {
            logger.LogWarning("Student with ID {StudentId} was not found for deletion", id);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Student not found",
                Detail = $"Student with ID {id} not found."
            });
        }

        logger.LogInformation("Deleted student with ID {StudentId}", id);
        return NoContent();
    }
}
