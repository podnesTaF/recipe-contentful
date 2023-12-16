using Contentful.Core.Models;

namespace ShareYourRecipe.Models;
public class Recipe
{
  public required SystemProperties Sys { get; set; }
  public required string Title { get; set; }
  public required string Description { get; set; }
  public string? PreparationTime { get; set; }
  public string? Energy { get; set; }
  public required string GramsPerServing { get; set; }

  public required string Author { get; set; }

  public Asset? Image { get; set; }
}
