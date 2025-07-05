namespace Jackson.Ideas.Core.Interfaces.AI;

public interface IBaseAIProvider
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default, params object[] args);
    Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default);
    Task<bool> ValidateApiKeyAsync(CancellationToken cancellationToken = default);
    string ProviderType { get; }
    string Model { get; }
}