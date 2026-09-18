using System.ComponentModel.DataAnnotations;

namespace StudentApi.DTOs;

public record StudentReadDto(int Id, string Name, DateOnly DateOfBirth, string Phone);

public record StudentCreateDto(
    [Required(ErrorMessage = "Name is required")]
[StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
string Name,

    [Required(ErrorMessage = "Date of Birth is required")]
DateOnly DateOfBirth,

    [Required(ErrorMessage = "Phone number is required")]
[Phone(ErrorMessage = "Invalid phone number format")]
string Phone
);
