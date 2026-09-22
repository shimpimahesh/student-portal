using MediatR;
using Microsoft.Extensions.Logging;
using StudentApi.Application.Common;
using StudentApi.Domain.Entities;
using StudentApi.Domain.Factories;

namespace StudentApi.Application.Features.Students;

public sealed class GetStudentsHandler(IStudentRepository repository, ILogger<GetStudentsHandler> logger) : IRequestHandler<GetStudentsQuery, IReadOnlyList<StudentDto>>
{
    public async Task<IReadOnlyList<StudentDto>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = (await repository.GetAllAsync(cancellationToken)).Select(Map).ToList();
        logger.LogInformation("Retrieved {StudentCount} students from the repository", students.Count);
        return students;
    }

    private static StudentDto Map(Student student) => new(student.Id, student.Name, student.DateOfBirth, student.Phone);
}

public sealed class GetStudentHandler(IStudentRepository repository, ILogger<GetStudentHandler> logger) : IRequestHandler<GetStudentQuery, StudentDto?>
{
    public async Task<StudentDto?> Handle(GetStudentQuery request, CancellationToken cancellationToken)
    {
        var student = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (student is null)
        {
            logger.LogWarning("Student with ID {StudentId} was not found", request.Id);
        }

        return student is null ? null : new(student.Id, student.Name, student.DateOfBirth, student.Phone);
    }
}

public sealed class CreateStudentHandler(IStudentRepository repository, StudentFactory studentFactory, ILogger<CreateStudentHandler> logger) : IRequestHandler<CreateStudentCommand, StudentDto>
{
    public async Task<StudentDto> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = studentFactory.Create(request.Name, request.DateOfBirth, request.Phone);
        await repository.AddAsync(student, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created student with ID {StudentId}", student.Id);
        return new(student.Id, student.Name, student.DateOfBirth, student.Phone);
    }
}

public sealed class DeleteStudentHandler(IStudentRepository repository, ILogger<DeleteStudentHandler> logger) : IRequestHandler<DeleteStudentCommand, bool>
{
    public async Task<bool> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (student is null)
        {
            logger.LogWarning("Student with ID {StudentId} was not found for deletion", request.Id);
            return false;
        }

        repository.Remove(student);
        await repository.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Deleted student with ID {StudentId}", request.Id);
        return true;
    }
}
