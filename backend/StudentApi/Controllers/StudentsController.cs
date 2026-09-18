using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApi.Data;
using StudentApi.DTOs;
using StudentApi.Models;

namespace StudentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(AppDbContext context, ILogger<StudentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudentReadDto>>> GetAll(CancellationToken ct)
    {
        var students = await _context.Students
            .AsNoTracking()
            .OrderByDescending(s => s.Id)
            .Select(s => new StudentReadDto(s.Id, s.Name, s.DateOfBirth, s.Phone))
            .ToListAsync(ct);

        return Ok(students);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentReadDto>> GetById(int id, CancellationToken ct)
    {
        var student = await _context.Students.FindAsync(new object[] { id }, ct);
        if (student == null) return NotFound(new { message = $"Student with ID {id} not found." });

        return Ok(new StudentReadDto(student.Id, student.Name, student.DateOfBirth, student.Phone));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StudentReadDto>> Create([FromBody] StudentCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var student = new Student
        {
            Name = dto.Name.Trim(),
            DateOfBirth = dto.DateOfBirth,
            Phone = dto.Phone.Trim()
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync(ct);

        var resultDto = new StudentReadDto(student.Id, student.Name, student.DateOfBirth, student.Phone);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, resultDto);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var student = await _context.Students.FindAsync(new object[] { id }, ct);
        if (student == null) return NotFound(new { message = $"Student with ID {id} not found." });

        _context.Students.Remove(student);
        await _context.SaveChangesAsync(ct);

        return NoContent();
    }
}
