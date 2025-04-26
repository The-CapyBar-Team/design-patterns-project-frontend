namespace ClientBlazor.Models
{
    public class CartItem
    {
        public CartItem() { }
        public CartItem(Product product, bool inStock, uint queuePosition)
        {
            Product = product;
            InStock = inStock;
            QueuePosition = queuePosition;
        }

        public Product Product { get; set; }
        public bool InStock { get; set; }
        public uint QueuePosition { get; set; } = 0;
    }
}
