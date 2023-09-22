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

            dynamic[] Bone_weight = hx.gatherFromOffsetDuration(stream, reader, modelInfo[i].boneWeightOffset, modelInfo[i].faceCount * 8, 1, hx.mode.int8);


            createValveSMD(vertex, indx, modelInfo[i].usedTextureIndexOnDat, modelInfo[i].meshName, UVMapping, skeletonPos, skeletonID, Bone_weight);
        }
        Console.WriteLine("export finished!");
    }
    static void createValveSMDBKP(dynamic[] verts, dynamic[] indices, int materialIndex, string meshName, dynamic[] UVMapping, dynamic[] skeletonPos, dynamic[] skeletonID, dynamic[] boneWeight)
    {
        //Path.GetFileName(ExtractModel.name);
        string partialDirectory = Path.GetDirectoryName(ExtractModel.savePath) + "/" + Path.GetFileNameWithoutExtension(ExtractModel.name);
        Directory.CreateDirectory(partialDirectory);


        //Console.WriteLine(partialDirectory);
        string name = "/" + removeIllegalChars(meshName);
        string dataPath = partialDirectory + name + ".smd";
        Console.WriteLine(removeIllegalChars("-> " + meshName));

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
                List<BONE_MESHPAINT> bone = new List<BONE_MESHPAINT>();

                for (int i = 0; i < boneWeight.Length; i += 8)
                {
                    BONE_MESHPAINT placeholder = new BONE_MESHPAINT();

                    placeholder.boneID = new int[] { (boneWeight[i + 1] / 4), (boneWeight[i + 2] / 4), (boneWeight[i + 3] / 4)};
                    placeholder.Weight = new float[] { (boneWeight[i + 4] / 100f), (boneWeight[i + 5] / 100f), (boneWeight[i + 6] / 100f) };
                    bone.Add(placeholder);
                }

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
                        writterStream.WriteLine(numbers + " \"" + skeletonID[i + 6].ToString("X") + "/" + skeletonID[i + 7].ToString("X") + "_" + i + "_" + "\" " + curID);
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

                    float resultX = x1;
                    float resultY = y1;
                    float resultZ = z1;

                    vector3[i].Matrix3 = new float[] { resultX, resultY, resultZ };
                }



                writterStream.WriteLine("end");
                writterStream.WriteLine("skeleton");


                writterStream.WriteLine("time 0");
                //writterStream.WriteLine("0  0.000000 0.000000 0.000000  0.000000 -0.000000 0.000000");


                numbers = 0;



                for (int i = 0; i < vector3.Count; i += 1)
                {
                    int curID = vector3[i].additiveOn;
                    if (curID < 0) curID = 0;
                    string x = hx.returnDecimal(vector3[i].Matrix3[0], 8);
                    string y = hx.returnDecimal(vector3[i].Matrix3[1], 8);
                    string z = hx.returnDecimal(vector3[i].Matrix3[2], 8);
                    string xEnd = "0.000000";
                    string yEnd = "0.000000";
                    string zEnd = "0.000000";
                    writterStream.WriteLine(numbers + "  " + x + " " + y + " " + z + "  " + xEnd + " " + yEnd + " " + zEnd);
                    numbers += 1;
                }

                writterStream.WriteLine("end");
                writterStream.WriteLine("triangles");


                //Console.WriteLine(verts.Length + "    LENGHT");
                //Console.WriteLine(UVMapping.Length + "    UVMAPS");
                for (int i = 0; i < verts.Length; i += 1)
                {
                    //THIS LINE REPEATS EXATCLY 88 TIMES


                    //Console.WriteLine(indices[i + 3]);

                    string localweight1 = "";
                    string localweight2 = "";
                    string localweight3 = "";
                    string localbone1 = "";
                    string localbone2 = "";
                    string localbone3 = "";

                    string bone1 = "";
                    string bone2 = "";
                    string bone3 = "";



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



                        //Console.WriteLine(boneWeight.Length);

                        if (bone[i / 4 - 2].Weight[0] != 0)
                        {
                            localbone1 = bone[i / 4 - 2].boneID[0].ToString();
                            localweight1 = hx.returnDecimal(bone[i / 4 - 2].Weight[0], 8);
                        }
                        if (bone[i / 4 - 2].Weight[1] != 0)
                        {
                            localbone2 = bone[i / 4 - 2].boneID[1].ToString();
                            localweight2 = hx.returnDecimal(bone[i / 4 - 2].Weight[1], 8);
                        }
                        if (bone[i / 4 - 2].Weight[2] != 0)
                        {
                            localbone3 = bone[i / 4 - 2].boneID[2].ToString();
                            localweight3 = hx.returnDecimal(bone[i / 4 - 2].Weight[2], 8);
                        }


                        bone1 = localbone1 + " " + localweight1 + " " + localbone2 + " " + localweight2 + " " + localbone3 + " " + localweight3;

                        if (bone[i / 4 - 1].Weight[0] != 0)
                        {
                            localweight1 = hx.returnDecimal(bone[i / 4 - 1].Weight[0], 8);
                            localbone1 = bone[i / 4 - 1].boneID[0].ToString();
                        }
                        if (bone[i / 4 - 1].Weight[1] != 0)
                        {
                            localweight2 = hx.returnDecimal(bone[i / 4 - 1].Weight[1], 8);
                            localbone2 = bone[i / 4 - 1].boneID[1].ToString();
                        }
                        if (bone[i / 4 - 1].Weight[2] != 0)
                        {
                            localweight3 = hx.returnDecimal(bone[i / 4 - 1].Weight[2], 8);
                            localbone3 = bone[i / 4 - 1].boneID[2].ToString();
                        }

                        bone2 = localbone1 + " " + localweight1 + " " + localbone2 + " " + localweight2 + " " + localbone3 + " " + localweight3;

                        if (bone[i / 4].Weight[0] != 0)
                        {
                            localbone1 = bone[i / 4 - 0].boneID[0].ToString();
                            localweight1 = hx.returnDecimal(bone[i / 4].Weight[0], 8);
                        }

                        if (bone[i / 4].Weight[1] != 0)
                        {
                            localbone2 = bone[i / 4 - 0].boneID[1].ToString();
                            localweight2 = hx.returnDecimal(bone[i / 4].Weight[1], 8);
                        }

                        if (bone[i / 4].Weight[2] != 0)
                        {
                            localbone3 = bone[i / 4 - 0].boneID[2].ToString();
                            localweight3 = hx.returnDecimal(bone[i / 4].Weight[2], 8);
                        }

                        bone3 = localbone1 + " " + localweight1 + " " + localbone2 + " " + localweight2 + " " + localbone3 + " " + localweight3;



                        writterStream.WriteLine("0  " + x1 + " " + y1 + " " + z1 + "  " + "0.000000 0.000000 0.000000  " + uvMapX1 + " " + uvMapY1 + " 1 " + bone1);
                        writterStream.WriteLine("0  " + x2 + " " + y2 + " " + z2 + "  " + "0.000000 0.000000 0.000000  " + uvMapX2 + " " + uvMapY2 + " 1 " + bone2);
                        writterStream.WriteLine("0  " + x3 + " " + y3 + " " + z3 + "  " + "0.000000 0.000000 0.000000  " + uvMapX3 + " " + uvMapY3 + " 1 " + bone3);
                    }
                }
                if (1 > 3)
                {
                    for (int i = 0; i < verts.Length; i += 4)
                    {
                        //THIS LINE REPEATS EXATCLY 88 TIMES


                        //Console.WriteLine(indices[i + 3]);

                        string localweight1 = "";
                        string localweight2 = "";
                        string localweight3 = "";
                        string localbone1 = "";
                        string localbone2 = "";
                        string localbone3 = "";

                        string bone1 = "";
                        string bone2 = "";
                        string bone3 = "";



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



                            //Console.WriteLine(boneWeight.Length);

                            if (bone[i / 4 - 2].Weight[0] != 0)
                            {
                                localbone1 = bone[i / 4 - 2].boneID[0].ToString();
                                localweight1 = hx.returnDecimal(bone[i / 4 - 2].Weight[0], 8);
                            }
                            if (bone[i / 4 - 2].Weight[1] != 0)
                            {
                                localbone2 = bone[i / 4 - 2].boneID[1].ToString();
                                localweight2 = hx.returnDecimal(bone[i / 4 - 2].Weight[1], 8);
                            }
                            if (bone[i / 4 - 2].Weight[2] != 0)
                            {
                                localbone3 = bone[i / 4 - 2].boneID[2].ToString();
                                localweight3 = hx.returnDecimal(bone[i / 4 - 2].Weight[2], 8);
                            }


                            bone1 = localbone1 + " " + localweight1 + " " + localbone2 + " " + localweight2 + " " + localbone3 + " " + localweight3;

                            if (bone[i / 4 - 1].Weight[0] != 0)
                            {
                                localweight1 = hx.returnDecimal(bone[i / 4 - 1].Weight[0], 8);
                                localbone1 = bone[i / 4 - 1].boneID[0].ToString();
                            }
                            if (bone[i / 4 - 1].Weight[1] != 0)
                            {
                                localweight2 = hx.returnDecimal(bone[i / 4 - 1].Weight[1], 8);
                                localbone2 = bone[i / 4 - 1].boneID[1].ToString();
                            }
                            if (bone[i / 4 - 1].Weight[2] != 0)
                            {
                                localweight3 = hx.returnDecimal(bone[i / 4 - 1].Weight[2], 8);
                                localbone3 = bone[i / 4 - 1].boneID[2].ToString();
                            }

                            bone2 = localbone1 + " " + localweight1 + " " + localbone2 + " " + localweight2 + " " + localbone3 + " " + localweight3;

                            if (bone[i / 4].Weight[0] != 0)
                            {
                                localbone1 = bone[i / 4 - 0].boneID[0].ToString();
                                localweight1 = hx.returnDecimal(bone[i / 4].Weight[0], 8);
                            }

                            if (bone[i / 4].Weight[1] != 0)
                            {
                                localbone2 = bone[i / 4 - 0].boneID[1].ToString();
                                localweight2 = hx.returnDecimal(bone[i / 4].Weight[1], 8);
                            }

                            if (bone[i / 4].Weight[2] != 0)
                            {
                                localbone3 = bone[i / 4 - 0].boneID[2].ToString();
                                localweight3 = hx.returnDecimal(bone[i / 4].Weight[2], 8);
                            }

                            bone3 = localbone1 + " " + localweight1 + " " + localbone2 + " " + localweight2 + " " + localbone3 + " " + localweight3;



                            writterStream.WriteLine("0  " + x1 + " " + y1 + " " + z1 + "  " + "0.000000 0.000000 0.000000  " + uvMapX1 + " " + uvMapY1 + " 1 " + bone1);
                            writterStream.WriteLine("0  " + x2 + " " + y2 + " " + z2 + "  " + "0.000000 0.000000 0.000000  " + uvMapX2 + " " + uvMapY2 + " 1 " + bone2);
                            writterStream.WriteLine("0  " + x3 + " " + y3 + " " + z3 + "  " + "0.000000 0.000000 0.000000  " + uvMapX3 + " " + uvMapY3 + " 1 " + bone3);








                            //writterStream.WriteLine("0  " + x1 + " " + y1 + " " + z1 + "  " + "0.000000 0.000000 0.000000  0.000000 0.000000 0 ");
                            //writterStream.WriteLine("0  " + x2 + " " + y2 + " " + z2 + "  " + "0.000000 0.000000 0.000000  0.000000 0.000000 0 ");
                            //writterStream.WriteLine("0  " + x3 + " " + y3 + " " + z3 + "  " + "0.000000 0.000000 0.000000  0.000000 0.000000 0 ");
                        }
                    }
                }
                writterStream.WriteLine("end");
            }
        }
        //Console.WriteLine("Valve SMD created successfully on path " + dataPath + ".");
    }

    static void createValveSMD(dynamic[] verts, dynamic[] indices, int materialIndex, string meshName, dynamic[] UVMapping, dynamic[] skeletonPos, dynamic[] skeletonID, dynamic[] boneWeight)
    {
        #region createSMD
        string partialDirectory = Path.GetDirectoryName(ExtractModel.savePath) + "/" + Path.GetFileNameWithoutExtension(ExtractModel.name);
        Directory.CreateDirectory(partialDirectory);
        string name = "/" + removeIllegalChars(meshName);
        string dataPath = partialDirectory + name + ".smd";
        Console.WriteLine(removeIllegalChars("-> " + meshName));
        #endregion


        using (var saveStream = new FileStream(dataPath, FileMode.Create))

        using (var writterStream = new StreamWriter(saveStream))
        {
            List<BONE_MESHPAINT> bone = new List<BONE_MESHPAINT>();

            for (int i = 0; i < boneWeight.Length; i += 8)
            {
                BONE_MESHPAINT placeholder = new BONE_MESHPAINT();


                placeholder.boneID = new int[] { 
                    ((boneWeight[i + 1] + 1) / 4),
                    ((boneWeight[i + 2] + 1) / 4),
                    ((boneWeight[i + 3] + 1) / 4),
                    0 };

                placeholder.Weight = new float[] { 
                    (boneWeight[i + 4] / 100f),
                    (boneWeight[i +5] / 100f),
                    (boneWeight[i +6] / 100f),
                    0f };
                bone.Add(placeholder);
            }

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
                    writterStream.WriteLine(numbers + " \"" + skeletonID[i + 6].ToString("X") + "/" + skeletonID[i + 7].ToString("X") + "_" + i + "_" + "\" " + curID);
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

                float resultX = x1;
                float resultY = y1;
                float resultZ = z1;

                vector3[i].Matrix3 = new float[] { resultX, resultY, resultZ };
            }



            writterStream.WriteLine("end");
            writterStream.WriteLine("skeleton");


            writterStream.WriteLine("time 0");
            //writterStream.WriteLine("0  0.000000 0.000000 0.000000  0.000000 -0.000000 0.000000");


            numbers = 0;



            for (int i = 0; i < vector3.Count; i += 1)
            {
                int curID = vector3[i].additiveOn;
                if (curID < 0) curID = 0;
                string x = hx.returnDecimal(vector3[i].Matrix3[0], 8);
                string y = hx.returnDecimal(vector3[i].Matrix3[1], 8);
                string z = hx.returnDecimal(vector3[i].Matrix3[2], 8);
                string xEnd = "0.000000";
                string yEnd = "0.000000";
                string zEnd = "0.000000";
                writterStream.WriteLine(numbers + "  " + x + " " + y + " " + z + "  " + xEnd + " " + yEnd + " " + zEnd);
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

                string localweight1 = "";
                string localweight2 = "";
                string localweight3 = "";
                string localbone1 = "";
                string localbone2 = "";
                string localbone3 = "";

                string bone1 = "";
                string bone2 = "";
                string bone3 = "";


                int weightLink1 = 0;
                int weightLink2 = 0;
                int weightLink3 = 0;


                int additive = -2;
                if (i > 4)
                {
                    if ((i / 4) + additive < bone.Count)
                    {
                        if (bone[(i / 4) + additive].Weight[0] != 0)
                        {
                            weightLink1 += 1;
                            localbone1 = " " + bone[(i / 4) + additive].boneID[0].ToString();
                            localweight1 = " " + hx.returnDecimal(bone[(i / 4) + additive].Weight[0], 8);
                        }
                        else
                        {
                            localbone1 = "";
                            localweight1 = "";
                        }
                        if (bone[(i / 4) + additive].Weight[1] != 0)
                        {
                            weightLink1 += 1;
                            localbone2 = " " + bone[(i / 4) + additive].boneID[1].ToString();
                            localweight2 = " " + hx.returnDecimal(bone[(i / 4) + additive].Weight[1], 8);
                        }
                        else
                        {
                            localbone2 = "";
                            localweight2 = "";
                        }
                        if (bone[(i / 4) + additive].Weight[2] != 0)
                        {
                            weightLink1 += 1;
                            localbone3 = " " + bone[(i / 4) + additive].boneID[2].ToString();
                            localweight3 = " " + hx.returnDecimal(bone[(i / 4) + additive].Weight[2], 8);
                        }
                        else
                        {
                            localbone3 = "";
                            localweight3 = "";
                        }
                    }


                    bone1 = localbone1 + localweight1 + localbone2 + localweight2 + localbone3 + localweight3;
                    additive = -1;
                    if ((i / 4) + additive < bone.Count)
                    {
                        if (bone[(i / 4) + additive].Weight[0] != 0)
                        {
                            weightLink2 += 1;
                            localbone1 = " " + bone[(i / 4) + additive].boneID[0].ToString();
                            localweight1 = " " + hx.returnDecimal(bone[(i / 4) + additive].Weight[0], 8);
                        }
                        else
                        {
                            localbone1 = "";
                            localweight1 = "";
                        }
                        if (bone[(i / 4) + additive].Weight[1] != 0)
                        {
                            weightLink2 += 1;
                            localbone2 = " " + bone[(i / 4) + additive].boneID[1].ToString();
                            localweight2 = " " + hx.returnDecimal(bone[(i / 4) + additive].Weight[1], 8);
                        }
                        else
                        {
                            localbone2 = "";
                            localweight2 = "";
                        }
                        if (bone[(i / 4) + additive].Weight[2] != 0)
                        {
                            weightLink2 += 1;
                            localbone3 = " " + bone[(i / 4) + additive].boneID[2].ToString();
                            localweight3 = " " + hx.returnDecimal(bone[(i / 4) + additive].Weight[2], 8);
                        }
                        else
                        {
                            localbone3 = "";
                            localweight3 = "";
                        }
                    }
                    bone2 = localbone1 + localweight1 + localbone2 + localweight2 + localbone3 + localweight3;

                    additive = 0;

                    if ((i / 4) + additive < bone.Count)
                    {
                        if (bone[(i / 4) + additive].Weight[0] != 0)
                        {
                            weightLink3 += 1;
                            localbone1 = " " + bone[(i / 4) + additive].boneID[0].ToString();
                            localweight1 = " " + hx.returnDecimal(bone[(i / 4) + additive].Weight[0], 8);
                        }
                        else
                        {
                            localbone1 = "";
                            localweight1 = "";
                        }
                        if (bone[(i / 4) + additive].Weight[1] != 0)
                        {
                            weightLink3 += 1;
                            localbone2 = " " + bone[(i / 4) + additive].boneID[1].ToString();
                            localweight2 = " " + hx.returnDecimal(bone[(i / 4) + additive].Weight[1], 8);
                        }
                        else
                        {
                            localbone2 = "";
                            localweight2 = "";
                        }
                        if (bone[(i / 4) + additive].Weight[2] != 0)
                        {
                            weightLink3 += 1;
                            localbone3 = " " + bone[(i / 4) + additive].boneID[2].ToString();
                            localweight3 = " " + hx.returnDecimal(bone[(i / 4) + additive].Weight[2], 8);
                        } else
                        {
                            localbone3 = "";
                            localweight3 = "";
                        }
                    }
                    bone3 = localbone1 + localweight1 + localbone2 + localweight2 + localbone3 + localweight3;

                }

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



                    //Console.WriteLine(boneWeight.Length);

                    writterStream.WriteLine("0  " + x1 + " " + y1 + " " + z1 + "  " + "0.000000 0.000000 0.000000  " + uvMapX1 + " " + uvMapY1 + " " + weightLink1 + bone1);
                    writterStream.WriteLine("0  " + x2 + " " + y2 + " " + z2 + "  " + "0.000000 0.000000 0.000000  " + uvMapX2 + " " + uvMapY2 + " " + weightLink2 + bone2);
                    writterStream.WriteLine("0  " + x3 + " " + y3 + " " + z3 + "  " + "0.000000 0.000000 0.000000  " + uvMapX3 + " " + uvMapY3 + " " + weightLink3 + bone3);


                    //writterStream.WriteLine("0  " + x1 + " " + y1 + " " + z1 + "  " + "0.000000 0.000000 0.000000  " + uvMapX1 + " " + uvMapY1 + " 1" + bone1);
                    //writterStream.WriteLine("0  " + x2 + " " + y2 + " " + z2 + "  " + "0.000000 0.000000 0.000000  " + uvMapX2 + " " + uvMapY2 + " 1" + bone2);
                    //writterStream.WriteLine("0  " + x3 + " " + y3 + " " + z3 + "  " + "0.000000 0.000000 0.000000  " + uvMapX3 + " " + uvMapY3 + " 1" + bone3);

                }
            }
            if (1 > 3)
            {
                for (int i = 0; i < verts.Length; i += 4)
                {
                    //THIS LINE REPEATS EXATCLY 88 TIMES


                    //Console.WriteLine(indices[i + 3]);

                    string localweight1 = "";
                    string localweight2 = "";
                    string localweight3 = "";
                    string localbone1 = "";
                    string localbone2 = "";
                    string localbone3 = "";

                    string bone1 = "";
                    string bone2 = "";
                    string bone3 = "";



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



                        //Console.WriteLine(boneWeight.Length);

                        if (bone[i / 4 - 2].Weight[0] != 0)
                        {
                            localbone1 = bone[i / 4 - 2].boneID[0].ToString();
                            localweight1 = hx.returnDecimal(bone[i / 4 - 2].Weight[0], 8);
                        }
                        if (bone[i / 4 - 2].Weight[1] != 0)
                        {
                            localbone2 = bone[i / 4 - 2].boneID[1].ToString();
                            localweight2 = hx.returnDecimal(bone[i / 4 - 2].Weight[1], 8);
                        }
                        if (bone[i / 4 - 2].Weight[2] != 0)
                        {
                            localbone3 = bone[i / 4 - 2].boneID[2].ToString();
                            localweight3 = hx.returnDecimal(bone[i / 4 - 2].Weight[2], 8);
                        }


                        bone1 = localbone1 + " " + localweight1 + " " + localbone2 + " " + localweight2 + " " + localbone3 + " " + localweight3;

                        if (bone[i / 4 - 1].Weight[0] != 0)
                        {
                            localweight1 = hx.returnDecimal(bone[i / 4 - 1].Weight[0], 8);
                            localbone1 = bone[i / 4 - 1].boneID[0].ToString();
                        }
                        if (bone[i / 4 - 1].Weight[1] != 0)
                        {
                            localweight2 = hx.returnDecimal(bone[i / 4 - 1].Weight[1], 8);
                            localbone2 = bone[i / 4 - 1].boneID[1].ToString();
                        }
                        if (bone[i / 4 - 1].Weight[2] != 0)
                        {
                            localweight3 = hx.returnDecimal(bone[i / 4 - 1].Weight[2], 8);
                            localbone3 = bone[i / 4 - 1].boneID[2].ToString();
                        }

                        bone2 = localbone1 + " " + localweight1 + " " + localbone2 + " " + localweight2 + " " + localbone3 + " " + localweight3;

                        if (bone[i / 4].Weight[0] != 0)
                        {
                            localbone1 = bone[i / 4 - 0].boneID[0].ToString();
                            localweight1 = hx.returnDecimal(bone[i / 4].Weight[0], 8);
                        }

                        if (bone[i / 4].Weight[1] != 0)
                        {
                            localbone2 = bone[i / 4 - 0].boneID[1].ToString();
                            localweight2 = hx.returnDecimal(bone[i / 4].Weight[1], 8);
                        }

                        if (bone[i / 4].Weight[2] != 0)
                        {
                            localbone3 = bone[i / 4 - 0].boneID[2].ToString();
                            localweight3 = hx.returnDecimal(bone[i / 4].Weight[2], 8);
                        }

                        bone3 = localbone1 + " " + localweight1 + " " + localbone2 + " " + localweight2 + " " + localbone3 + " " + localweight3;



                        writterStream.WriteLine("0  " + x1 + " " + y1 + " " + z1 + "  " + "0.000000 0.000000 0.000000  " + uvMapX1 + " " + uvMapY1 + " 1 " + bone1);
                        writterStream.WriteLine("0  " + x2 + " " + y2 + " " + z2 + "  " + "0.000000 0.000000 0.000000  " + uvMapX2 + " " + uvMapY2 + " 1 " + bone2);
                        writterStream.WriteLine("0  " + x3 + " " + y3 + " " + z3 + "  " + "0.000000 0.000000 0.000000  " + uvMapX3 + " " + uvMapY3 + " 1 " + bone3);








                        //writterStream.WriteLine("0  " + x1 + " " + y1 + " " + z1 + "  " + "0.000000 0.000000 0.000000  0.000000 0.000000 0 ");
                        //writterStream.WriteLine("0  " + x2 + " " + y2 + " " + z2 + "  " + "0.000000 0.000000 0.000000  0.000000 0.000000 0 ");
                        //writterStream.WriteLine("0  " + x3 + " " + y3 + " " + z3 + "  " + "0.000000 0.000000 0.000000  0.000000 0.000000 0 ");
                    }
                }
            }
            writterStream.WriteLine("end");
        }
        Console.WriteLine("Valve SMD created successfully on path " + dataPath + ".");
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
