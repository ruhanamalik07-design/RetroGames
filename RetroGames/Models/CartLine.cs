namespace RetroGames.Models;

public record CartLine(int ProductId, string Name, decimal UnitPrice, int Quantity)
{
    public decimal Total => UnitPrice * Quantity;
}
