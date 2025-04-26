namespace ClientBlazor.Models
{
    public class CartItem
    {
        public Product Product { get; set; }
        public bool InStock { get; set; }
        public int QueuePosition { get; set; } = 0;
    }
}
