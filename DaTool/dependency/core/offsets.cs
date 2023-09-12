using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public static class offsets
{
    public static int fileCounter = 0;
    public static dynamic[] allFileOffset;
    public static string[] allFileType;
    public static void gatherOffsets(string path)
    {
        using (var stream = new FileStream(path,FileMode.Open))
        using (var reader = new BinaryReader(stream))
        {
            fileCounter = hxg.gatherSingleNumberFromOffset(stream,reader,0,1,hxg.mode.int32);
            allFileOffset = hxg.gatherFromOffsetDuration(stream,reader,4,fileCounter * 4,4,hxg.mode.int32);
            allFileType = hxg.gatherStringFromDuration(stream, reader, fileCounter * 4 + 4, fileCounter * 4, 4);
            reader.Dispose();
            stream.Dispose();
            makeFile.startProcess();
        }
    }
}
