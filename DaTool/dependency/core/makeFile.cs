using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public static class makeFile
{
    public static void startProcess()
    {
        string dir = Path.GetDirectoryName(Extract.path) + "/" + Path.GetFileNameWithoutExtension(Extract.path) + "_extracted/";
        Directory.CreateDirectory(Path.GetDirectoryName(Extract.path) + "/" + Path.GetFileNameWithoutExtension(Extract.path) + "_extracted/");


        //Console.WriteLine(offsets.allFileOffset.Length + "      " + offsets.allFileType.Length);

        byte[] file = File.ReadAllBytes(Extract.path);
        for (int i = 0; i < offsets.allFileOffset.Length;i++)
        {
            //dynamic[] file = hxg.gatherFromOffsetDuration(stream,reader, offsets.allFileOffset[i], offsets.allFileOffset[i] - offset,1,hxg.mode.int8);

            int target = file.Length;

            //Console.WriteLine(i + "    " + offsets.allFileOffset.Length);
            if (i + 1 < offsets.allFileOffset.Length)
                target = offsets.allFileOffset[i + 1];


                Directory.CreateDirectory(dir + offsets.allFileType[i]);
            string curDir = dir + offsets.allFileType[i] + "/" + offsets.allFileType[i] + (i) + "." + offsets.allFileType[i];



            int math = 0;
            if(target > offsets.allFileOffset[i])
            {
                math = target - offsets.allFileOffset[i];
            }
            if (target < offsets.allFileOffset[i])
            {
                math = offsets.allFileOffset[i] - target;
            }
            if (target - offsets.allFileOffset[i] == offsets.allFileOffset[i] || offsets.allFileOffset[i] - target == offsets.allFileOffset[i])
            {
                math = 0;
            }
            //Console.WriteLine(offsets.allFileOffset[i] + "      " + math + "    " + i + "     " + offsets.allFileOffset.Length);

            var v = File.Create(curDir);
            v.Write(file, offsets.allFileOffset[i], math);
            v.Dispose();
        }
        Console.WriteLine("File processed sucessfully!");
        Console.WriteLine("Saved data path: " + dir);
        Console.WriteLine("--------------------------");
        DaTool.Program.Main();


        //Console.ReadLine();

    }
}
