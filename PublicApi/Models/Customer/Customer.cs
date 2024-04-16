namespace PublicApi.Models.Customer
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public int CustomerTypeId { get; set; }
        public string CustomerTypeDescription { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
