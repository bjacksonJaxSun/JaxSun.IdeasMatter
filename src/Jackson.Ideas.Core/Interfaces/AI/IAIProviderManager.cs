using Jackson.Ideas.Core.Entities;
using Jackson.Ideas.Core.Enums;

namespace Jackson.Ideas.Core.Interfaces.AI;

public interface IAIProviderManager
{
    string EncryptApiKey(string apiKey);
    string DecryptApiKey(string encryptedKey);
    Task<IBaseAIProvider> LoadProviderAsync(AIProviderConfig providerConfig, CancellationToken cancellationToken = default);
    Task<IBaseAIProvider> GetProviderAsync(string providerId, CancellationToken cancellationToken = default);
    Task<IBaseAIProvider> GetProviderAsync(AIProviderType providerType, CancellationToken cancellationToken = default);
    Task<bool> TestProviderAsync(IBaseAIProvider provider, CancellationToken cancellationToken = default);
}