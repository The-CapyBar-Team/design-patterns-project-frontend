namespace ClientBlazor.Models
{
    public class CartItem
    {
        public CartItem() { }
        public CartItem(Product product, bool inStock, uint queuePosition, long acquisitionTime, int availableStock)
        {
            Product = product;
            InStock = inStock;
            QueuePosition = queuePosition;
            AcquisitionTime = acquisitionTime;
            AvailableStock = availableStock;
        }

        public Product Product { get; set; }
        public bool InStock { get; set; }
        public uint QueuePosition { get; set; } = 0;
        public long AcquisitionTime { get; set; }
        public int AvailableStock { get; set; }
    }
}
