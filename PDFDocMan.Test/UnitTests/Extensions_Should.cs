using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFDocMan.Api;

namespace PDFDocMan.Test.UnitTests
{
    [TestClass]
    public class Extensions_Should
    {
        [TestMethod]
        public void HasPDFExtension_ExtensionIsPDF_ReturnTrue()
        {
            var filename = "MyTestFile.PDF";
            var result = filename.HasPDFExtension();
            Assert.IsTrue(result, $"{filename} should be recognised as having the PDF extension");
        }

        [TestMethod]
        public void HasPDFExtension_ExtensionIsNotPDF_ReturnFalse()
        {
            var filename = "MyTestFile.XLS";
            var result = filename.HasPDFExtension();
            Assert.IsFalse(result, $"{filename} should be recognised as NOT having the PDF extension");
        }

        [DataTestMethod]
        [DataRow("%PDF-1.5")]
        [DataRow("%PDF Blah")]
        [DataRow("%PDFXXXX")]
        public void ContentIsPDF_ContentIsPDF_ReturnTrue(string source)
        {
            var content = Encoding.ASCII.GetBytes(source);
            var result = content.ContentIsPDF();
            Assert.IsTrue(result, $"{source} should be recognised as a valid PDF");
        }

        [DataTestMethod]
        [DataRow("PDF-1.5")]
        [DataRow("%PD F-1.5")]
        [DataRow("% PDF-1.4")]
        [TestMethod]
        public void ContentIsPDF_ContentIsNotPDF_ReturnFalse(string source)
        {
            var content = Encoding.ASCII.GetBytes(source);
            var result = content.ContentIsPDF();
            Assert.IsFalse(result, $"{source} should be recognised as NOT a valid PDF");
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(99)]
        [DataRow(9999999)]
        [DataRow(5242881)]
        public void IsValidFileSize_InvalidValues_ReturnFalse(long size)
        {
            var result = size.IsValidFileSize();
            Assert.IsFalse(result, $"{size} should be an invalid file size");
        }

        [DataTestMethod]
        [DataRow(100)]
        [DataRow(12345)]
        [DataRow(5242880)]
        public void IsValidFileSize_ValidValues_ReturnTrue(long size)
        {
            var result = size.IsValidFileSize();
            Assert.IsTrue(result, $"{size} should be a valid file size");
        }

        private IFormFile GetFormFile(string filename, string source)
        {
            var content = Encoding.ASCII.GetBytes(source);
            var stream = new MemoryStream(content);
            return new FormFile(stream, 0, source.Length, Path.GetFileNameWithoutExtension(filename), filename);
        }

        private Func<Task> GetFormFileAction(string filename, string source)
        {
            var file = GetFormFile(filename, source);
            return async () => { await file.GetPDFDoc(); };
        }

        [DataTestMethod]
        [DataRow("MyTest1.PDF", "%PDF-NOT Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged.It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.")]
        [TestMethod]
        public async Task GetPDFDoc_ValidInput_Succeed(string filename, string source)
        {
            var file = GetFormFile(filename, source);
            var doc = await file.GetPDFDoc();

            Assert.AreEqual(doc.Filename, filename, $"Filename should match original {filename}");
            var tsource = Encoding.ASCII.GetString(doc.Binary);
            Assert.AreEqual(tsource, source, $"Source should match original {source}");
        }

        [DataTestMethod]
        [DataRow("MyTest1.PDF", "%PDF TEST 1")]
        [DataRow("MyTest1.DOCX", "Not a PDF :-)")]
        [DataRow("MyTest2323.PDF", "%PDX-1.4 Content is not PDF")]
        [TestMethod]
        public async Task GetPDFDoc_InvalidContentLength_ThrowException(string filename, string source)
        {
            var action = GetFormFileAction(filename, source);
            await action.Should().ThrowAsync<ValidationException>().WithMessage("Uploaded file*");
        }
    }
}
