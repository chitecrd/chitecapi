using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
	public class InventoryCountingDetail
	{
		public string DocumentEntry { get; set; }
		public string LineNumber { get; set; }
		public string ItemCode { get; set; }
		public string ItemDescription { get; set; }
		public string Freeze { get; set; }
		public string WarehouseCode { get; set; }
		public string InWarehouseQuantity { get; set; }
		public string Counted { get; set; }
		public string UoMCode { get; set; }
		public string CountedQuantity { get; set; }
		public string Variance { get; set; }
		public string VariancePercentage { get; set; }
		public string VisualOrder { get; set; }
		public string TargetEntry { get; set; }
		public string TargetLine { get; set; }
		public string TargetType { get; set; }
		public string TargetReference { get; set; }
		public string ProjectCode { get; set; }
		public string Manufacturer { get; set; }
		public string LineStatus { get; set; }
		public string CounterType { get; set; }
		public string CounterID { get; set; }
		public string MultipleCounterRole { get; set; }

	}
}
