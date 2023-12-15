namespace ShareYourRecipe.Utilities
{
  public static class StringHelper
  {
    public static string TruncateDescription(string description, int maxLength)
    {
      if (string.IsNullOrEmpty(description))
      {
        return string.Empty;
      }

      return description.Length <= maxLength
          ? description
          : description.Substring(0, maxLength) + "...";
    }
  }

}