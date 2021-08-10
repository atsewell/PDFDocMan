using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PDFDocMan.Api.Db;

namespace PDFDocMan.Test
{
    public static class TestSetup
    {
        public static readonly string BaseDir = Path.Combine(AppContext.BaseDirectory, "data");
        private static readonly string[] TestDocFiles =
        {
            "sample-pdf.pdf",
            "sample-pdf2.pdf",
            "sample-pdf3.pdf",
            "sample-pdf-with-images.pdf"
        };

        private static IEnumerable<Doc> ReadDocFiles() => TestDocFiles.Select(f => ReadDocFile(f));

        private static Doc ReadDocFile(string filename)
        {
            var filePath = Path.Combine(BaseDir, filename);
            var content = File.ReadAllBytes(filePath);
            return new Doc()
            {
                Filename = filename,
                Binary = content,
                Size = content.Length,
                CreateDt = DateTime.Now
            };
        }

        public static void InitTestDb(DocDbContext context)
        {
            var DocRepo = new DocRepo(context);
            context.Database.EnsureCreated();
            var docs = ReadDocFiles();
            foreach (var doc in docs)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                using var t = new Task(() => DocRepo.Create(doc));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                t.RunSynchronously();
            }
        }
    }
}
