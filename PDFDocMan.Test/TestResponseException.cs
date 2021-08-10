using System;

namespace PDFDocMan.Test
{
    public class TestResponseException : Exception
    {
        public TestResponseException(string message) : base(message) { }
    }
}
