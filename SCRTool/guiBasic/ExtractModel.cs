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

        Console.WriteLine("Drag and drop the model in this window and hit enter(BE SURE THAT THE FILE IS IN A FOLDER THAT DOESN'T CONTAIN ANY BLANK SPACE).");
        string r1 = Console.ReadLine();
        savePath = r1;
        name = Path.GetFileName(r1);

        offsets.process(r1);

    }
}
