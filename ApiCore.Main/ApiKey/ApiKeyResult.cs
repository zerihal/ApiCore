namespace ApiCore.Main.ApiKey
{
    /// <summary>
    /// API key validation result.
    /// </summary>
    /// <param name="IsValid">Indicates whether the key is active or not (also false if not found).</param>
    /// <param name="Owner">API key owner.</param>
    /// <param name="KeyType">API key type.</param>
    public record ApiKeyResult (bool IsValid, string? Owner, int KeyType);
}
