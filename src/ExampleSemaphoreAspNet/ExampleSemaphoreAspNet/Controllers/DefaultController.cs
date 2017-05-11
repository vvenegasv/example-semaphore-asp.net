using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ExampleSemaphoreAspNet.Controllers
{
    public class DefaultController : Controller
    {        
        private static volatile bool _isRunning;
        private static volatile string _processId;
        private static object _syncRoot = new Object();
        private readonly ILog _logger;

        public DefaultController()
        {
            log4net.Config.XmlConfigurator.Configure();
            _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public ActionResult Index()
        {
            try
            {
                //Set is running to true. It tell's to another request that cannot be processed
                if (!_isRunning)
                {
                    lock (_syncRoot)
                    {
                        if (!_isRunning)
                        {
                            _processId = Guid.NewGuid().ToString();
                            _isRunning = true;
                        }
                        else
                        {
                            _logger.Info("The process is running. You cannot start a new process until it finished");
                            ViewBag.Message = "The process is running. You cannot start a new process until it finished";
                            return View();
                        }
                    }
                }
                else
                {
                    _logger.Info("The process is running. You cannot start a new process until it finished");
                    ViewBag.Message = "The process is running. You cannot start a new process until it finished";
                    return View();
                }

                _logger.Info($"A new process was started. The process id is {_processId}");
                ViewBag.Message = $"A new process was started. The process id is {_processId}";
                Task.Factory.StartNew(() => DoWork());
            }
            catch (Exception ex)
            {
                _logger.Error($"There is an unexpected exception.", ex);
                ViewBag.Message = $"There is an unexpected exception. The message is '{ex.Message}'";
            }
            return View();
        }


        private async void DoWork()
        {
            _logger.Info($"{_processId}: Started intensive work");

            //Wait for a 30 seconds
            await Task.Delay(30000);            

            lock (_syncRoot)
            {
                _isRunning = false;
            }

            _logger.Info($"{_processId}: Finished intensive work");
        }
    }
}