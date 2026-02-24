using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

public class AwsSecretsConfigurationProvider : ConfigurationProvider
{
    private readonly string _region;
    private readonly string _secretName;

    public AwsSecretsConfigurationProvider(string region, string secretName)
    {
        _region = region;
        _secretName = secretName;
    }

    public override void Load()
    {
        LoadAsync().GetAwaiter().GetResult();
    }

    private async Task LoadAsync()
    {
        var client = new AmazonSecretsManagerClient(
            Amazon.RegionEndpoint.GetBySystemName(_region));

        var response = await client.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = _secretName
        });

        if (string.IsNullOrEmpty(response.SecretString))
            return;

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(response.SecretString);

        if (dict != null)
        {
            Data = FlattenJson(dict);
        }
    }

    // 🔥 Supports nested JSON (very important)
    private static Dictionary<string, string> FlattenJson(
        Dictionary<string, object> dict,
        string parentKey = "")
    {
        var result = new Dictionary<string, string>();

        foreach (var kvp in dict)
        {
            var key = string.IsNullOrEmpty(parentKey)
                ? kvp.Key
                : $"{parentKey}:{kvp.Key}";

            if (kvp.Value is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    var childDict = JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText());
                    var child = FlattenJson(childDict!, key);
                    foreach (var c in child)
                        result[c.Key] = c.Value;
                }
                else
                {
                    result[key] = element.ToString();
                }
            }
        }

        return result;
    }
}
