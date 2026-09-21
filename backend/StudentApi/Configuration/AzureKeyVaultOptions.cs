namespace StudentApi.Configuration;

/// <summary>
/// Options for Azure Key Vault configuration
/// </summary>
public class AzureKeyVaultOptions
{
    public const string SectionName = "AzureKeyVault";

    /// <summary>
    /// The URL of the Azure Key Vault
    /// </summary>
    public string VaultUrl { get; set; } = string.Empty;
}
