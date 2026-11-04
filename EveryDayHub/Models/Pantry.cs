
namespace EveryDayHub.Models
{
    // Represents food items stored in the pantry
    public class Pantry : FoodItem
    {
        // Indicates whether the item is in stock
        public bool InStock { get; set; }

        // An item is available when it is in stock
        public override bool IsAvailable => InStock;
    }
}
