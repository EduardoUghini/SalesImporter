namespace FileImporter.model
{
    public class OutputFileContent
    {
        public int CustomerCount { get; set; }
        public int SellerCount { get; set; }
        public int[] ExpensiveSaleId { get; set; }
        public string[] WorstSaller { get; set; }
    }
}
