using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

public static class ExtractModel
{
    public static string savePath;
    public static string name;
    public static void GUI()
    {

        Console.WriteLine("Drag and drop the model in this window and hit enter \n(be sure that the path does not have any blank space)");
        string r1 = Console.ReadLine().Replace("\"", "");
        savePath = r1;
        name = Path.GetFileName(r1);

        offsets.process(r1);

    }

    public static void skipGUI(string path)
    {

        string r1 = path.Replace("\"", "");
        savePath = r1;
        name = Path.GetFileName(r1);

        offsets.process(r1);

    }
}
