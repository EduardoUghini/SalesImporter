using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SalesImporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                var service = new SalesService();
                service.TestStartupAndStop(null);

                //Para testar em debug essa linha deixa o serviço rodando por 1 minuto
                System.Threading.Thread.Sleep(60000);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { new SalesService() };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
