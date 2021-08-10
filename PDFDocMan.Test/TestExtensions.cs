using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PDFDocMan.Test
{
    public static class TestExtensions
    {
        public static IFormFile GetFileForUpload(string filePath)
        {
            var stream = File.OpenRead(filePath);

            return new FormFile(stream, 0, stream.Length, Path.GetFileNameWithoutExtension(filePath), Path.GetFileName(filePath));
        }

        private static readonly TaskFactory _taskFactory = new
            TaskFactory(CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
            => _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();

        public static void RunSync(Func<Task> func)
            => _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
    }
}
