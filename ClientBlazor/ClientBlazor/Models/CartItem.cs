namespace ClientBlazor.Models
{
    public class CartItem
    {
        public CartItem() { }
        public CartItem(Product product, bool inStock, uint queuePosition, long acquisitionTime)
        {
            Product = product;
            InStock = inStock;
            QueuePosition = queuePosition;
            AcquisitionTime = acquisitionTime;
        }

        public Product Product { get; set; }
        public bool InStock { get; set; }
        public uint QueuePosition { get; set; } = 0;
        public long AcquisitionTime { get; set; }
    }
}
