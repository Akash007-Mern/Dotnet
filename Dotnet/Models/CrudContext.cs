using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Dotnet.Models;

public partial class CrudContext : DbContext
{
    public CrudContext()
    {
    }

    public CrudContext(DbContextOptions<CrudContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BlogPost> BlogPosts { get; set; }

    public virtual DbSet<UseList> UseLists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=dbconn");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__BlogPost__AA126018A8345B5A");

            entity.Property(e => e.PostId).ValueGeneratedNever();
            entity.Property(e => e.AuthorId).HasColumnName("AuthorID");
            entity.Property(e => e.PostDescription).HasMaxLength(200);
            entity.Property(e => e.PublishedDate).HasColumnType("datetime");
            entity.Property(e => e.Tittle).HasMaxLength(200);

            entity.HasOne(d => d.Author).WithMany(p => p.BlogPosts)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK__BlogPosts__Autho__5165187F");
        });

        modelBuilder.Entity<UseList>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UseList__1788CCAC92CC03DD");

            entity.ToTable("UseList");

            entity.HasIndex(e => e.EmailAddress, "UQ__UseList__49A1474095B247AE").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.CurrentAddress).HasMaxLength(50);
            entity.Property(e => e.EmailAddress).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.UserPassword).HasMaxLength(50);
            entity.Property(e => e.UserRole).HasMaxLength(40);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
