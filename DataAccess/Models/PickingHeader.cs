using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class PickingHeader
    {
        public string DocEntry { get; set; }
        public string DocNum { get; set; }
        public string DocDate { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string DocTotal { get; set; }
        public string DocCurrency { get; set; }
        public string JournalMemo { get; set; }
        public string DocTime { get; set; }
        public string SalesPersonCode { get; set; }
        public string TransNum { get; set; }
        public string VatSum { get; set; }
        public string VatSumSys { get; set; }
        public string DocTotalSys { get; set; }
        public string PickStatus { get; set; }
        public string DocumentStatus { get; set; }
        public string DownPaymentType { get; set; }
        public string U_NCF { get; set; }
        public string U_Usuario { get; set; }
        public string U_claveVendedor { get; set; }
        public List<PickingDetail> DocumentLines { get; set; }
        public string Comments { get; set; }

        [JsonIgnore]
        public bool IsNotInitialized =>
            DocEntry == null;
    }




}


