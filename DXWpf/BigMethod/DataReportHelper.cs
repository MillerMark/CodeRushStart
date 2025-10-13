#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace DXWpf.BigMethod {
    public class DataReportHelper {
        public void ProcessDataAndGenerateReport(string inputFile, string outputFile, int threshold, bool includeSummary) {
            var lines = File.ReadAllLines(inputFile);
            var filtered = new List<string>();
            var stats = new Dictionary<string, int>();
            var errors = new List<string>();
            var sb = new StringBuilder();
            int total = 0;
            int aboveThreshold = 0;
            int belowThreshold = 0;
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
                if (!int.TryParse(parts[1], out var value)) {
                    errors.Add($"Invalid value for {name}: {parts[1]}");
                    continue;
                }
                var category = parts[2].Trim();
                if (!stats.ContainsKey(category))
                    stats[category] = 0;
                stats[category]++;
                total++;
                if (value >= threshold) {
                    aboveThreshold++;
                    filtered.Add(line);
                }
                else {
                    belowThreshold++;
                }
            }
            sb.AppendLine("Filtered Results:");
            foreach (var entry in filtered) {
                sb.AppendLine(entry);
            }
            sb.AppendLine();
            sb.AppendLine("Category Counts:");
            foreach (var kvp in stats.OrderByDescending(x => x.Value)) {
                sb.AppendLine($"{kvp.Key}: {kvp.Value}");
            }
            sb.AppendLine();
            sb.AppendLine($"Total Records: {total}");
            sb.AppendLine($"Above Threshold: {aboveThreshold}");
            sb.AppendLine($"Below Threshold: {belowThreshold}");
            if (includeSummary) {
                sb.AppendLine();
                sb.AppendLine("Summary:");
                if (aboveThreshold > belowThreshold) {
                    sb.AppendLine("Majority of records are above the threshold.");
                }
                else if (aboveThreshold < belowThreshold) {
                    sb.AppendLine("Majority of records are below the threshold.");
                }
                else {
                    sb.AppendLine("Equal number of records above and below the threshold.");
                }
            }
            if (errors.Count > 0) {
                sb.AppendLine();
                sb.AppendLine("Errors:");
                foreach (var err in errors) {
                    sb.AppendLine(err);
                }
            }
            File.WriteAllText(outputFile, sb.ToString());
        }
    }
}