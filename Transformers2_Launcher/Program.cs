using System;
using System.Windows.Forms;


namespace Transformers2_Launcher
{
    public static class Program
    {        
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool bEnableLogs = false;
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].ToLower().Equals("-v") || args[i].ToLower().Equals("--verbose"))
                    {
                        bEnableLogs = true;
                        break;
                    }
                }
            }

            Transformers2_Launcher Launcher = new Transformers2_Launcher(bEnableLogs);
            Launcher.RunGame();
            //Launcher.Run_Game_Debug();
            Application.Exit();
        }
    }
}
