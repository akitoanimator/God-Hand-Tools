using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            string draggedFile = args[0];
            if (File.Exists(draggedFile))
            {
                ExtractModel.skipGUI(draggedFile) ;
            }
        }
        Console.WriteLine("input '1' to extract a model.");
        Console.WriteLine("input '2' to convert a model. (WIP)");
        //Console.WriteLine("input '3' to open the model viewer.");
        string ans = Console.ReadLine();
        if (!ans.Contains("1") && !ans.Contains("2") && !ans.Contains("3"))
            Main(new string[] { "" });
        else
            Answer.analyseAnswer(ans);
    }
}
