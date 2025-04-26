namespace ClientBlazor.Models
{
    public class Product
    {
        public Product() { }
        public Product(int id, string name, string description, float price, uint stock)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            Stock = stock;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public uint Stock { get; set; }
    }
}
