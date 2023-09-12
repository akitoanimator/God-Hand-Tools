using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaTool
{
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("DaTool by AKITO");
            Console.WriteLine("Input [1] to Extract a .DAT file.");
            Console.WriteLine("Input [2] to Repack into a .DAT file(WIP).");
            Console.WriteLine("Input [3] to View the .DAT contents(WIP).");

            string answer = Console.ReadLine();
            switch(answer)
            {
                case "1":
                    Extract.onGUI();
                    break;
                case "2":
                    Repack.onGUI();
                    break;
                case "3":
                    View.onGUI();
                    break;
                case "":
                    Main();
                    break;
                case " ":
                    Main();
                    break;
            }
        }
    }
}
