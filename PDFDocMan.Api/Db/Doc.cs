using System;

namespace PDFDocMan.Api.Db
{
    public class Doc
    {
        public int Id { get; set; }
        public string Filename { get; set; } = null!;
        public int Size { get; set; }
        public byte[] Binary { get; set; } = null!;
        public DateTime CreateDt { get; set; }
        public int? SortVal { get; set; }
    }
}
