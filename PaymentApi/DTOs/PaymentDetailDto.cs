using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentApi.DTOs
{
    public class PaymentDetailDto
    {
        public int PaymentDetailId { get; set; }
        public string CardOwnerName { get; set; } = "";
        [Column(TypeName = "nvarchar(16)")]
        public string CardNumber { get; set; } = "";
        [Column(TypeName = "nvarchar(5)")]
        public string ExpirationDate { get; set; } = "";
        [Column(TypeName = "nvarchar(3)")]
        public string SecurityCode { get; set; } = "";
    }
}
