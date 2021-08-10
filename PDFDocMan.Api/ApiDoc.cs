namespace PDFDocMan.Api
{
    public record ApiDoc
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public int Size { get; init; }
        public string Location { get; init; } = null!;
    }
}
