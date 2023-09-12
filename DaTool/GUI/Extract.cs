using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Extract
{
    public static string path;
    public static void onGUI()
    {
        Console.WriteLine("------------------");
        Console.WriteLine("Drag and drop the file inside this tab");
        Console.WriteLine("(make sure the file path don't contain any kind of blank space)");
        Console.Write("->");
        path = Console.ReadLine();
        offsets.gatherOffsets(path);
        
    }
}
