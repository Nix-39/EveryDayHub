
namespace EveryDayHub.Models
{
    // Abstract base class for all food items
    public abstract class FoodItem
    {
        // Unique identifier for each food item
        public int Id { get; set; }

        // Name of the food item
        public string Name { get; set; } = "";

        // Quantity of the food item
        public int Quantity { get; set; }

        // Expiration date of the food item
        public DateTime ExpirationDate { get; set; }
    }
}