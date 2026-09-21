namespace StudentApi.Domain.Entities;

public sealed class Student
{
    private Student() { }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public string Phone { get; private set; } = string.Empty;

    public static Student Create(string name, DateOnly dateOfBirth, string phone)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(phone)) throw new ArgumentException("Phone is required.", nameof(phone));
        return new Student { Name = name.Trim(), DateOfBirth = dateOfBirth, Phone = phone.Trim() };
    }
}
