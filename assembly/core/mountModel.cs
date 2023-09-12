using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;



public static class mountModel
{
    public static List<modelHeader> modelInfo = new List<modelHeader>();

    public static void saveFile(FileStream stream, BinaryReader reader)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.BackgroundColor = ConsoleColor.Black;
        //Console.WriteLine("extracting model into a .SMD file...");
        //Console.WriteLine("MESHCOUNT: " + modelInfo.Count);
        for (int i = 0; i < modelInfo.Count; i++)
        {
            //Console.WriteLine(modelInfo[i].meshName);
            //Console.WriteLine(modelInfo[i].faceCount);
            //Console.WriteLine(modelInfo[i].faceCount);
            dynamic[] vertex = hx.gatherFromOffsetDuration(stream, reader, modelInfo[i].vertexOffset, modelInfo[i].faceCount * 16, 4, hx.mode.singleFloat);
            dynamic[] indx = hx.gatherFromOffsetDuration(stream, reader, modelInfo[i].vertexOffset, modelInfo[i].faceCount * 16, 4, hx.mode.int32);
            dynamic[] UVMapping = hx.gatherFromOffsetDuration(stream, reader, modelInfo[i].UVOffset, modelInfo[i].faceCount * 4, 2, hx.mode.int16);


            dynamic[] skeletonPos = hx.gatherFromOffsetDuration(stream, reader, skeletonHeader.boneStartOffset, skeletonHeader.boneCount * 16, 4, hx.mode.singleFloat);
            dynamic[] skeletonID = hx.gatherFromOffsetDuration(stream, reader, skeletonHeader.boneStartOffset, skeletonHeader.boneCount * 16, 2, hx.mode.int16);


            createValveSMD(vertex, indx, modelInfo[i].usedTextureIndexOnDat,modelInfo[i].meshName,UVMapping,skeletonPos,skeletonID);
        }
        string g = Console.ReadLine();
    }
    static void createValveSMD(dynamic[] verts, dynamic[] indices, int materialIndex, string meshName, dynamic[] UVMapping,dynamic[] skeletonPos, dynamic[] skeletonID)
    {
        //Path.GetFileName(ExtractModel.name);
        string partialDirectory = Path.GetDirectoryName(ExtractModel.savePath) + "/" + Path.GetFileNameWithoutExtension(ExtractModel.name);
        Directory.CreateDirectory(partialDirectory);


        Console.WriteLine(partialDirectory);
        string name = "/" + removeIllegalChars(meshName);
        string dataPath = partialDirectory + name + ".smd";
        
        //if (File.Exists(dataPath))
        //{
        //    for (int p = 0; p < 60; p++)
         //   {
         //       name = "/" + removeIllegalChars(meshName);
         //       dataPath = partialDirectory + name + p + ".smd";
         //       if (!File.Exists(dataPath)) break;
          //  }
        //}
        //Console.WriteLine(dataPath);


        using (var saveStream = new FileStream(dataPath, FileMode.Create))
        {
            using (var writterStream = new StreamWriter(saveStream))
            {

                writterStream.WriteLine("version 1");
                writterStream.WriteLine("nodes");
                //writterStream.WriteLine("0 \"root\" -1");


                //List<skeletonHierarchy> idList = new List<skeletonHierarchy>();
                int numbers = 0;

                for (int i = 0; i < skeletonID.Length; i += 8)
                {
                    if (i + 1 < skeletonID.Length)
                    {
                        //Console.WriteLine("0 \"" + skeletonID[i + 7] + "\" " + skeletonID[i + 6]);
                        int curID = skeletonID[i + 7];

                        //writterStream.WriteLine(numbers + " \"" + skeletonID[i + 6].ToString("X") + skeletonID[i + 7].ToString("X") + "\" " + "-1");
                        writterStream.WriteLine(numbers + " \"" + skeletonID[i + 6].ToString("X") + skeletonID[i + 7].ToString("X") + "\" " + "-1");
                        numbers += 1;

                    }
                }
                List<Vector3> vector3 = new List<Vector3>();
                for (int i = 0; i < skeletonPos.Length; i += 4)
                {
                    Vector3 placeHolder = new Vector3();
                    int tmp = skeletonID[(i * 2) + 7];
                    placeHolder.additiveOn = tmp;



                    float x = skeletonPos[i + 0];
                    float y = skeletonPos[i + 1];
                    float z = skeletonPos[i + 2];
                    placeHolder.Matrix3 = new float[] { x, y, z };

                    //if (tmp < 0)
                    //Console.WriteLine(placeHolder.additiveOn);
                    vector3.Add(placeHolder);
                }


                for (int i = 0; i < vector3.Count; i++)
                {
                    int tmp = vector3[i].additiveOn;

                    float x2 = 0;
                    float y2 = 0;
                    float z2 = 0;

                    if (tmp > -1)
                    {
                        x2 = vector3[vector3[i].additiveOn].Matrix3[0];
                        y2 = vector3[vector3[i].additiveOn].Matrix3[1];
                        z2 = vector3[vector3[i].additiveOn].Matrix3[2];
                    }

                    float x1 = vector3[i].Matrix3[0];
                    float y1 = vector3[i].Matrix3[1];
                    float z1 = vector3[i].Matrix3[2];

                    float resultX = x1 + x2;
                    float resultY = y1 + y2;
                    float resultZ = z1 + z2;

                    vector3[i].Matrix3 = new float[] {resultX,resultY,resultZ };
                }



                    writterStream.WriteLine("end");
                writterStream.WriteLine("skeleton");


                writterStream.WriteLine("time 0");
                //writterStream.WriteLine("0  0.000000 0.000000 0.000000  0.000000 -0.000000 0.000000");


                numbers = 0;
                float xRootAdditive = 0;
                float yRootAdditive = 0;
                float zRootAdditive = 0;


                for (int i = 0; i < vector3.Count; i += 1)
                {
                    //Console.WriteLine("ADDITIVEON " + vector3[i].additiveOn);


                    int curID = vector3[i].additiveOn;
                    //if (curID < 0) curID = 0;



                    xRootAdditive = vector3[i].Matrix3[0];
                    yRootAdditive = vector3[i].Matrix3[1];
                    zRootAdditive = vector3[i].Matrix3[2];



                    string x = hx.returnDecimal(vector3[i].Matrix3[0], 8);
                    string y = hx.returnDecimal(vector3[i].Matrix3[1], 8);
                    string z = hx.returnDecimal(vector3[i].Matrix3[2], 8);



                    writterStream.WriteLine(numbers + "  " + x + " " + y + " " + z + "  " + "1.000000" + " " + "1.000000" + " " + "1.000000");
                    numbers += 1;
                }

                writterStream.WriteLine("end");
                writterStream.WriteLine("triangles");


                //Console.WriteLine(verts.Length + "    LENGHT");
                //Console.WriteLine(UVMapping.Length + "    UVMAPS");
                for (int i = 0; i < verts.Length; i += 4)
                {
                    //THIS LINE REPEATS EXATCLY 88 TIMES


                    //Console.WriteLine(indices[i + 3]);

                    //Console.WriteLine("repeats: " + i);
                    if (indices[i + 3].ToString() != "32768")
                    {
                        writterStream.WriteLine(materialIndex); //MATERIAL NAME
                        string x1 = hx.returnDecimal(verts[i - 8], 8); //vertex are in reverse order
                        string y1 = hx.returnDecimal(verts[i - 7], 8);
                        string z1 = hx.returnDecimal(verts[i - 6], 8);

                        string x2 = hx.returnDecimal(verts[i - 4], 8);
                        string y2 = hx.returnDecimal(verts[i - 3], 8);
                        string z2 = hx.returnDecimal(verts[i - 2], 8);

                        string x3 = hx.returnDecimal(verts[i + 0], 8); //cur
                        string y3 = hx.returnDecimal(verts[i + 1], 8);
                        string z3 = hx.returnDecimal(verts[i + 2], 8);

                        string uvMapX1 = hx.returnDecimal(UVMapping[(i / 2) - 4] / 4100f, 8); //uv's are in reverse too
                        string uvMapY1 = hx.returnDecimal(UVMapping[(i / 2) - 3] / 4100f * -1, 8);

                        string uvMapX2 = hx.returnDecimal(UVMapping[(i / 2) - 2] / 4100f, 8);
                        string uvMapY2 = hx.returnDecimal(UVMapping[(i / 2) - 1] / 4100f * -1, 8);

                        string uvMapX3 = hx.returnDecimal(UVMapping[(i / 2) + 0] / 4100f, 8);
                        string uvMapY3 = hx.returnDecimal(UVMapping[(i / 2) + 1] / 4100f * -1, 8);


                        //writterStream.WriteLine("0  " + x1 + " " + y1 + " " + z1 + "  " + "0.000000 0.000000 0.000000  0.000000 0.000000 0 ");
                        //writterStream.WriteLine("0  " + x2 + " " + y2 + " " + z2 + "  " + "0.000000 0.000000 0.000000  0.000000 0.000000 0 ");
                        //writterStream.WriteLine("0  " + x3 + " " + y3 + " " + z3 + "  " + "0.000000 0.000000 0.000000  0.000000 0.000000 0 ");

                        writterStream.WriteLine("0  " + x1 + " " + y1 + " " + z1 + "  " + "0.000000 0.000000 0.000000  " + uvMapX1 + " " + uvMapY1 + " 0 ");
                        writterStream.WriteLine("0  " + x2 + " " + y2 + " " + z2 + "  " + "0.000000 0.000000 0.000000  " + uvMapX2 + " " + uvMapY2 + " 0 ");
                        writterStream.WriteLine("0  " + x3 + " " + y3 + " " + z3 + "  " + "0.000000 0.000000 0.000000  " + uvMapX3 + " " + uvMapY3 + " 0 ");

                        //UVMapping
                    }
                }
                writterStream.WriteLine("end");
            }
        }
        //Console.WriteLine("Valve SMD created successfully on path " + dataPath + ".");
    }
    
    public static void addOnList(modelHeader md)
    {
        modelInfo.Add(md);
        //Console.WriteLine(md.boneWeightOffset);
    }
    static string removeIllegalChars(string path)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string sanitizedPath = string.Join("_", path.Split(invalidChars));

        return sanitizedPath;
    }
}
