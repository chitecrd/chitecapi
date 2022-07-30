using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
	public class StockTranfersDetail
	{
		public string DocEntry { get; set; }
		public string LineNum { get; set; }
		public string ItemCode { get; set; }
		public string ItemDescription { get; set; }
		public string Quantity { get; set; }
		public string Price { get; set; }
		public string Currency { get; set; }
		public string SerialNumber { get; set; }
		public string WarehouseCode { get; set; }
		public string FromWarehouseCode { get; set; }
		public string UnitsOfMeasurment { get; set; }
		public string UoMCode { get; set; }
		public string InventoryQuantity { get; set; }
		public string LineStatus { get; set; }
		public string M_SYN { get; set; }

		

		[JsonIgnore]
		public bool IsNotInitialized =>
		  DocEntry == null;

	}
}
