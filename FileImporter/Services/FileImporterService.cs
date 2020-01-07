using FileImporter.Common;
using FileImporter.model;
using FileImporter.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FileImporter.Services
{
    public class FileImporterService
    {
        #region private properties
        private FileImporterConfiguration _fileImporterConfiguration { get; set; }

        #endregion
        public FileImporterService(FileImporterConfiguration fileImporterConfiguration)
        {
            _fileImporterConfiguration = fileImporterConfiguration;
        }

        public void ProcessFile(string fullFileName)
        {
            try
            {
                if (Path.GetExtension(fullFileName).ToLower() == _fileImporterConfiguration.extentionFile.ToLower())
                {
                    importFile(fullFileName);
                    LoggerService.WriteLog("Arquivo importado!", LoggerService.LogType.info);
                }
                else
                {
                    ignoreFile(fullFileName);
                    LoggerService.WriteLog("Arquivo ignorado!", LoggerService.LogType.info);
                }
                
            }
            catch (Exception ex)
            {
                LoggerService.WriteLog("Ocorreu um erro durante o processamento.", LoggerService.LogType.error, ex);
            }
        }

        private void importFile(string fullFileName)
        {
            try
            {
                string fileName = Path.GetFileName(fullFileName);
                LoggerService.WriteLog(string.Format("Importando arquivo: {0}", fileName), LoggerService.LogType.info);

                using(StreamReader sr = new StreamReader(fullFileName))
                {
                    string line = null;
                    FileContent fileContent = new FileContent();
                    while ((line = sr.ReadLine()) != null)
                    {
                        processLine(fileContent, line);
                    }
                    sr.Close();
                    OutputFileContent outputFileContent = summarizeFile(fileContent);
                    saveReport(outputFileContent, fileName);
                    LoggerService.WriteLog(string.Format("Arquivo {0} processado", fileName), LoggerService.LogType.info);

                    string processedFile = Path.Combine(_fileImporterConfiguration.processedFilesDir, fileName);
                    moveAndReplaceFile(fullFileName, processedFile);
                }

            }
            catch(Exception ex)
            {
                LoggerService.WriteLog("Erro procesando arquivo.", LoggerService.LogType.error, ex);
            }
        }

        private void processLine(FileContent fileContent, string line)
        {
            string lineId = "";
            if (line.Length > 3)
                lineId = line.Substring(0, 3);
            switch (lineId)
            {
                case "001":
                    processSeller(fileContent, line);
                    break;
                case "002":
                    processCustomer(fileContent, line);
                    break;
                case "003":
                    processSales(fileContent, line);
                    break;
                default:
                    LoggerService.WriteLog(string.Format("Identificador de linha inválido, linha ignorada. Linha: {0}", line), LoggerService.LogType.alert);
                    break;
            }
        }

        /// <summary>
        /// Formato esperado para linha de vendedor
        /// 001ç3245678865434çPauloç40000.99
        /// </summary>
        private void processSeller(FileContent fileContent, string line)
        {
            try
            {
                string[] sellerLine = line.Split(_fileImporterConfiguration.separator);
                if (sellerLine.Length != 4)
                {
                    LoggerService.WriteLog(string.Format("Formato da linha de vendedor é inválido, linha ignorada. Linha: {0}", line), LoggerService.LogType.alert);
                    return;
                }

                Seller seller = new Seller();
                seller.CPF = sellerLine[1];
                seller.Name = sellerLine[2];
                seller.Salary = Convert.ToDecimal(sellerLine[3], new CultureInfo("en-US"));

                if (!CPFCNPJValidator.IsCpf(seller.CPF))
                    LoggerService.WriteLog(string.Format("Vendedor {0} contém um CPF inválido. CPF: {1}", seller.Name, seller.CPF), LoggerService.LogType.alert);

                fileContent.Sellers.Add(seller);
            }
            catch (Exception ex)
            {
                LoggerService.WriteLog(string.Format("Formato da linha de vendedor é inválido, Linha ignorada. Linha: {0}", line), LoggerService.LogType.error, ex);
            }
        }

        /// <summary>
        /// Formato esperado para linha de cliente
        /// 002ç2345675434544345çJose da SilvaçRural
        /// </summary>
        private void processCustomer(FileContent fileContent, string line)
        {
            string[] customerLine = line.Split(_fileImporterConfiguration.separator);
            if (customerLine.Length != 4)
            {
                LoggerService.WriteLog(string.Format("Formato da linha de cliente é inválido, linha ignorada. Linha: {0}", line), LoggerService.LogType.alert);
                return;
            }
            Customer customer = new Customer();
            customer.CNPJ = customerLine[1];
            customer.Name = customerLine[2];
            customer.BusinessArea = customerLine[3];

            if (!CPFCNPJValidator.IsCnpj(customer.CNPJ))
                LoggerService.WriteLog(string.Format("Cliente {0} contém um CNPJ inválido. CNPJ: {1}", customer.Name, customer.CNPJ), LoggerService.LogType.alert);

            fileContent.Customers.Add(customer);
        }

        /// <summary>
        /// Formato esperado para linha de vendas
        /// 003ç10ç[1-10-100,2-30-2.50,3-40-3.10]çPedro
        /// </summary>
        private void processSales(FileContent fileContent, string line)
        {
            string[] salesLine = line.Split(_fileImporterConfiguration.separator);
            if (salesLine.Length != 4)
            {
                LoggerService.WriteLog(string.Format("Formato da linha de vendas é inválido, linha ignorada. Linha: {0}", line), LoggerService.LogType.alert);
                return;
            }
            Sales sale = new Sales();
            sale.SaleId = Convert.ToInt32(salesLine[1]);
            sale.SalesmanName = salesLine[3];

            sale.SalesItens = processSalesItens(salesLine[2]);

            sale.SalesTotalValue = sale.SalesItens.Select(o => o.Price * o.Quantity).Sum();
            fileContent.Sales.Add(sale);
        }

        /// <summary>
        /// Formato esperado para Item da venda
        /// [1-10-100,2-30-2.50,3-40-3.10]
        /// </summary>
        private IEnumerable<SalesItem> processSalesItens(string itensField)
        {
            List<SalesItem> salesItens = new List<SalesItem>();
            try
            {
                string[] itens = itensField.Split(_fileImporterConfiguration.itensInitialMark, _fileImporterConfiguration.itensFinalMark)[1].Split(_fileImporterConfiguration.itensSeparator);
                foreach(string item in itens)
                {
                    SalesItem saleItem = processSalesItem(item);
                    if (saleItem != null)
                        salesItens.Add(saleItem);
                }

                return salesItens;
            }
            catch (Exception ex)
            {
                LoggerService.WriteLog(string.Format("Não foi possível processar a linha de vendas. Itens: {0}", itensField), LoggerService.LogType.error, ex);
                return salesItens; //Retorna vendas vazia
            }
        }

        private SalesItem processSalesItem(string itemField)
        {
            try
            {
                string[] itens = itemField.Split(_fileImporterConfiguration.itemSeparator);

                if (itens.Length != 3)
                {
                    LoggerService.WriteLog(string.Format("Formato da linha de item da venda é inválido, linha ignorada. Item: {0}", itemField), LoggerService.LogType.alert);
                    return null;
                }

                SalesItem saleItem = new SalesItem();
                saleItem.Id = Convert.ToInt32(itens[0]);
                saleItem.Quantity = Convert.ToInt32(itens[1]);
                saleItem.Price = Convert.ToDecimal(itens[2], new CultureInfo("en-US"));

                return saleItem;
            }
            catch (Exception ex)
            {
                LoggerService.WriteLog(string.Format("Não foi possível processar o item da venda. Item: {0}", itemField), LoggerService.LogType.error, ex);
                return null; //Retorna vendas vazia
            }
        }

        /// <summary>
        /// Gera dados do relatório
        /// * Quantidade de clientes no arquivo de entrada
        /// * Quantidade de vendedor no arquivo de entrada
        /// * ID da venda mais cara
        /// * O pior vendedor
        /// </summary>
        private OutputFileContent summarizeFile(FileContent fileContent)
        {
            OutputFileContent outputFileContent = new OutputFileContent();
            outputFileContent.CustomerCount = fileContent.Customers.Count;
            outputFileContent.SellerCount = fileContent.Sellers.Count;

            //Pode haver mais de uma venda com o mesmo valor
            decimal bestSale = 0;
            if (fileContent.Sales.Count > 0)
                bestSale = fileContent.Sales.Max(o => o.SalesTotalValue);
            
            List<Sales> totalBySaller = (from s in fileContent.Sales
                                   group s by new { s.SalesmanName } into grp
                                   select new Sales
                                   {
                                       SalesmanName = grp.Key.SalesmanName,
                                       SalesTotalValue = grp.Sum(o => o.SalesTotalValue)
                                   }).ToList();

            decimal worstSale = 0;
            if (totalBySaller.Count > 0)
                worstSale = totalBySaller.Min(o => o.SalesTotalValue);

            outputFileContent.ExpensiveSaleId = fileContent.Sales.Where(o => o.SalesTotalValue == bestSale).Select(o => o.SaleId).ToArray();
            outputFileContent.WorstSaller = totalBySaller.Where(o => o.SalesTotalValue == worstSale).Select(o => o.SalesmanName).ToArray();

            return outputFileContent;
        }

        private void saveReport(OutputFileContent outputFileContent, string fileName)
        {
            try
            {
                string outputFileName = fileName.ToLower().Replace(_fileImporterConfiguration.extentionFile.ToLower(), _fileImporterConfiguration.processedExtentionFile.ToLower());
                string outputString = Newtonsoft.Json.JsonConvert.SerializeObject(outputFileContent);

                LoggerService.WriteLog(string.Format("Criando relatório {0}", outputFileName), LoggerService.LogType.info);
                using (StreamWriter sw = new StreamWriter(Path.Combine(_fileImporterConfiguration.outputDir, outputFileName)))
                {
                    sw.WriteLine(outputString);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                LoggerService.WriteLog("Não foi possível mover arquivo", LoggerService.LogType.error, ex);
            }

        }

        private void ignoreFile(string fullFileName)
        {
            try
            {
                string fileName = Path.GetFileName(fullFileName);
                LoggerService.WriteLog(string.Format("Ignorando arquivo com extenção inválida. Arquivo: {0}", fileName), LoggerService.LogType.info);

                string ignoredFile = Path.Combine(_fileImporterConfiguration.ignoredFilesDir, fileName);
                moveAndReplaceFile(fullFileName, ignoredFile);
            }
            catch (Exception ex)
            {
                LoggerService.WriteLog("Não foi possível mover arquivo", LoggerService.LogType.error, ex);
            }
        }

        private void moveAndReplaceFile(string fullFileName, string fullOutPutFileName)
        {
            if (File.Exists(fullOutPutFileName))
            {
                File.Delete(fullOutPutFileName);
            }
            File.Move(fullFileName, fullOutPutFileName);

        }

    }
}
