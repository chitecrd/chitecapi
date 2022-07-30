using Newtonsoft.Json;

namespace DataAccess.Models
{
    public class PurchaseOrderDetail
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string DocNum { get; set; }
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public float Quantity { get; set; }
        public decimal Costo_Unitario { get; set; }
        public string BarCode { get; set; }
        public string UoMCode { get; set; }
        public string LineStatus { get; set; }
        public string UserName { get; set; }
        public string WarehouseCode { get; set; }
        public string BinLocation { get; set; }
        public decimal Costo_Total { get; set; }
        public decimal Itbis { get; set; }
        public float QuantityConfirm { get; set; }

        [JsonIgnore]
        public bool IsNotInitialized =>
            Id == 0
            && DocNum == null
            && LineNum == 0
            && ItemCode == null
            && ItemDescription == null
            && Quantity == 0
            && Costo_Unitario == 0
            && BarCode == null
            && UoMCode == null
            && LineStatus == null
            && UserName == null
            && WarehouseCode == null
            && BinLocation == null
            && Costo_Total == 0
            && Itbis == 0
            && QuantityConfirm == 0;
    }
}
