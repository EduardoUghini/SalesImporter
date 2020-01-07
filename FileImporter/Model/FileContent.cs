using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileImporter.model
{
    public class FileContent
    {
        public FileContent()
        {
            Customers = new List<Customer>();
            Sales = new List<Sales>();
            Sellers = new List<Seller>();
        }
        public List<Customer> Customers { get; set; }
        public List<Sales> Sales { get; set; }
        public List<Seller> Sellers { get; set; }
    }

    
}
