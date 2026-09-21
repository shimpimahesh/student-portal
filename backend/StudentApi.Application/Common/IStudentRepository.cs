using StudentApi.Domain.Entities;

namespace StudentApi.Application.Common;

public interface IStudentRepository
{
    Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken);
    Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Student student, CancellationToken cancellationToken);
    void Remove(Student student);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
