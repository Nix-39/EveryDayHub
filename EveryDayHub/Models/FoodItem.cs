
using System.Text.Json.Serialization;

namespace EveryDayHub.Models
{
    // Abstract base class for everything that can be at home
    [JsonPolymorphic]
    [JsonDerivedType(typeof(Dairy), "dairy")]
    [JsonDerivedType(typeof(Vegetable), "vegetable")]
    [JsonDerivedType(typeof(Meat), "meat")]
    [JsonDerivedType(typeof(Pantry), "pantry")]
    public abstract class FoodItem
    {
        // Unique identifier for each food item
        public int Id { get; set; }

        // Name of the food item
        public string Name { get; set; } = "";

        // True if the item is available at home
        public abstract bool IsAvailable { get; }
    }
}
