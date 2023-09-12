using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

class hxg
{
    public enum mode { int32, int16, int8, singleFloat, doubleFloat, singleString };
    public static dynamic[] gatherFromOffsetDuration(FileStream stream, BinaryReader reader, int startOffset, int endOffset, int additive, mode numberMode)
    {
        List<dynamic> returnContent = new List<dynamic>();
        for (int i = startOffset; i < endOffset + startOffset; i += additive)
        {
            stream.Seek(i, SeekOrigin.Begin);
            dynamic v = 0;
            //Console.WriteLine(i);
            switch (numberMode)
            {
                case mode.int32: v = reader.ReadInt32(); break;
                case mode.int16: v = reader.ReadInt16(); break;
                case mode.int8: v = reader.ReadByte(); break;
                case mode.singleFloat: v = reader.ReadSingle(); break;
                case mode.doubleFloat: v = reader.ReadDouble(); break;
            }
            returnContent.Add(v);
            //Console.WriteLine(v);
        }
        return returnContent.ToArray();
    }

    public static string[] gatherStringFromDuration(FileStream stream, BinaryReader reader, int startOffset, int endOffset, int additive)
    {
        List<string> returnContent = new List<string>();
        for (int i = startOffset; i < endOffset + startOffset; i += additive)
        {
            stream.Seek(i, SeekOrigin.Begin);
            string v = "";
            string r1 = string.Concat(reader.ReadChars(4));

            v = r1.Split(Path.GetInvalidFileNameChars())[0];
            returnContent.Add(v);
        }
        return returnContent.ToArray();
    }


    public static dynamic gatherSingleNumberFromOffset(FileStream stream, BinaryReader reader, int offset, int multiplier, mode numberMode)
    {
        stream.Seek(offset, SeekOrigin.Begin);
        dynamic returnContent = 0;
        switch (numberMode)
        {
            case mode.int32: returnContent = reader.ReadInt32() * multiplier; break;
            case mode.int16: returnContent = reader.ReadInt16() * multiplier; break;
            case mode.int8: returnContent = reader.ReadByte() * multiplier; break;
            case mode.singleFloat: returnContent = reader.ReadSingle() * multiplier; break;
            case mode.doubleFloat: returnContent = reader.ReadDouble() * multiplier; break;
            case mode.singleString: returnContent = string.Concat(reader.ReadChars(4)); break;
        }
        //Console.WriteLine(returnContent * multiplier);
        return returnContent;
    }
    public static string returnDecimal(dynamic number, int strictLength)
    {
        string verySmallNumberString = number.ToString();
        decimal convertedVerySmallDecimal = Decimal.Parse(verySmallNumberString, System.Globalization.NumberStyles.Float);
        string curString = convertedVerySmallDecimal.ToString().Replace(",", ".");
        string pass1 = "";
        if (curString.Length > strictLength)
        {
            if (curString.Contains("-"))
            {
                string tmpString = curString.Replace("-", "");
                pass1 = curString.Substring(0, strictLength + 1);
            }
            else
            {
                pass1 = curString.Substring(0, strictLength);
            }
        }
        else
        {
            for (int i = curString.Length; i < strictLength; i++)
            {
                if (curString.Contains("."))
                {
                    curString = curString + "0";
                }
                else
                {
                    curString = curString + ".";
                }
            }
            pass1 = curString;
        }
        if (!curString.Contains("-"))
        {
            string tmp = pass1;
            //pass1 = " " + tmp ;
        }
        return pass1;
    }



    public static dynamic[] gatherFromOffsetArrays(FileStream stream, BinaryReader reader, dynamic[] array, mode numberMode)
    {
        List<dynamic> returnContent = new List<dynamic>();
        for (int i = 0; i < array.Length; i++)
        {
            stream.Seek(array[i], SeekOrigin.Begin);
            dynamic v = 0;
            switch (numberMode)
            {
                case mode.int32: v = reader.ReadInt32(); break;
                case mode.int16: v = reader.ReadInt16(); break;
                case mode.int8: v = reader.ReadByte(); break;
                case mode.singleFloat: v = reader.ReadSingle(); break;
                case mode.doubleFloat: v = reader.ReadDouble(); break;
            }
            returnContent.Add(v);
            Console.WriteLine(v);
        }
        return returnContent.ToArray();
    }
    public static dynamic concactOffsets(dynamic[] array1, dynamic[] array2)
    {
        List<dynamic> arrayReturn = new List<dynamic>();
        for (int i = 0; i < array1.Length; i++)
        {
            arrayReturn.Add(array1[i] + array2[i]);
            Console.WriteLine(array1[i] + array2[i]);
        }
        return arrayReturn.ToArray();
    }

}
