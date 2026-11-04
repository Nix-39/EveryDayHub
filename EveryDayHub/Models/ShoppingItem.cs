namespace EveryDayHub.Models;


public class ShoppingItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public bool IsBought { get; set; }

}