namespace ShareYourRecipe.Models
{
  public class User
  {
    public required int UserId { get; set; } // Primary Key
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required string Password { get; set; } // In a real app, use Identity for password management
  }
}
