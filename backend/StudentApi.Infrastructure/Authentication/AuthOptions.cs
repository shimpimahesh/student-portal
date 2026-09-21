using System.ComponentModel.DataAnnotations;

namespace StudentApi.Infrastructure.Authentication;


public sealed class AzureAdOptions
{
    public const string SectionName = "AzureAd";

    [Required]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string Scope { get; set; } = "User.Read";
}
