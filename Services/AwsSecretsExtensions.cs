using Microsoft.Extensions.Configuration;

public static class AwsSecretsExtensions
{
    public static IConfigurationBuilder AddAwsSecrets(
        this IConfigurationBuilder builder,
        string region,
        string secretName)
    {
        return builder.Add(new AwsSecretsConfigurationSource(region, secretName));
    }
}
