using Microsoft.Extensions.Configuration;

public class AwsSecretsConfigurationSource : IConfigurationSource
{
    private readonly string _region;
    private readonly string _secretName;

    public AwsSecretsConfigurationSource(string region, string secretName)
    {
        _region = region;
        _secretName = secretName;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new AwsSecretsConfigurationProvider(_region, _secretName);
    }
}
