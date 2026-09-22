using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentApi.Application.Common;
using StudentApi.Domain.Entities;
using StudentApi.Infrastructure.Data;

namespace StudentApi.Infrastructure.Repositories;

public sealed class StudentRepository(AppDbContext context, ILogger<StudentRepository> logger) : IStudentRepository
{
    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken)
    {
        var students = await context.Students.AsNoTracking().OrderByDescending(student => student.Id).ToListAsync(cancellationToken);
        logger.LogInformation("Retrieved {StudentCount} students from the database", students.Count);
        return students;
    }

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving student with ID {StudentId}", id);
        return context.Students.FirstOrDefaultAsync(student => student.Id == id, cancellationToken);
    }

    public Task AddAsync(Student student, CancellationToken cancellationToken)
    {
        context.Students.Add(student);
        logger.LogInformation("Adding student to the database");
        return Task.CompletedTask;
    }

    public void Remove(Student student)
    {
        context.Students.Remove(student);
        logger.LogInformation("Removing student with ID {StudentId} from the database", student.Id);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Saved student changes to the database");
    }
}
