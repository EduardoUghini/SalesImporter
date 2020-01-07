using System;
using System.Configuration;
using System.IO;

namespace FileImporter.Services
{
    public static class LoggerService
    {
        public static string logDir { get; set; }
        public static bool disableLogger { get; set; }
        public enum LogType
        {
            info = 0,
            alert = 1,
            error = 2
        }

        public static void WriteLog(string message, LogType logType, Exception ex = null)
        {
            if (disableLogger)
                return;
            string formatedMessage = message;
            string formatedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            switch (logType)
            {
                case LogType.info:
                    formatedMessage = string.Format("{0} INFO => {1}", formatedDate, message);
                    break;
                case LogType.alert:
                    formatedMessage = string.Format("{0} ALERT => {1}", formatedDate, message);
                    break;
                case LogType.error:
                    if (ex != null)
                        formatedMessage = string.Format("{0} ERROR ===> {1} Mensagem de erro: {2}", formatedDate, message, ex.Message);
                    else
                        formatedMessage = string.Format("{0} ERROR ===> {1} Mensagem de erro não informada.", formatedDate, message);
                    break;
                default:
                    break;
            }

            string logFile = Path.Combine(logDir, "log_" + DateTime.Now.ToString("yyyy-MM") + ".log");
            using (StreamWriter streamWriter = new StreamWriter(logFile, true))
            {
                streamWriter.WriteLine(formatedMessage);
                streamWriter.Close();
            }
        }

        /// <summary>
        /// Configura o diretório de logs
        /// </summary>
        public static void ConfigureLogDirectory()
        {
            if (!Directory.Exists(logDir))
            {
                try
                {
                    Directory.CreateDirectory(logDir);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

    }
}
