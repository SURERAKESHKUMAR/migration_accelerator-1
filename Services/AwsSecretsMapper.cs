public static class AwsSecretsMapper
{
    public static void Apply(IConfiguration config)
    {
        foreach (var item in config.AsEnumerable())
        {
            if (string.IsNullOrWhiteSpace(item.Value))
                continue;

            // Map connection string
            if (item.Key.Equals("AwsSecreteConnection", StringComparison.OrdinalIgnoreCase))
            {
                config["ConnectionStrings:DefaultConnection"] = item.Value;
                continue;
            }

            // Replace placeholders in AppSettings
            var appSettingKey = $"AppSettings:{item.Key}";

            if (config[appSettingKey] != null)
            {
                config[appSettingKey] = item.Value;
            }
        }
    }
}
