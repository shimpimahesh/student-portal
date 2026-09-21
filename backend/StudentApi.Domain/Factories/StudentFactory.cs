using StudentApi.Domain.Entities;

namespace StudentApi.Domain.Factories;

public sealed class StudentFactory
{
    public Student Create(string name, DateOnly dateOfBirth, string phone)
        => Student.Create(name, dateOfBirth, phone);
}
