using FileImporter.Model;
using FileImporter.Services;
using System;
using System.Configuration;
using System.IO;
using System.ServiceProcess;

namespace SalesImporter
{
    public partial class SalesService : ServiceBase
    {
        private FileImporterConfiguration _fileImporterConfiguration { get; set; }
        public SalesService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _fileImporterConfiguration = readConfigFile();
            initializeDirectories();

            //Prmeiro processa todos os arquivos pendentes
            processAllPendingFiles();

            //Inicializa monitoramento do diretório
            startMonitor();
        }

        private void processAllPendingFiles()
        {
            try
            {
                LoggerService.WriteLog(string.Format("Iniciando leitura do diretório {0}", _fileImporterConfiguration.inputDir), LoggerService.LogType.info);
                string[] directoryFiles = Directory.GetFiles(_fileImporterConfiguration.inputDir);

                FileImporterService fileImporterService = new FileImporterService(_fileImporterConfiguration);
                foreach (string fullFileName in directoryFiles)
                {
                    fileImporterService.ProcessFile(fullFileName);
                }

            }
            catch (Exception ex)
            {
                LoggerService.WriteLog("Ocorreu um erro durante a leitura do dretório de entrada.", LoggerService.LogType.error, ex);
            }
        }

        protected override void OnStop()
        {
        }

        /// <summary>
        /// Para testar via debug
        /// </summary>
        internal void TestStartupAndStop(string[] args)
        {
            OnStart(args);
            Console.ReadLine();
            OnStop();
        }

        private void startMonitor()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = _fileImporterConfiguration.inputDir;
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;

            watcher.Created += new FileSystemEventHandler(OnCreated);
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            LoggerService.WriteLog(string.Format("Novo arquivo encontrado. Arquivo: {0}", e.FullPath), LoggerService.LogType.info);
            FileImporterService fileReader = new FileImporterService(_fileImporterConfiguration);
            fileReader.ProcessFile(e.FullPath);
        }

        private FileImporterConfiguration readConfigFile()
        {
            FileImporterConfiguration fileImporterConfiguration = new FileImporterConfiguration()
            {
                baseDir = ConfigurationManager.AppSettings["baseDir"].ToString(),
                inputDir = ConfigurationManager.AppSettings["inputDir"].ToString(),
                outputDir = ConfigurationManager.AppSettings["outputDir"].ToString(),
                processedFilesDir = ConfigurationManager.AppSettings["processedFilesDir"].ToString(),
                ignoredFilesDir = ConfigurationManager.AppSettings["ignoredFilesDir"].ToString(),
                logDir = ConfigurationManager.AppSettings["logDir"].ToString(),
                disableLogger = Convert.ToBoolean(ConfigurationManager.AppSettings["disableLogger"]),

                extentionFile = ConfigurationManager.AppSettings["extentionFile"].ToString(),
                processedExtentionFile = ConfigurationManager.AppSettings["processedExtentionFile"].ToString(),
                separator = ConfigurationManager.AppSettings["separator"].ToCharArray(),
                itensSeparator = ConfigurationManager.AppSettings["itensSeparator"].ToCharArray(),
                itemSeparator = ConfigurationManager.AppSettings["itemSeparator"].ToCharArray(),
                itensInitialMark = Convert.ToChar(ConfigurationManager.AppSettings["itensInitialMark"]),
                itensFinalMark = Convert.ToChar(ConfigurationManager.AppSettings["itensFinalMark"]),
            };

            return fileImporterConfiguration;
        }


        #region Directories configuration
        private void initializeDirectories()
        {
            if (_fileImporterConfiguration.baseDir == "")
            {
                _fileImporterConfiguration.baseDir = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            }
            initializeLogger();
            configureBaseDirectories();
        }

        private void initializeLogger()
        {
            try
            {
                LoggerService.logDir = Path.Combine(_fileImporterConfiguration.baseDir, _fileImporterConfiguration.logDir);
                LoggerService.disableLogger = _fileImporterConfiguration.disableLogger;
                LoggerService.ConfigureLogDirectory();
            }
            catch (Exception ex)
            {
                throw new Exception("Não foi possível inicializar diretório para logs, verifique o arquivo de configuração.", ex);
            }
        }

        private void configureBaseDirectories()
        {
            if (!Directory.Exists(_fileImporterConfiguration.baseDir))
                LoggerService.WriteLog("Não foi possível localizar diretório base, verifique o arquivo de configuração! ", LoggerService.LogType.alert);

            //Cria diretórios de entrada e saida
            _fileImporterConfiguration.inputDir = setDirectory(_fileImporterConfiguration.inputDir);
            _fileImporterConfiguration.outputDir = setDirectory(_fileImporterConfiguration.outputDir);
            _fileImporterConfiguration.processedFilesDir = setDirectory(_fileImporterConfiguration.processedFilesDir);
            _fileImporterConfiguration.ignoredFilesDir = setDirectory(_fileImporterConfiguration.ignoredFilesDir);
        }

        private string setDirectory(string dirName)
        {
            string fullPath = Path.Combine(_fileImporterConfiguration.baseDir, dirName);
            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                }
                catch (Exception ex)
                {
                    LoggerService.WriteLog(string.Format("Erro inicializando diretório {0}", dirName), LoggerService.LogType.error, ex);
                    return null;
                }
            }
            return fullPath;
        }
        #endregion
    }
}
