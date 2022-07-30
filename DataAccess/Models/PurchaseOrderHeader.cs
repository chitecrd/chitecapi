using Newtonsoft.Json;
using System;

namespace DataAccess.Models
{
    public class PurchaseOrderHeader
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string DocNum { get; set; }
        public string DocDate { get; set; }
        public string DocDueDate { get; set; }
        public string CardName { get; set; }
        public string CardCode { get; set; }
        public string NumAtCard { get; set; }
        public int DocRate { get; set; }
        public string JournalMemo { get; set; }
        public string TaxDate { get; set; }
        public decimal DocTotalFc { get; set; }
        public decimal DocTotalSys { get; set; }
        public string DocumentStatus { get; set; }
        public string U_NCF { get; set; }
        public int TotalLines { get; set; }

        [JsonIgnore]
        public bool IsNotInitialized =>
            Id == 0
            && DocNum == null
            && DocDate == null
            && DocDueDate == null
            && CardName == null
            && CardCode == null
            && NumAtCard == null
            && DocRate == 0
            && JournalMemo == null
            && TaxDate == null
            && DocTotalFc == 0
            && DocTotalSys == 0
            && DocumentStatus == null
            && U_NCF == null
            && TotalLines == 0;
    }
}
