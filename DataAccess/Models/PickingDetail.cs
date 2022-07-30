using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class PickingDetail
    {
        public string DocEntry { get; set; }
        public string LineNum { get; set; }
        public string ItemCode { get; set; }
        public string ItemDescription { get; set; }
        public string Quantity { get; set; }
        public string Price { get; set; }
        public string PriceAfterVAT { get; set; }
        public string Currency { get; set; }
        public string WarehouseCode { get; set; }
        public string BarCode { get; set; }
        public string PickQuantity { get; set; }
        public string FreeText { get; set; }
        public string ShippingMethod { get; set; }
        public string NetTaxAmount { get; set; }
        public string LineStatus { get; set; }
        public string PackageQuantity { get; set; }
        public string ActualDeliveryDate { get; set; }
        public string UoMCode { get; set; }
        public string InventoryQuantity { get; set; }
        public string U_metrosxpiezas { get; set; }
        public string U_articulosTienda { get; set; }
        public string U_Mayor { get; set; }
        public string U_metrosCliente { get; set; }
    }

}
