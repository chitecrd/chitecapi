using Newtonsoft.Json;

namespace DataAccess.Models
{
    public class BusinessPartner
    {
        [JsonIgnore]
        public string Id { get; set; }
        public string BusinessName { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string CardType { get; set; }
        public string Notes { get; set; }
        public string FederalTaxId { get; set; }
        public string Currency { get; set; }
        public string VatLiable { get; set; }
        [JsonIgnore]
        public string CreatedAt { get; set; }
        [JsonIgnore]
        public string UpdatedAt { get; set; }
        [JsonIgnore]
        public bool IsNotInitialized =>
            BusinessName == null
            && CardCode == null
            && CardName == null
            && CardType == null
            && Notes == null
            && FederalTaxId == null
            && Currency == null
            && VatLiable == null;
    }
}
