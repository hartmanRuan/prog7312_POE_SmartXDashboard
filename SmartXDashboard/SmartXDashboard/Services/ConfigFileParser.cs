using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SmartXDashboard.Services
{
    public class ConfigFileParser
    {
        public class ConfigMetadata
        {
            public string FileName { get; set; } = string.Empty;
            public long FileSizeKB { get; set; }
            public int PropertyCount { get; set; }
            public Dictionary<string, string> KeyValues { get; set; } = new Dictionary<string, string>();
            public bool IsValid { get; set; }
        }

        public ConfigMetadata ParseFile(string filePath)
        {
            var metadata = new ConfigMetadata();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                metadata.IsValid = false;
                return metadata;
            }

            try
            {
                FileInfo info = new FileInfo(filePath);
                metadata.FileName = info.Name;
                metadata.FileSizeKB = info.Length / 1024;
                string extension = info.Extension.ToLower();

                string content = File.ReadAllText(filePath);

                if (extension == ".json")
                {
                    using (JsonDocument doc = JsonDocument.Parse(content))
                    {
                        foreach (JsonProperty element in doc.RootElement.EnumerateObject())
                        {
                            metadata.KeyValues[element.Name] = element.Value.ToString();
                        }
                    }
                }
                else
                {
                    // Handle .txt or .log line-by-line (Key=Value format)
                    string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines)
                    {
                        if (line.Contains("="))
                        {
                            string[] parts = line.Split(new[] { '=' }, 2);
                            metadata.KeyValues[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                }

                metadata.PropertyCount = metadata.KeyValues.Count;
                metadata.IsValid = true;
            }
            catch (Exception)
            {
                metadata.IsValid = false;
            }

            return metadata;
        }
    }
}