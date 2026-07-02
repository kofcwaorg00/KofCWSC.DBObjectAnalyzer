using System;
using System.Collections.Generic;
using System.Text;

namespace KofCWSC.DBObjectAnalyzer.Configuration
{
    public class AnalyzerSettings
    {
        public List<string> SourceFolders { get; set; } = new();

        public string OutputFolder { get; set; } = string.Empty;

        public List<string> FileExtensions { get; set; } = new();

        public List<string> IgnoreFolders { get; set; } = new();

        public List<string> ObjectTypes { get; set; } = new();

        public bool ShouldScanExtension(string extension) =>
            FileExtensions.Any(e =>
                string.Equals(e, extension, StringComparison.OrdinalIgnoreCase));

        public bool ShouldIgnoreFolder(string folderName) =>
            IgnoreFolders.Any(f =>
                string.Equals(f, folderName, StringComparison.OrdinalIgnoreCase));
    }
}
