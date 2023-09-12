using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCRTool
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("input 'e' to extract a model.");
            Console.WriteLine("input 'c' to convert a model.");
            Console.WriteLine("input 'm' to open the model viewer.");
            string ans = Console.ReadLine();
            if (!ans.Contains("e") && !ans.Contains("c") && !ans.Contains("m"))
                Main();
            else
                Answer.analyseAnswer(ans);
        }
    }
}
