using System;
using System.Linq;
using FileImporter.model;
using FileImporter.Model;
using FileImporter.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestSalesImporter
{
    [TestClass]
    public class FileImporterUnitTest
    {
        [TestMethod]
        public void TestIfImportSeller()
        {

            FileContent fileContent = new FileContent();
            string line = "001ç3245678865434çPauloç40000.99";
            LoggerService.disableLogger = true;

            FileImporterService fileImporterService = new FileImporterService(configureFileImporterParameters());
            PrivateObject obj = new PrivateObject(fileImporterService);
            obj.Invoke("processSeller", fileContent, line);

            Assert.IsTrue(
                fileContent.Sellers.Count == 1 &&
                fileContent.Sellers[0].Name == "Paulo" &&
                fileContent.Sellers[0].CPF == "3245678865434" &&
                fileContent.Sellers[0].Salary == 40000.99m
            );
        }

        [TestMethod]
        public void TestIfImportCustomer()
        {

            FileContent fileContent = new FileContent();
            string line = "002ç2345675434544345çJose da SilvaçRural";
            LoggerService.disableLogger = true;

            FileImporterService fileImporterService = new FileImporterService(configureFileImporterParameters());
            PrivateObject obj = new PrivateObject(fileImporterService);
            obj.Invoke("processCustomer", fileContent, line);

            Assert.IsTrue(
                fileContent.Customers.Count == 1 &&
                fileContent.Customers[0].Name == "Jose da Silva" &&
                fileContent.Customers[0].CNPJ == "2345675434544345" &&
                fileContent.Customers[0].BusinessArea == "Rural"
            );
        }

        [TestMethod]
        public void TestIfImportSale()
        {

            FileContent fileContent = new FileContent();
            string line = "003ç10ç[1-10-100,2-10-1.50,3-1-3.30]çPedro";
            LoggerService.disableLogger = true;

            FileImporterService fileImporterService = new FileImporterService(configureFileImporterParameters());
            PrivateObject obj = new PrivateObject(fileImporterService);
            obj.Invoke("processSales", fileContent, line);

            Assert.IsTrue(
                fileContent.Sales.Count == 1 &&
                fileContent.Sales[0].SaleId == 10 &&
                fileContent.Sales[0].SalesmanName == "Pedro" &&
                fileContent.Sales[0].SalesTotalValue == 1018.3m
            );
        }

        [TestMethod]
        public void TestSumarizeReport()
        {
            LoggerService.disableLogger = true;
            FileContent fileContent = new FileContent();
            fileContent.Sales.Add(new Sales { SaleId = 1, SalesmanName = "Pedro", SalesTotalValue = 100 });
            fileContent.Sales.Add(new Sales { SaleId = 2, SalesmanName = "Carlos", SalesTotalValue = 200 });
            fileContent.Sales.Add(new Sales { SaleId = 3, SalesmanName = "Pedro", SalesTotalValue = 200 });
            fileContent.Sellers.Add(new Seller { CPF = "111111111", Name = "Pedro", Salary = 2500.55m });
            fileContent.Sellers.Add(new Seller { CPF = "222222222", Name = "Carlos", Salary = 2800.55m });
            fileContent.Customers.Add(new Customer { CNPJ = "99999999999999", BusinessArea = "TI", Name = "XXX LTDA" });
            fileContent.Customers.Add(new Customer { CNPJ = "88888888888888", BusinessArea = "Agro", Name = "YYY LTDA" });

            FileImporterService fileImporterService = new FileImporterService(configureFileImporterParameters());
            PrivateObject obj = new PrivateObject(fileImporterService);

            OutputFileContent summarizeFile = (OutputFileContent)obj.Invoke("summarizeFile", fileContent);

            Assert.IsTrue(
                summarizeFile.CustomerCount == 2 &&
                summarizeFile.SellerCount == 2 &&
                Enumerable.SequenceEqual(summarizeFile.ExpensiveSaleId, new int[] { 2, 3 }) &&
                Enumerable.SequenceEqual(summarizeFile.WorstSaller, new string[] { "Carlos" })
            );
        }

        private FileImporterConfiguration configureFileImporterParameters()
        {
            /* 
             * Os parametros devem ser os mesmos do APP Config 
             * (Nem todos são utilizados no projeto de teste
             */
            FileImporterConfiguration fileImporterConfiguration = new FileImporterConfiguration()
            {
                //baseDir = "c:\\SalesImporter",
                //inputDir = "data\\in",
                //outputDir = "data\\out",
                //processedFilesDir = "data\\processed",
                //ignoredFilesDir = "data\\ignored",
                //logDir = "logs",
                //extentionFile = ".dat",
                //processedExtentionFile = ".done.dat",

                separator = "ç".ToCharArray(),
                itensSeparator = ",".ToCharArray(),
                itemSeparator = "-".ToCharArray(),
                itensInitialMark = Convert.ToChar("["),
                itensFinalMark = Convert.ToChar("]"),
            };
            return fileImporterConfiguration;
        }
    }
}
