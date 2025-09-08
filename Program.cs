using System;
using System.Windows.Forms;

namespace ReservaCine
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); // Form 100% por codigo
        }
    }
}
