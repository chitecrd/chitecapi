using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
	public class StockTranfersHeader
	{

		public string DocEntry { get; set; }
		public string DocDate { get; set; }
		public string DueDate { get; set; }
		public string CardCode { get; set; }
		public string CardName { get; set; }
		public string JournalMemo { get; set; }
		public string FromWarehouse { get; set; }
		public string ToWarehouse { get; set; }
		public string CreationDate { get; set; }
		public string UpdateDate { get; set; }
		public string TaxDate { get; set; }
		public string DocumentStatus { get; set; }
		public string User { get; set; }
		public string M_SYN { get; set; }

		public List<StockTranfersDetail> DocumentLines { get; set; }

		
		[JsonIgnore]
		public bool IsNotInitialized =>
		  DocEntry == null;
	}
}
