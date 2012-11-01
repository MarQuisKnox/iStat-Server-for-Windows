using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace iStatServerService
{
    class Logger
    {
        static Logger instance = null;
        static readonly object padlock = new object();
        public EventLog eventLog;

        Logger()
        {
        }

        public static Logger Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Logger();
                    }
                    return instance;
                }
            }
        }

        public void OpenLog()
        {
            this.eventLog = new System.Diagnostics.EventLog();
            this.eventLog.Source = "iStatServerService";
            this.eventLog.Log = "Application";

            eventLog.WriteEntry("Starting...", EventLogEntryType.Information);

            ((System.ComponentModel.ISupportInitialize)(this.eventLog)).BeginInit();
            if (!EventLog.SourceExists(this.eventLog.Source))
            {
                EventLog.CreateEventSource(this.eventLog.Source, this.eventLog.Log);
            }
            ((System.ComponentModel.ISupportInitialize)(this.eventLog)).EndInit();
        }
    }
}
