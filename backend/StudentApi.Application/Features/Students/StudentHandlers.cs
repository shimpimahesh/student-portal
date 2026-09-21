using MediatR;
using StudentApi.Application.Common;
using StudentApi.Domain.Entities;
using StudentApi.Domain.Factories;

namespace StudentApi.Application.Features.Students;

public sealed class GetStudentsHandler(IStudentRepository repository) : IRequestHandler<GetStudentsQuery, IReadOnlyList<StudentDto>>
{
    public async Task<IReadOnlyList<StudentDto>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
        => (await repository.GetAllAsync(cancellationToken)).Select(Map).ToList();

    private static StudentDto Map(Student student) => new(student.Id, student.Name, student.DateOfBirth, student.Phone);
}

public sealed class GetStudentHandler(IStudentRepository repository) : IRequestHandler<GetStudentQuery, StudentDto?>
{
    public async Task<StudentDto?> Handle(GetStudentQuery request, CancellationToken cancellationToken)
    {
        var student = await repository.GetByIdAsync(request.Id, cancellationToken);
        return student is null ? null : new(student.Id, student.Name, student.DateOfBirth, student.Phone);
    }
}

public sealed class CreateStudentHandler(IStudentRepository repository, StudentFactory studentFactory) : IRequestHandler<CreateStudentCommand, StudentDto>
{
    public async Task<StudentDto> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = studentFactory.Create(request.Name, request.DateOfBirth, request.Phone);
        await repository.AddAsync(student, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return new(student.Id, student.Name, student.DateOfBirth, student.Phone);
    }
}

public sealed class DeleteStudentHandler(IStudentRepository repository) : IRequestHandler<DeleteStudentCommand, bool>
{
    public async Task<bool> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (student is null) return false;
        repository.Remove(student);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
