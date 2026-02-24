using Amazon.SecretsManager;
using migration_accelerator.Models;
using Amazon.SecretsManager.Model;
using System.Text.Json;

namespace migration_accelerator.Services
{
    public class AwsSecretsService
    {

        private readonly IAmazonSecretsManager _client;

        public AwsSecretsService(IAmazonSecretsManager client)
        {
            _client = client;
        }

        public async Task SaveAsync(string secretName, List<SecretItem> items)
        {
            var json = JsonSerializer.Serialize(
                items.ToDictionary(x => x.Key, x => x.Value));

            try
            {
                await _client.PutSecretValueAsync(new PutSecretValueRequest
                {
                    SecretId = secretName,
                    SecretString = json
                });
            }
            catch (ResourceNotFoundException)
            {
                await _client.CreateSecretAsync(new CreateSecretRequest
                {
                    Name = secretName,
                    SecretString = json
                });
            }
        }

        // 🔹 Get all secret names
        public async Task<List<string>> GetAllSecretNamesAsync()
        {
            var result = new List<string>();
            string? nextToken = null;

            do
            {
                var response = await _client.ListSecretsAsync(
                    new ListSecretsRequest
                    {
                        NextToken = nextToken
                    });

                result.AddRange(response.SecretList.Select(s => s.Name));
                nextToken = response.NextToken;

            } while (!string.IsNullOrEmpty(nextToken));

            return result.OrderBy(x => x).ToList();
        }

        // 🔹 Get key/value secrets
        public async Task<Dictionary<string, string>> GetSecretsAsync(string secretName)
        {
            var response = await _client.GetSecretValueAsync(
                new GetSecretValueRequest
                {
                    SecretId = secretName.Trim()
                });

            return System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, string>>(response.SecretString!)!;
        }
    }
}
