namespace Application.DTOs
{
    public class TeklifSatiriDto
    {
        public Guid Id { get; set; }
        public int UrunId { get; set; }
        public string UrunAdi { get; set; } = string.Empty; // Frontend'e kolaylık olması için ürün adını ekliyoruz.
        public string Aciklama { get; set; } = string.Empty;
        public decimal Miktar { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal Toplam { get; set; }
    }
}
