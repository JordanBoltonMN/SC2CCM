using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ModManager.StarCraft.Base.Tracing;

namespace Starcraft_Mod_Manager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(
                new FormMain(
                    new RollingFileLogger(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "SMM_Log",
                        1024 * 1024 /* 1 megabyte */
                    )
                )
            );
        }
    }
}
