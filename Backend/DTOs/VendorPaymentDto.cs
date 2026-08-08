namespace RestaurantSystem.DTOs
{
    public class VendorPaymentDto
    {
        public int Id { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaidByUser { get; set; }
        public DateTime PaidOn { get; set; }
    }

}
