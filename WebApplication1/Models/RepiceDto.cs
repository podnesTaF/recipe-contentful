using Contentful.Core.Models;

namespace WebApplication1.Models
{
  public class RecipeDto
  {
    public Dictionary<string, string>? Title { get; set; }
    public Dictionary<string, string>? Description { get; set; }
    public Dictionary<string, string>? PreparationTime { get; set; }
    public Dictionary<string, string>? Energy { get; set; }

    public SystemProperties? SystemProperties { get; set; }

    public Asset? Image { get; set; }
  }
}
