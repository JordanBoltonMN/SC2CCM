using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModManager.StarCraft.Base
{
    public static class Prompter
    {
        public const string Caption = "StarCraft II Custom Campaign Manager";

        public static bool AskYesNo(string message)
        {
            return MessageBox.Show(message, Caption, MessageBoxButtons.YesNo) == DialogResult.Yes;
        }
    }
}
