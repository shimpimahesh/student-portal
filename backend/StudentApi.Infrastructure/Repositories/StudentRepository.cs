using Microsoft.EntityFrameworkCore;
using StudentApi.Application.Common;
using StudentApi.Domain.Entities;
using StudentApi.Infrastructure.Data;

namespace StudentApi.Infrastructure.Repositories;

public sealed class StudentRepository(AppDbContext context) : IStudentRepository
{
    public async Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken)
        => await context.Students.AsNoTracking().OrderByDescending(student => student.Id).ToListAsync(cancellationToken);

    public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => context.Students.FirstOrDefaultAsync(student => student.Id == id, cancellationToken);

    public Task AddAsync(Student student, CancellationToken cancellationToken)
    {
        context.Students.Add(student);
        return Task.CompletedTask;
    }

    public void Remove(Student student) => context.Students.Remove(student);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
