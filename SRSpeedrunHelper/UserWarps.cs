using MonomiPark.SlimeRancher.DataModel;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UModFramework.API;

namespace SRSpeedrunHelper
{
    static class UserWarps
    {
        private static readonly string defaultFilename = "SRSH_userwarps.xml";
        private static readonly string defaultFilepath = Path.Combine(UMFData.ConfigsPath, defaultFilename);

        internal static List<WarpData> Warps { get; private set; } = new List<WarpData>();

        internal static void AddUserWarp(WarpData warpData)
        {
            Warps.Add(warpData);
            WriteToFile();
        }

        internal static bool RemoveUserWarp(WarpData warpData)
        {
            bool result = Warps.Remove(warpData);
            WriteToFile();
            return result;
            
        }

        internal static WarpData GetWarpDataByIndex(int index)
        {
            if(index >= Warps.Count)
            {
                return null;
            }
            return Warps.ElementAt(index);
        }

        internal static bool MoveWarpUp(int index)
        {
            if (index < 1 || Warps.Count < 2)
            {
                SRSpeedrunHelper.Log("Attempted to move user warp up in list when it was not possible");
                return false;
            }
            else
            {
                WarpData tmp = Warps[index - 1];
                Warps[index - 1] = Warps[index];
                Warps[index] = tmp;
                WriteToFile();
                return true;
            }
        }

        internal static bool MoveWarpDown(int index)
        {
            if (index >= Warps.Count)
            {
                SRSpeedrunHelper.Log("Attempted to move user warp down in list when it was not possible");
                return false;
            }
            else
            {
                WarpData tmp = Warps[index + 1];
                Warps[index + 1] = Warps[index];
                Warps[index] = tmp;
                WriteToFile();
                return true;
            }
        }

        internal static void LoadWarps()
        {
            LoadFromFile();
        }

        private static void LoadFromFile()
        {
            if (!File.Exists(defaultFilepath))
            {
                File.Create(defaultFilepath);
            }
            else
            {
                using(Stream reader = new FileStream(defaultFilepath, FileMode.Open))
                {
                    XmlSerializer x = new XmlSerializer(Warps.GetType());
                    Warps = (List<WarpData>)x.Deserialize(reader);
                }
            }
        }

        private static void WriteToFile()
        {
            using (FileStream file = File.Open(defaultFilepath, FileMode.Create))
            {
                XmlSerializer x = new XmlSerializer(Warps.GetType());
                x.Serialize(file, Warps);
            }
        }

        internal static void WarpPlayer(WarpData warpData)
        {
            if (warpData == null)
            {
                return;
            }
            if (Levels.isMainMenu() || Levels.isSpecial())
            {
                return;
            }

            PlayerModel playerModelTmp = SRSpeedrunHelper.GetPlayerModel();
            playerModelTmp.SetTransform(warpData.Position, warpData.RotEuler);
            playerModelTmp.SetCurrRegionSet(warpData.RegionSetId);

            if (warpData.HasInventoryData())
            {
                SetPlayerSlots(warpData.InventoryData);
            }

            if (warpData.PlayerHealth != null)
            {
                playerModelTmp.SetHealth((float)warpData.PlayerHealth);
                playerModelTmp.healthBurstAfter = SceneContext.Instance.TimeDirector.WorldTime() + 300.0;
            }
            if (warpData.PlayerEnergy != null)
            {
                SRSpeedrunHelper.SetPlayerEnergy((float)warpData.PlayerEnergy);
            }
            if (warpData.PlayerNewbucks != null)
            {
                playerModelTmp.SetCurrency((int)warpData.PlayerNewbucks);
            }

            SRSpeedrunHelper.ForceUnpause();
        }

        internal static void SetPlayerSlots(InventoryData inventoryData)
        {
            if (inventoryData == null)
            {
                return;
            }

            PlayerState playerStateTmp = SceneContext.Instance.PlayerState;
            playerStateTmp.SetAmmoMode(inventoryData.AmmoMode);

            playerStateTmp.Ammo.Clear();
            int slotNum = 0;

            foreach (InventoryData.IdentifiableCountPair slot in inventoryData.AmmoList)
            {
                playerStateTmp.Ammo.MaybeAddToSpecificSlot(slot.Id, null, slotNum, slot.Count, true);

                // Special case for Slimes
                // Since we don't pass an Identifiable to Ammo.MaybeAddToSpecificSlot, SlimeEmotion data never gets added (stays null)
                // This causes a bug when trying to shoot slimes added to ammo slots
                // Calling Ammo.Replace adds default SlimeEmotion data
                // Could probably do this in a less roundabout way using reflection (ammo slots are private)
                if (Identifiable.IsSlime(slot.Id))
                {
                    playerStateTmp.Ammo.Replace(slot.Id, slot.Id);
                }
                slotNum++;
            }
        }
    }
}
