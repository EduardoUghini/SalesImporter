using System;
using System.Collections.Generic;
using System.Text;

namespace FileImporter.Model
{
    public class FileImporterConfiguration
    {
        public string baseDir { get; set; }
        public string inputDir { get; set; }
        public string outputDir { get; set; }
        public string processedFilesDir { get; set; }
        public string ignoredFilesDir { get; set; }
        public string logDir { get; set; }
        public bool disableLogger { get; set; }

        public string extentionFile { get; set; }
        public string processedExtentionFile { get; set; }
        public char[] separator { get; set; }
        public char[] itensSeparator { get; set; }
        public char[] itemSeparator { get; set; }
        public char itensInitialMark { get; set; }
        public char itensFinalMark { get; set; }
    }
}
