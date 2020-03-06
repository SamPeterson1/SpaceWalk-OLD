using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChunkIO
{
    public static string savePath = "/Save/chunkData.dat";
    private static float chunkSeparator = float.MaxValue;

    public static void CreateFile()
    {
        if(!File.Exists(savePath))
        {
            File.Create(savePath);
        }
    }

    public static Dictionary<Vector3, TerrainDeformation> LoadData()
    {
        Dictionary<Vector3, TerrainDeformation> deformations = new Dictionary<Vector3, TerrainDeformation>();
        using (BinaryReader reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
        {
            int numChunks = reader.ReadInt32();
            for (int i = 0; i < numChunks; i++)
            {
                float f;
                int index = 0;
                float x = 0;
                float y = 0;
                float z = 0;
                float density = 0;
                int densityIndex;

                Dictionary<int, float> points = new Dictionary<int, float>();

                while ((f = reader.ReadSingle()) != chunkSeparator)
                {
                    if (index == 0) x = f;
                    else if (index == 1) y = f;
                    else if (index == 2) z = f;
                    else if (index % 2 == 1)
                    {
                        density = f;
                    }
                    else if (index % 2 == 0)
                    {
                        densityIndex = (int)f;
                        if (points.ContainsKey(densityIndex))
                        {
                            points.Remove(densityIndex);
                        }
                        points.Add(densityIndex, density);
                    }
                    index++;
                }

                Vector3 offset = new Vector3(x, y, z);
                deformations.Add(offset, new TerrainDeformation(points));
            }
        }
        return deformations;
    }

    public static void WriteData(Dictionary<Vector3, TerrainDeformation> deformations)
    {

        using (BinaryWriter writer = new BinaryWriter(File.Open(savePath, FileMode.Append)))
        {
            writer.Write(deformations.Count);
            foreach (Vector3 key in deformations.Keys)
            {
                deformations.TryGetValue(key, out TerrainDeformation deformation);
                Dictionary<int, float> points = deformation.points;


                writer.Write(key.x);
                writer.Write(key.y);
                writer.Write(key.z);
                foreach (int i in points.Keys)
                {
                    points.TryGetValue(i, out float f);
                    writer.Write(f);
                    //make it a float to make reading the file easier
                    writer.Write((float)i);
                }
                writer.Write(chunkSeparator);
            }
        }
    }

}
