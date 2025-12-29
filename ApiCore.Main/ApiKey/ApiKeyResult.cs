namespace ApiCore.Main.ApiKey
{
    public record ApiKeyResult (bool IsValid, string? Owner, int KeyType);
}
