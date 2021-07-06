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
        }

        public static bool RemoveUserWarp(WarpData warpData)
        {
            return userWarps.Remove(warpData);
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
                return true;
            }
        }

        public static void LoadFromFile()
        {
            XmlSerializer x = new XmlSerializer(userWarps.GetType());
            Stream reader = new FileStream(defaultFilepath, FileMode.Open);

            userWarps = (List<WarpData>)x.Deserialize(reader);
        }

        public static void WriteToFile()
        {
            XmlSerializer x = new XmlSerializer(userWarps.GetType());
            FileStream file = File.Open(defaultFilepath, FileMode.Create);

            x.Serialize(file, userWarps);
        }
    }
}
