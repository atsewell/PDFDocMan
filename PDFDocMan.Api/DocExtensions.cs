using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PDFDocMan.Api.Db;

namespace PDFDocMan.Api
{
    public static class DocExtensions
    {
        /// <summary>
        /// Check if the given filename has the .PDF extension.
        /// </summary>
        /// <param name="filename">The filename to be tested.</param>
        /// <returns>True if the filename has the .PDF extension or false otherwise.</returns>
        public static bool HasPDFExtension(this string filename) => Path.GetExtension(filename).Equals(".pdf", StringComparison.InvariantCultureIgnoreCase);

        public const int MinPDFFileSize = 100;
        public const int MaxPDFFileSize = 1024 * 1024 * 5;

        /// <summary>
        /// Check that the given file size is with the valid range.
        /// The filename is assumed to be valid :-/
        /// </summary>
        /// <param name="size">The size of the file in bytes.</param>
        /// <returns>True if the file size is in the correct range or false otherwise.</returns>
        public static bool IsValidFileSize(this long size) => !(size < MinPDFFileSize || size > MaxPDFFileSize);

        /// <summary>
        /// Check that the given byte array appears to contain a PDF.
        /// </summary>
        /// <param name="content">A byte array buffer containing the fiel.</param>
        /// <returns>True if the buffer is deemed to be a valid PDF or false otherwise.</returns>
        public static bool ContentIsPDF(this byte[] content)
        {
            var s = Encoding.Default.GetString(content[0..4]);
            return s == "%PDF";
        }

        /// <summary>
        /// Get the PDF document from the uploaded file object.
        /// </summary>
        /// <param name="file">An IFormFile object containing a PDF.</param>
        /// <returns>A new Doc entity to be added to the database.</returns>
        public static async Task<Doc> GetPDFDoc(this IFormFile file)
        {
            if (!file.FileName.HasPDFExtension())
            {
                throw new ValidationException("Uploaded file does not have the PDF extension");
            }

            if (!file.Length.IsValidFileSize())
            {
                throw new ValidationException($"Uploaded file size, {file.Length}, is invalid; should be between {MinPDFFileSize} and {MaxPDFFileSize} bytes");
            }

            using var ms = new MemoryStream();

            await file.CopyToAsync(ms);
            ms.Close();

            var doc = new Doc
            {
                Filename = file.FileName,
                Binary = ms.ToArray(),
                Size = (int)file.Length
            };

            if (!ContentIsPDF(doc.Binary))
            {
                throw new ValidationException($"Uploaded file does not appear to be a PDF");
            }

            return doc;
        }

        /// <summary>
        /// Creates an ApiDoc object, for returning a document from the API, from a database Doc entity object
        /// </summary>
        /// <param name="doc">The db Doc entity</param>
        /// <param name="basePath">The base path for the location.</param>
        /// <returns>An ApiDoc object.</returns>
        public static ApiDoc GetApiDoc(this Doc doc, string basePath) =>
            new ApiDoc()
            {
                Id = doc.Id,
                Name = doc.Filename,
                Size = doc.Size,
                Location = $"{basePath}/{doc.Id}"
            };

        /// <summary>
        /// Gets a collection of ApiDoc objects from collection of Doc entities.
        /// </summary>
        /// <param name="docs">The collection of Doc entities.</param>
        /// <param name="basePath">The base path for the location.</param>
        /// <returns>A collection of ApiDoc objects.</returns>
        public static IEnumerable<ApiDoc> GetApiDocs(this IEnumerable<Doc> docs, string basePath) => docs.Select(d => d.GetApiDoc(basePath));
    }
}
