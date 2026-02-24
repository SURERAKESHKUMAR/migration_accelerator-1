using migration_accelerator.Models;

namespace migration_accelerator.Services
{
    public class TxtSecretReader
    {
        public static List<SecretItem> ReadFromStream(Stream stream)
        {
            var list = new List<SecretItem>();

            using var reader = new StreamReader(stream);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    list.Add(new SecretItem
                    {
                        Key = parts[0].Trim(),
                        Value = parts[1].Trim()
                    });
                }
            }

            return list;
        }
    }
}
