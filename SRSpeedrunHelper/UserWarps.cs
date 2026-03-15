using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UModFramework.API;

namespace SRSpeedrunHelper
{
    static class UserWarps
    {
        public static readonly string defaultFilename = "SRSH_userwarps.xml";
        public static readonly string defaultFilepath = Path.Combine(UMFData.ConfigsPath, defaultFilename);

        private static List<WarpData> userWarps = new List<WarpData>();

        public static void AddUserWarp(WarpData warpData)
        {
            userWarps.Add(warpData);
            WriteToFile();
        }

        public static bool RemoveUserWarp(WarpData warpData)
        {
            bool result = userWarps.Remove(warpData);
            WriteToFile();
            return result;
            
        }

        public static WarpData GetWarpDataByIndex(int index)
        {
            if(index >= userWarps.Count)
            {
                return null;
            }
            return userWarps.ElementAt(index);
        }

        public static List<WarpData> GetUserWarps()
        {
            return new List<WarpData>(userWarps);
        }

        public static bool MoveWarpUp(int index)
        {
            if (index < 1 || userWarps.Count < 2)
            {
                SRSpeedrunHelper.Log("Attempted to move user warp up in list when it was not possible");
                return false;
            }
            else
            {
                WarpData tmp = userWarps[index - 1];
                userWarps[index - 1] = userWarps[index];
                userWarps[index] = tmp;
                WriteToFile();
                return true;
            }
        }

        public static bool MoveWarpDown(int index)
        {
            if (index >= userWarps.Count)
            {
                SRSpeedrunHelper.Log("Attempted to move user warp down in list when it was not possible");
                return false;
            }
            else
            {
                WarpData tmp = userWarps[index + 1];
                userWarps[index + 1] = userWarps[index];
                userWarps[index] = tmp;
                WriteToFile();
                return true;
            }
        }

        public static void LoadFromFile()
        {
            if (!File.Exists(defaultFilepath))
            {
                File.Create(defaultFilepath);
            }
            else
            {
                using(Stream reader = new FileStream(defaultFilepath, FileMode.Open))
                {
                    XmlSerializer x = new XmlSerializer(userWarps.GetType());
                    userWarps = (List<WarpData>)x.Deserialize(reader);
                }
            }
        }

        public static void WriteToFile()
        {
            using (FileStream file = File.Open(defaultFilepath, FileMode.Create))
            {
                XmlSerializer x = new XmlSerializer(userWarps.GetType());
                x.Serialize(file, userWarps);
            }
        }
    }
}
