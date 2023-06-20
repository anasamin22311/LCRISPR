using CRISPR.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRISPR.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Model> Models { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<DataSet> DataSets { get; set; }
        public DbSet<Prop> Props { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet <DNASequence> DNASequences { get; set; }
        public DbSet<CrisprOutViewModel> CrisprOutViewModels { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Model>().ToTable("Model");
            modelBuilder.Entity<Comment>().ToTable("Comment");
            modelBuilder.Entity<DataSet>().ToTable("DataSet");
            modelBuilder.Entity<Prop>().ToTable("Prop");
            modelBuilder.Entity<Tag>().ToTable("Tag");
            modelBuilder.Entity<DNASequence>().ToTable("DNASequence");
            modelBuilder.Entity<CrisprOutViewModel>().HasNoKey();
            modelBuilder.Entity<DNASequence>().HasNoKey();

            modelBuilder.Entity<DataSet>()
                .HasMany(ds => ds.Comments)
                .WithOne()
                .HasForeignKey("DataSetId");

            modelBuilder.Entity<DataSet>()
                .HasMany(ds => ds.Models)
                .WithOne()
                .HasForeignKey("DataSetId");

            modelBuilder.Entity<DataSet>()
                .HasMany(ds => ds.Tags) // Update this line to refer to the new Tag model
                .WithOne(t => t.DataSet) // Add the navigation property
                .HasForeignKey(t => t.DataSetId); // Use the correct foreign key property

            modelBuilder.Entity<Prop>()
                .HasOne(p => p.DataSet)
                .WithMany(ds => ds.Props)
                .HasForeignKey(p => p.ModelId);
        }
    }
}