﻿using ElasticsearchFulltextExample.Web.Contracts;
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
        private readonly ApplicationOptions applicationOptions;

        public TesseractService(IOptions<ApplicationOptions> applicationOptions)
        {
            this.applicationOptions = applicationOptions.Value;
        }

        public async Task<string> ProcessDocument(DocumentDto document, string language)
        {
            var temporarySourceFilename = Path.Combine(applicationOptions.TempDirectory, Path.GetRandomFileName());
            var temporaryTargetFilename = Path.Combine(applicationOptions.TempDirectory, Path.GetRandomFileName());

            // The Tesseract CLI in 5.0.0-alpha always adds a .txt to the output file:
            var temporaryTesseractOutputFile = $"{temporaryTargetFilename}.txt";

            try
            {
                using (var fileStream = new FileStream(temporarySourceFilename, FileMode.Create))
                {
                    await document.File.CopyToAsync(fileStream);
                }

                var tesseractArguments = $"{temporarySourceFilename} {temporaryTargetFilename} -l {language}";

                var result = await RunProcessAsync(applicationOptions.Tesseract, tesseractArguments);

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
