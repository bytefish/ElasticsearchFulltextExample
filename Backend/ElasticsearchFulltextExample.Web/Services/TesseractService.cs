// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchFulltextExample.Web.Options;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ElasticsearchFulltextExample.Web.Services
{
    public class TesseractService
    {
        private readonly TesseractOptions tesseractOptions;

        public TesseractService(IOptions<TesseractOptions> tesseractOptions)
        {
            this.tesseractOptions = tesseractOptions.Value;
        }

        public async Task<string> ProcessDocument(byte[] document, string language)
        {
            var temporarySourceFilename = Path.Combine(tesseractOptions.TempDirectory, Path.GetRandomFileName());
            var temporaryTargetFilename = Path.Combine(tesseractOptions.TempDirectory, Path.GetRandomFileName());

            // The Tesseract CLI in 5.0.0-alpha always adds a .txt to the output file:
            var temporaryTesseractOutputFile = $"{temporaryTargetFilename}.txt";

            try
            {
                await File.WriteAllBytesAsync(temporarySourceFilename, document);

                var tesseractArguments = $"{temporarySourceFilename} {temporaryTargetFilename} -l {language}";

                var result = await RunProcessAsync(tesseractOptions.Executable, tesseractArguments);

                if (result != 0)
                {
                    throw new Exception($"Tesseract exited with Error Code {result}");
                }

                if (!File.Exists(temporaryTesseractOutputFile))
                {
                    return string.Empty;
                }

                return File.ReadAllText(temporaryTesseractOutputFile);
            }
            finally
            {
                if (File.Exists(temporarySourceFilename))
                {
                    File.Delete(temporarySourceFilename);
                }

                if (File.Exists(temporaryTesseractOutputFile))
                {
                    File.Delete(temporaryTesseractOutputFile);
                }
            }
        }

        // This is just a very simple way to kick off Tesseract by Command Line. Does 
        // it scale? Oh it doesn't for sure, but we can switch the implementation at a 
        // later point anyway ...
        private static Task<int> RunProcessAsync(string filename, string arguments)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = { FileName = filename, Arguments = arguments },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);

                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }
}
