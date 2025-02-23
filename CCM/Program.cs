using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ModManager.StarCraft.Base;

namespace Starcraft_Mod_Manager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(
                new ProgramForm(
                    new RollingFileLogger(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "SCC",
                        1024 * 1024 * 100 /* 100 megabyte */
                    )
                )
            );
        }
    }
}
