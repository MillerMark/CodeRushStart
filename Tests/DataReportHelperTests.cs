#nullable enable
using NUnit.Framework;
using System;
using System.IO;
using System.Collections.Generic;
using DXWpf.BigMethod;

[TestFixture]
public class DataReportHelperTests {
    private string CreateTempFile(IEnumerable<string> lines) {
        var path = Path.GetTempFileName();
        File.WriteAllLines(path, lines);
        return path;
    }

    private string ReadFile(string path) {
        return File.ReadAllText(path);
    }

    [Test]
    public void ProcessDataAndGenerateReport_FiltersAboveThreshold() {
        var inputLines = new[]
        {
                "Alice,50,Alpha",
                "Bob,30,Beta",
                "Charlie,70,Alpha"
            };
        var inputFile = CreateTempFile(inputLines);
        var outputFile = Path.GetTempFileName();

        try {
            var sut = new DataReportHelper();
            sut.ProcessDataAndGenerateReport(inputFile, outputFile, 40, false);

            var output = ReadFile(outputFile);

            StringAssert.Contains("Alice,50,Alpha", output);
            StringAssert.DoesNotContain("Bob,30,Beta", output);
            StringAssert.Contains("Charlie,70,Alpha", output);
            StringAssert.Contains("Alpha: 2", output);
            StringAssert.Contains("Beta: 1", output);
            StringAssert.Contains("Total Records: 3", output);
            StringAssert.Contains("Above Threshold: 2", output);
            StringAssert.Contains("Below Threshold: 1", output);
        }
        finally {
            File.Delete(inputFile);
            File.Delete(outputFile);
        }
    }

    [Test]
    public void ProcessDataAndGenerateReport_IncludesSummary_WhenRequested() {
        var inputLines = new[]
        {
                "Alice,50,Alpha",
                "Bob,30,Beta"
            };
        var inputFile = CreateTempFile(inputLines);
        var outputFile = Path.GetTempFileName();

        try {
            var sut = new DataReportHelper();
            sut.ProcessDataAndGenerateReport(inputFile, outputFile, 40, true);

            var output = ReadFile(outputFile);

            StringAssert.Contains("Summary:", output);
            Assert.That(
                output.Contains("Majority of records are above the threshold.") ||
                output.Contains("Majority of records are below the threshold.") ||
                output.Contains("Equal number of records above and below the threshold."),
                Is.True);
        }
        finally {
            File.Delete(inputFile);
            File.Delete(outputFile);
        }
    }

    [Test]
    public void ProcessDataAndGenerateReport_HandlesMalformedLinesAndErrors() {
        var inputLines = new[] {
                "Alice,50,Alpha",
                "MalformedLine",
                "Bob,notanumber,Beta",
                string.Empty
            };
        var inputFile = CreateTempFile(inputLines);
        var outputFile = Path.GetTempFileName();

        try {
            var sut = new DataReportHelper();
            sut.ProcessDataAndGenerateReport(inputFile, outputFile, 40, false);

            var output = ReadFile(outputFile);

            StringAssert.Contains("Errors:", output);
            StringAssert.Contains("Malformed line: MalformedLine", output);
            StringAssert.Contains("Invalid value for Bob: notanumber", output);
            StringAssert.Contains("Empty line detected", output);
        }
        finally {
            File.Delete(inputFile);
            File.Delete(outputFile);
        }
    }

    [Test]
    public void ProcessDataAndGenerateReport_HandlesEqualAboveAndBelowThreshold() {
        var inputLines = new[]
        {
                "Alice,50,Alpha",
                "Bob,30,Beta"
            };
        var inputFile = CreateTempFile(inputLines);
        var outputFile = Path.GetTempFileName();

        try {
            var sut = new DataReportHelper();
            sut.ProcessDataAndGenerateReport(inputFile, outputFile, 40, true);

            var output = ReadFile(outputFile);

            Assert.That(
                output.Contains("Equal number of records above and below the threshold.") ||
                output.Contains("Majority of records are above the threshold.") ||
                output.Contains("Majority of records are below the threshold."),
                Is.True);

        }
        finally {
            File.Delete(inputFile);
            File.Delete(outputFile);
        }
    }
}
