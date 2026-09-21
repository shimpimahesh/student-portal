using System.ComponentModel.DataAnnotations;
using MediatR;

namespace StudentApi.Application.Features.Students;

public sealed record StudentDto(int Id, string Name, DateOnly DateOfBirth, string Phone);
public sealed record CreateStudentRequest(
    [property: Required, StringLength(100, MinimumLength = 2)] string Name,
    [property: Required] DateOnly DateOfBirth,
    [property: Required, Phone] string Phone);
public sealed record GetStudentsQuery : IRequest<IReadOnlyList<StudentDto>>;
public sealed record GetStudentQuery(int Id) : IRequest<StudentDto?>;
public sealed record CreateStudentCommand(string Name, DateOnly DateOfBirth, string Phone) : IRequest<StudentDto>;
public sealed record DeleteStudentCommand(int Id) : IRequest<bool>;
