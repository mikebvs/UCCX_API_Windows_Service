using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;

namespace UCCX_API_Service
{
    public partial class UCCXAPIService : ServiceBase
    {
        private int eventId = 1;
        private APIHandler apiHandler;
        private ExcelData excelData;
        System.Timers.Timer _timer;
        //First Runtime
        DateTime _scheduleTime;
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };
        public UCCXAPIService(string[] args)
        {
            InitializeComponent();
            
            // Set Event Log based on input args if passed, otherwise use default
            string eventSourceName = "UCCX API Service";
            string logName = "UCCX API Log";

            if (args.Length > 0)
            {
                eventSourceName = args[0];
            }
            if (args.Length > 1)
            {
                logName = args[1];
            }
            eventLog1 = new EventLog();
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }

            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;

            // Create Timer Object on Init
            _timer = new System.Timers.Timer();
            _scheduleTime = DateTime.Now.AddMinutes(15);
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            eventLog1.WriteEntry("Service Initiating...", EventLogEntryType.Information, eventId++);

            //OLD TIMER, ONLY EXECUTED ON INTERVAL
            //Initialize Timer Object to poll when the service should run
            //Timer timer = new Timer();
            //timer.Interval = 120000; // 2 minutes
            //timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            //timer.Start();


            // Set initial Interval to the difference in milliseconds from current time and the scheduled time
            _timer.Enabled = true;
            _timer.Interval = _scheduleTime.Subtract(DateTime.Now).TotalSeconds * 1000;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);


            // Initialize APIHandler to route commands through
            apiHandler = new APIHandler();
            apiHandler.SetEventLog(eventLog1);
            apiHandler.Refresh(ref eventId);
            
            // Initialize ExcelData
            excelData = new ExcelData(eventLog1);
            excelData.Refresh(apiHandler.cm, ref eventId, eventLog1);
            eventId++;            
            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            eventLog1.WriteEntry("Service Started.", EventLogEntryType.Information, eventId++);
        }
        // THIS METHOD FIRES ON THE OLD TIMER'S ELAPSED EVENT
        //public void OnTimer(object sender, ElapsedEventArgs args)
        //{
        //    // Refresh APIHandler Configuration Information
        //    apiHandler.Refresh(ref eventId);
        //    // Refresh APIData Information
        //    excelData.Refresh(apiHandler.cm, ref eventId, eventLog1);
        //    eventId++;
        //    // Begin API Agent Queue Update
        //    eventLog1.WriteEntry("Beginning Agent Queue Update.", EventLogEntryType.Information, eventId++);
        //    apiHandler.ExcelQueueUpdate(excelData, ref eventId);
        //}
        protected void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Refresh APIHandler Configuration Information
            apiHandler.Refresh(ref eventId);
            // Refresh APIData Information
            excelData.Refresh(apiHandler.cm, ref eventId, eventLog1);
            eventId++;
            // Begin API Agent Queue Update
            eventLog1.WriteEntry("Beginning Agent Queue Update.", EventLogEntryType.Information, eventId++);
            apiHandler.ExcelQueueUpdate(excelData, ref eventId);

            // Set Timer Interval for next runtime to 1 hour in the future
            if (_timer.Interval != 1 * 60 * 60 * 1000)
            {
                _timer.Interval = 1 * 60 * 60 * 1000;
            }
        }
        protected override void OnStop()
        {

            // Update the service state to Stop Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            eventLog1.WriteEntry("Service is stopping...", EventLogEntryType.Information, eventId++);

            // Update the service state to Stopped.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            eventLog1.WriteEntry("Service has stopped.", EventLogEntryType.Information, eventId++);
        }
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);
    }
}
