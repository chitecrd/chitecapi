using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
	public class InventoryCountingHeader
	{
		public string DocumentEntry { get; set; }
		public string DocumentNumber { get; set; }
		public string Series { get; set; }
		public string CountDate { get; set; }
		public string CountTime { get; set; }
		public string SingleCounterType { get; set; }
		public string SingleCounterID { get; set; }
		public string DocumentStatus { get; set; }
		public string Remarks { get; set; }
		public string DocObjectCodeEx { get; set; }
		public string FinancialPeriod { get; set; }
		public string PeriodIndicator { get; set; }
		public string CountingType { get; set; }

		public List<InventoryCountingDetail> InventoryCountingLines { get; set; }

		[JsonIgnore]
		public bool IsNotInitialized =>
		  DocumentEntry == null;

	}
}
