using System;

namespace PDFDocMan.Api
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}
