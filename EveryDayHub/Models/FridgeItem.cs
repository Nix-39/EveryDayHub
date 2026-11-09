
namespace EveryDayHub.Models
{
    // Base class for items kept in the fridge
    public abstract class FridgeItem : FoodItem
    {
        // Quantity of the food item
        public int Quantity { get; set; }

        // Expiration date of the food item
        public DateTime ExpirationDate { get; set; }

        // Days before expiration when the item is flagged
        public abstract int WarningDays { get; }

        // An item is available when its quantity is greater than zero
        public override bool IsAvailable => Quantity > 0;
    }
}
