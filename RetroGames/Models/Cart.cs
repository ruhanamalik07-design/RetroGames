namespace RetroGames.Models;

public class Cart
{
    public int Id { get; set; }
    public Guid SessionKey { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
