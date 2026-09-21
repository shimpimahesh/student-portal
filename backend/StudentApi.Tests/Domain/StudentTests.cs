using StudentApi.Domain.Entities;

namespace StudentApi.Tests.Domain;

public sealed class StudentTests
{
    [Fact]
    public void Create_trims_values_and_creates_student()
    {
        var student = Student.Create(" Alice ", new DateOnly(2000, 1, 2), " 555-0100 ");

        Assert.Equal("Alice", student.Name);
        Assert.Equal(new DateOnly(2000, 1, 2), student.DateOfBirth);
        Assert.Equal("555-0100", student.Phone);
    }

    [Theory]
    [InlineData("", "555-0100", "name")]
    [InlineData("   ", "555-0100", "name")]
    [InlineData("Alice", "", "phone")]
    [InlineData("Alice", "   ", "phone")]
    public void Create_rejects_missing_required_values(string name, string phone, string parameterName)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            Student.Create(name, new DateOnly(2000, 1, 2), phone));

        Assert.Equal(parameterName, exception.ParamName);
    }
}
