#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DXWpf.BigMethod {
    public class DataReportHelper {
        public void ProcessDataAndGenerateReport(string inputFile, string outputFile, int threshold, bool includeSummary) {
            var lines = File.ReadAllLines(inputFile);
            var outputLines = new List<string>();
            var errors = new List<string>();
            var groupCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int totalRecords = 0;
            int aboveThreshold = 0;
            int belowThreshold = 0;
            var filteredLines = new List<string>();

            foreach (var line in lines) {
                if (string.IsNullOrWhiteSpace(line)) {
                    errors.Add("Empty line detected");
                    continue;
                }

                var parts = line.Split(',');
                if (parts.Length != 3) {
                    errors.Add($"Malformed line: {line}");
                    continue;
                }

                var name = parts[0].Trim();
                var valueStr = parts[1].Trim();
                var group = parts[2].Trim();

                if (!int.TryParse(valueStr, out int value)) {
                    errors.Add($"Invalid value for {name}: {valueStr}");
                    continue;
                }

                totalRecords++;

                if (!groupCounts.ContainsKey(group))
                    groupCounts[group] = 0;
                groupCounts[group]++;

                if (value > threshold) {
                    aboveThreshold++;
                    filteredLines.Add(line);
                }
                else {
                    belowThreshold++;
                }
            }

            // Write filtered lines
            foreach (var filtered in filteredLines)
                outputLines.Add(filtered);

            // Write group counts
            foreach (var kvp in groupCounts)
                outputLines.Add($"{kvp.Key}: {kvp.Value}");

            // Write summary stats
            outputLines.Add($"Total Records: {totalRecords}");
            outputLines.Add($"Above Threshold: {aboveThreshold}");
            outputLines.Add($"Below Threshold: {belowThreshold}");

            if (includeSummary) {
                outputLines.Add("Summary:");
                if (aboveThreshold > belowThreshold)
                    outputLines.Add("Majority of records are above the threshold.");
                else if (belowThreshold > aboveThreshold)
                    outputLines.Add("Majority of records are below the threshold.");
                else
                    outputLines.Add("Equal number of records above and below the threshold.");
            }

            if (errors.Count > 0) {
                outputLines.Add("Errors:");
                outputLines.AddRange(errors);
            }

            File.WriteAllLines(outputFile, outputLines);
        }
    }
}
