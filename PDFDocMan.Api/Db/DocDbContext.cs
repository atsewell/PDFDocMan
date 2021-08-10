using Microsoft.EntityFrameworkCore;

namespace PDFDocMan.Api.Db
{
    public class DocDbContext : DbContext
    {
        public DbSet<Doc> Docs { get; set; } = null!;

        public DocDbContext(DbContextOptions<DocDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Doc>().Property(d => d.Id)
                                            .HasColumnName("DocId")
                                            .ValueGeneratedOnAdd();
            modelBuilder.Entity<Doc>().Property(d => d.Binary)
                                            .HasColumnName("DocBinary");
            modelBuilder.Entity<Doc>().Property(d => d.Filename)
                                            .HasColumnName("DocFilename");
            modelBuilder.Entity<Doc>().Property(d => d.Size)
                                            .HasColumnName("SizeBytes");
            modelBuilder.Entity<Doc>().HasKey(d => d.Id)
                                            .IsClustered()
                                            .HasName("PK_Docs");
        }
    }
}
