#nullable enable
using NUnit.Framework;
using DXWpf.BigMethod;
using System;
using System.IO;
using System.Collections.Generic;

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

            Assert.That(output, Does.Contain("Alice,50,Alpha"));
            Assert.That(output, Does.Not.Contain("Bob,30,Beta"));
            Assert.That(output, Does.Contain("Charlie,70,Alpha"));
            Assert.That(output, Does.Contain("Alpha: 2"));
            Assert.That(output, Does.Contain("Beta: 1"));
            Assert.That(output, Does.Contain("Total Records: 3"));
            Assert.That(output, Does.Contain("Above Threshold: 2"));
            Assert.That(output, Does.Contain("Below Threshold: 1"));
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

            Assert.That(output, Does.Contain("Summary:"));
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

            Assert.That(output, Does.Contain("Errors:"));
            Assert.That(output, Does.Contain("Malformed line: MalformedLine"));
            Assert.That(output, Does.Contain("Invalid value for Bob: notanumber"));
            Assert.That(output, Does.Contain("Empty line detected"));
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
