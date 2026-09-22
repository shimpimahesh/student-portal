using StudentApi.Application.Common;
using StudentApi.Application.Common;
using StudentApi.Application.Features.Students;
using StudentApi.Domain.Entities;
using StudentApi.Domain.Factories;
using Microsoft.Extensions.Logging.Abstractions;

namespace StudentApi.Tests.Application;

public sealed class StudentHandlersTests
{
    [Fact]
    public async Task GetStudentsHandler_maps_students_to_dtos()
    {
        var repository = new FakeStudentRepository
        {
            Students =
            [
                Student.Create(" Alice ", new DateOnly(2000, 1, 2), " 555-0100 "),
                Student.Create("Bob", new DateOnly(2001, 3, 4), "555-0101")
            ]
        };

        var result = await new GetStudentsHandler(repository, NullLogger<GetStudentsHandler>.Instance)
            .Handle(new GetStudentsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Alice", result[0].Name);
        Assert.Equal(new DateOnly(2000, 1, 2), result[0].DateOfBirth);
        Assert.Equal("555-0100", result[0].Phone);
    }

    [Fact]
    public async Task GetStudentHandler_returns_null_when_student_does_not_exist()
    {
        var result = await new GetStudentHandler(new FakeStudentRepository(), NullLogger<GetStudentHandler>.Instance)
            .Handle(new GetStudentQuery(42), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetStudentHandler_maps_existing_student_to_dto()
    {
        var student = Student.Create("Alice", new DateOnly(2000, 1, 2), "555-0100");
        var repository = new FakeStudentRepository { Students = [student] };

        var result = await new GetStudentHandler(repository, NullLogger<GetStudentHandler>.Instance)
            .Handle(new GetStudentQuery(student.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(student.Name, result.Name);
        Assert.Equal(student.DateOfBirth, result.DateOfBirth);
        Assert.Equal(student.Phone, result.Phone);
    }

    [Fact]
    public async Task GetStudentsHandler_returns_empty_list_when_repository_is_empty()
    {
        var result = await new GetStudentsHandler(new FakeStudentRepository(), NullLogger<GetStudentsHandler>.Instance)
            .Handle(new GetStudentsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateStudentHandler_adds_student_saves_changes_and_returns_dto()
    {
        var repository = new FakeStudentRepository();
        var dateOfBirth = new DateOnly(1999, 12, 31);

        var result = await new CreateStudentHandler(repository, new StudentFactory(), NullLogger<CreateStudentHandler>.Instance)
            .Handle(new CreateStudentCommand(" Alice ", dateOfBirth, " 555-0100 "), CancellationToken.None);

        var student = Assert.Single(repository.AddedStudents);
        Assert.Same(student, repository.SavedStudent);
        Assert.Equal("Alice", result.Name);
        Assert.Equal(dateOfBirth, result.DateOfBirth);
        Assert.Equal("555-0100", result.Phone);
    }

    [Fact]
    public async Task DeleteStudentHandler_returns_false_without_saving_when_student_does_not_exist()
    {
        var repository = new FakeStudentRepository();

        var result = await new DeleteStudentHandler(repository, NullLogger<DeleteStudentHandler>.Instance)
            .Handle(new DeleteStudentCommand(42), CancellationToken.None);

        Assert.False(result);
        Assert.Null(repository.SavedStudent);
        Assert.Empty(repository.RemovedStudents);
    }

    [Fact]
    public async Task DeleteStudentHandler_removes_existing_student_and_saves_changes()
    {
        var student = Student.Create("Alice", new DateOnly(2000, 1, 2), "555-0100");
        var repository = new FakeStudentRepository { Students = [student] };

        var result = await new DeleteStudentHandler(repository, NullLogger<DeleteStudentHandler>.Instance)
            .Handle(new DeleteStudentCommand(student.Id), CancellationToken.None);

        Assert.True(result);
        Assert.Contains(student, repository.RemovedStudents);
        Assert.Same(student, repository.SavedStudent);
    }

    private sealed class FakeStudentRepository : IStudentRepository
    {
        public IReadOnlyList<Student> Students { get; set; } = [];
        public List<Student> AddedStudents { get; } = [];
        public List<Student> RemovedStudents { get; } = [];
        public Student? SavedStudent { get; private set; }

        public Task<IReadOnlyList<Student>> GetAllAsync(CancellationToken cancellationToken)
            => Task.FromResult(Students);

        public Task<Student?> GetByIdAsync(int id, CancellationToken cancellationToken)
            => Task.FromResult(Students.FirstOrDefault(student => student.Id == id));

        public Task AddAsync(Student student, CancellationToken cancellationToken)
        {
            AddedStudents.Add(student);
            return Task.CompletedTask;
        }

        public void Remove(Student student) => RemovedStudents.Add(student);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SavedStudent = AddedStudents.LastOrDefault() ?? RemovedStudents.LastOrDefault();
            return Task.CompletedTask;
        }
    }
}
