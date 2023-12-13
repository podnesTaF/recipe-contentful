

namespace WebApplication1.Models
{
  public class RecipeViewModel
  {
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string PreparationTime { get; set; }
    public required string Energy { get; set; }

    public IFormFile? Image { get; set; }

    public RecipeDto? Response { get; set; }
  }
}
