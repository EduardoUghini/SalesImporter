using System.Collections.Generic;

namespace FileImporter.model
{
    public class Sales
    {
        public int SaleId { get; set; }
        public IEnumerable<SalesItem> SalesItens { get; set; }
        public string SalesmanName { get; set; }
        public decimal SalesTotalValue { get; set; }
    }

    
}
