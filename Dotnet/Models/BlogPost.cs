using System;
using System.Collections.Generic;

namespace Dotnet.Models;

public partial class BlogPost
{
    public int PostId { get; set; }

    public string Tittle { get; set; } = null!;

    public string? PostDescription { get; set; }

    public string Content { get; set; } = null!;

    public DateTime PublishedDate { get; set; }

    public short? AuthorId { get; set; }

    public virtual UseList? Author { get; set; }
}
