using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace PaymentApi.Models
{
    public class StoredProcedureModel 
    {
        public string CardOwnerName { get; set; }
        public string CardNumber { get; set; }
        public string SecurityCode {  get; set; }
        public string ExpirationDate { get; set; }

    }
}
