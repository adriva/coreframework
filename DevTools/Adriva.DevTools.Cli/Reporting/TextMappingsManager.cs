using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Humanizer;

namespace Adriva.DevTools.Cli.Reporting
{
    internal class TextMappingsManager
    {
        private readonly FileInfo MappingsFileInfo;
        private readonly Dictionary<string, string> CaseInsensitiveLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> CaseSensitiveLookup = new Dictionary<string, string>(StringComparer.Ordinal);

        public TextMappingsManager(FileInfo mappingsFileInfo)
        {
            this.MappingsFileInfo = mappingsFileInfo;
        }

        public async Task InitializeAsync()
        {
            if (null == this.MappingsFileInfo)
            {
                return;
            }
            else if (!this.MappingsFileInfo.Exists)
            {
                await Console.Out.WriteLineAsync($"Specified mappings file '{this.MappingsFileInfo.FullName}' could not be found.");
            }
            else
            {
                using (var fileStream = this.MappingsFileInfo.OpenRead())
                using (var reader = new StreamReader(fileStream))
                {
                    string line = null;

                    do
                    {
                        line = await reader.ReadLineAsync();

                        if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
                        {
                            var match = Regex.Match(line, @"(\""(?<fromText>.+)\"")\s*\:\s*(\""(?<toText>.+)\"")((\s*\:\s*(?<sensitivity>(CS|CI))){0,1})");

                            if (match.Success && match.Groups["fromText"].Success && match.Groups["toText"].Success)
                            {
                                string fromText = match.Groups["fromText"].Value;
                                string toText = match.Groups["toText"].Value;
                                string sensitivity = match.Groups["sensitivity"].Success ? match.Groups["sensitivity"].Value : null;

                                Dictionary<string, string> targetDictionary = null;

                                if ("CS".Equals(sensitivity, StringComparison.OrdinalIgnoreCase))
                                {
                                    targetDictionary = this.CaseSensitiveLookup;
                                }
                                else
                                {
                                    targetDictionary = this.CaseInsensitiveLookup;
                                }

                                if (!string.IsNullOrWhiteSpace(fromText))
                                {
                                    if ("null".Equals(toText, StringComparison.OrdinalIgnoreCase))
                                    {
                                        toText = null;
                                    }
                                    targetDictionary.TryAdd(fromText, toText);
                                }
                            }
                        }
                    } while (null != line);
                }
            }
        }

        public string GetSubstitution(string originalText, bool useCamelCase = false)
        {
            if (string.IsNullOrWhiteSpace(originalText)) return originalText;

            bool hasMatch = false;

            if (!this.CaseSensitiveLookup.TryGetValue(originalText, out string replacementText))
            {
                hasMatch = this.CaseInsensitiveLookup.TryGetValue(originalText, out replacementText);
            }
            else
            {
                hasMatch = true;
            }

            string output = hasMatch ? replacementText : originalText;

            return useCamelCase switch
            {
                true => output.Camelize(),
                false => output
            };
        }
    }
}