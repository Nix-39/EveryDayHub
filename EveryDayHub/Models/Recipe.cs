namespace EveryDayHub.Models
{
    // Represents a recipe with its ingredients and cooking instructions
    public class Recipe
    {
        // Unique identifier for each recipe
        public int Id { get; set; }

        // Name of the recipe
        public string Name { get; set; } = "";

        // List of ingredients needed for the recipe
        public List<string> Ingredients { get; set; } = new();

        // Instructions for preparing the recipe
        public string Instructions { get; set; } = "";
    }
}