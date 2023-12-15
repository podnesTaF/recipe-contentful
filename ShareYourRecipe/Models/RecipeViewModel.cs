

namespace ShareYourRecipe.Models
{
  public class RecipeViewModel
  {
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string PreparationTime { get; set; }
    public required string Energy { get; set; }

    public required string GramsPerServing { get; set; }

    public required string Author { get; set; }

    public IFormFile? Image { get; set; }

    public RecipeDto? Response { get; set; }
  }

  public class RecipeView
  {
    public required IEnumerable<Recipe> Recipes { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string? SearchString { get; set; }
  }
}
