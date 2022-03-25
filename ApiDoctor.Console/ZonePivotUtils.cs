/*
 * API Doctor
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the ""Software""), to deal in
 * the Software without restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
 * Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace ApiDoctor.ConsoleApp
{

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides a wrapper around processes involving the GIT app
    /// </summary>
    class ZonePivotUtils
    {
        private const string httpTab = "# [HTTP](#tab/http)";
        private const string csharpTab = "# [C#](#tab/csharp)";
        private const string jsTab = "# [JavaScript](#tab/javascript)";
        private const string objCTab = "# [Objective-C](#tab/objc)";
        private const string javaTab = "# [Java](#tab/java)";
        private const string goTab = "# [Go](#tab/go)";
        private const string psTab = "# [PowerShell](#tab/powershell)";

        private static Dictionary<string, string> zones = new Dictionary<string, string>()
        {
            { httpTab, "programming-language-curl" },
            { csharpTab, "programming-language-csharp" },
            { jsTab, "programming-language-browserjs" },
            { objCTab, "programming-language-objectivec" },
            { javaTab, "programming-language-java" },
            { goTab, "programming-language-go" },
            { psTab, "programming-language-powershell" },
        };

        public const string ZoneEnd = "::: zone-end";
        private const string zonePivotGroup = "zone_pivot_groups: graph-sdk-languages";

        public static string[] ConvertTabbedToZonePivots(string[] original)
        {
            var updatedLines = new List<string>(original);

            if (updatedLines.FindIndex(0, s => s.Contains(zonePivotGroup, StringComparison.InvariantCultureIgnoreCase)) < 0)
            {
                // Insert metadata to enable zone pivots
                var yamlBlockStart = updatedLines.FindIndex(0, s => s.Trim().Equals("---", StringComparison.InvariantCulture));
                var yamlBlockEnd = updatedLines.FindIndex(yamlBlockStart + 1, s => s.Trim().Equals("---", StringComparison.InvariantCulture));
                updatedLines.Insert(yamlBlockEnd, zonePivotGroup);
            }

            var currentIndex = updatedLines.FindIndex(0, s => s.Trim().Equals(httpTab, StringComparison.InvariantCultureIgnoreCase));

            while (currentIndex >= 0)
            {
                // Find end of tabbed block
                var endIndex = updatedLines.FindIndex(currentIndex, s => s.Trim().Equals("---"));
                if (endIndex < 0)
                {
                    // End of block marker wasn't found.
                    // Technically this is an error in the doc,
                    // but we can attempt to recover.
                    endIndex = updatedLines.FindIndex(currentIndex, s => s.Contains("# Response", StringComparison.InvariantCultureIgnoreCase));
                    endIndex--;
                }

                // Create new pivot block
                var pivotBlock = new List<string>();
                for (var i = currentIndex; i <= endIndex; i++)
                {
                    if (updatedLines[i].Trim().StartsWith('#'))
                    {
                        pivotBlock.Add(MapTabToZone(updatedLines[i]));
                        if (updatedLines[i].Contains("#tab/http", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // HTTP block should end with closing of code fence
                            var codeFenceStart = updatedLines.FindIndex(i, s => s.StartsWith("```", StringComparison.InvariantCulture));
                            var blockEndIndex = updatedLines.FindIndex(codeFenceStart+1, s => s.Trim().Equals("```", StringComparison.InvariantCulture));
                            pivotBlock.AddRange(updatedLines.GetRange(i+1, blockEndIndex - i));

                            pivotBlock.Add(string.Empty);
                            pivotBlock.Add(ZoneEnd);
                            pivotBlock.Add(string.Empty);
                            i = blockEndIndex;
                        }
                        else
                        {
                            // Other blocks are simpler
                            var blockEndIndex = updatedLines.FindIndex(i+1, s=> string.IsNullOrWhiteSpace(s) || s.Contains("#tab/", StringComparison.InvariantCultureIgnoreCase));
                            pivotBlock.AddRange(updatedLines.GetRange(i+1, blockEndIndex-i-1));
                            pivotBlock.Add(ZoneEnd);
                            pivotBlock.Add(string.Empty);

                            i = blockEndIndex - 1;
                        }
                    }
                    else if (i < endIndex)
                    {
                        pivotBlock.Add(updatedLines[i]);
                    }
                }

                // Replace old block with new
                updatedLines.RemoveRange(currentIndex, endIndex - currentIndex + 1);
                updatedLines.InsertRange(currentIndex, pivotBlock);

                currentIndex = updatedLines.FindIndex(currentIndex + pivotBlock.Count,
                    s => s.Trim().Equals(httpTab, StringComparison.InvariantCultureIgnoreCase));
            }

            return updatedLines.ToArray();
        }

        public static string GetZonePivotForLanguage(string codeFenceName)
        {
            var zoneId = codeFenceName.Replace("javascript", "browserjs").Replace("objc", "objectivec");
            return $"::: zone pivot=\"programming-language-{zoneId}\"";
        }

        private static string MapTabToZone(string tab)
        {
            if (zones.TryGetValue(tab, out string zoneId))
            {
                return $"::: zone pivot=\"{zoneId}\"";
            }

            throw new ArgumentOutOfRangeException($"Unknown tab identifier {tab}");
        }
    }

    public static class ZonePivotStringExtensions
    {
        public static bool IsNonZonePivotLine(this string line)
        {
            return !string.IsNullOrWhiteSpace(line) &&
                !line.StartsWith("::: zone") &&
                !line.Contains("[sample-code]") &&
                !line.Contains("[sdk-documentation]");
        }
    }
}
