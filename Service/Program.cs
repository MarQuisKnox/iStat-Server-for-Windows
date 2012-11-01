using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace iStatServerService
{
    internal class Program 
    {
        /// <summary>
        /// Sets log level for entire program
        /// </summary>
        public static TraceSwitch LogLevel = new TraceSwitch("istatServer", "Current Log Level");


        [STAThread]
        private static void Main2(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LogLevel.Level = TraceLevel.Verbose;
            Trace.Listeners.Add(new ConsoleTraceListener(false));
            using (var a = new iStatServerService())
            {
                Application.Run();
            }
        }




    }
   
}

