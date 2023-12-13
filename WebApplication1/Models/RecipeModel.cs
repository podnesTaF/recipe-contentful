using Contentful.Core.Models;

namespace WebApplication1.Models;
public class Recipe
{
  public required string Title { get; set; }
  public required string Description { get; set; }
  public string? PreparationTime { get; set; }
  public string? Energy { get; set; }

  // Assuming image URLs are stored in the database
  public Asset? Image { get; set; }

  public SystemProperties? SystemProperties { get; set; }
}
