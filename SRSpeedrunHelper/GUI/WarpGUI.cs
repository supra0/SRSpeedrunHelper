using MonomiPark.SlimeRancher.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SRSpeedrunHelper.SRSHGUI
{
    internal static class WarpGUI
    {
        // TODO: why 30?
        private static readonly int WARP_NAME_MAX_LENGTH = 30;

        private static Vector2 warpsScrollPosition = Vector2.zero;
        private static int warpsToolbarTab = 0;

        private static readonly string[] warpsToolbarTabTitles =
        {
            "Presets",
            "Custom",
            "Create"
        };
        private static string newUserWarpText = "New warp";

        private static bool saveAmmoToggle = false;
        private static bool saveHealthToggle = true;
        private static bool saveEnergyToggle = true;
        private static bool saveNewbucksToggle = false;



        internal static void DoGUI()
        {
            // Warp settings
            warpsToolbarTab = GUILayout.Toolbar(warpsToolbarTab, warpsToolbarTabTitles);
            switch (warpsToolbarTab)
            {
                // Predefined warps
                case (0):
                    DoPresetGUI();
                    break;

                // User warps
                case (1):
                    DoCustomGUI();
                    break;

                case (2):
                    DoCreateGUI();
                    break;

                default:
                    GUILayout.Label("You should never see this. Oops!");
                    break;
            }
        }

        #region Tabs
        private static void DoPresetGUI()
        {
            // Lay out the labels and buttons for the predefined warps
            foreach (KeyValuePair<WarpData[], string> area in WarpData.ALL_AREA_WARPS)
            {
                GUILayout.Label(area.Value, SRSpeedrunHelper.LABEL_STYLE_BOLD);

                GUILayout.BeginHorizontal();
                foreach (WarpData warp in area.Key)
                {
                    if (GUILayout.Button(warp.Name))
                    {
                        UserWarps.WarpPlayer(warp);

                        if (SRSHConfig.saveStateCloseMenu)
                        {
                            SRSpeedrunHelper.showMenu = false;
                            SRSingleton<PauseMenu>.Instance.UnPauseGame();
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        private static void DoCustomGUI()
        {
            warpsScrollPosition = GUILayout.BeginScrollView(warpsScrollPosition);
            var userWarps = UserWarps.Warps;

            if (UserWarps.Warps == null)
            {
                UserWarps.LoadWarps();
            }

            // List user warps
            foreach (WarpData warpData in UserWarps.Warps)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(warpData.Name, SRSpeedrunHelper.LABEL_STYLE_BOLD);
                if (GUILayout.Button("Load"))
                {
                    UserWarps.WarpPlayer(warpData);

                    if (SRSHConfig.saveStateCloseMenu)
                    {
                        SRSpeedrunHelper.showMenu = false;
                        SRSingleton<PauseMenu>.Instance.UnPauseGame();
                    }
                }
                else if (GUILayout.Button("Remove"))
                {
                    UserWarps.RemoveUserWarp(warpData);
                }
                else if (GUILayout.Button("^"))
                {
                    UserWarps.MoveWarpUp(userWarps.IndexOf(warpData));
                }
                else if (GUILayout.Button("v"))
                {
                    UserWarps.MoveWarpDown(userWarps.IndexOf(warpData));
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();

            GUILayout.Label("Warps are saved to disk automatically.", SRSpeedrunHelper.LABEL_STYLE_DEFAULT);
            if (GUILayout.Button("Reload warps from disk (do this if you've manually changed the SRSH_userwarps.xml file)"))
            {
                UserWarps.LoadWarps();
            }
        }

        private static void DoCreateGUI()
        {
            // Create custom warps tab
            GUILayout.Label("Warp name", SRSpeedrunHelper.LABEL_STYLE_BOLD);
            newUserWarpText = GUILayout.TextField(newUserWarpText, WARP_NAME_MAX_LENGTH);

            GUILayout.Label("\nOptions", SRSpeedrunHelper.LABEL_STYLE_BOLD);

            saveAmmoToggle = GUILayout.Toggle(saveAmmoToggle, "Save Ammo");
            saveHealthToggle = GUILayout.Toggle(saveHealthToggle, "Save Health");
            saveEnergyToggle = GUILayout.Toggle(saveEnergyToggle, "Save Energy");
            saveNewbucksToggle = GUILayout.Toggle(saveNewbucksToggle, "Save Newbucks");

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add"))
            {
                InventoryData newInventoryData = null;

                if (saveAmmoToggle)
                {
                    PlayerState playerStateTmp = SRSingleton<SceneContext>.Instance.PlayerState;
                    newInventoryData = new InventoryData(playerStateTmp.GetAmmoMode());

                    for (int i = 0; i < playerStateTmp.Ammo.GetUsableSlotCount(); i++)
                    {
                        SRSpeedrunHelper.Log("Slot " + i.ToString());
                        newInventoryData.AddSlot(playerStateTmp.Ammo.GetSlotName(i), playerStateTmp.Ammo.GetSlotCount(i));
                        SRSpeedrunHelper.Log("Id: " + playerStateTmp.Ammo.GetSlotName(i).ToString());
                        SRSpeedrunHelper.Log("Count: " + playerStateTmp.Ammo.GetSlotCount(i));
                    }
                }

                PlayerModel playerModelTmp = SRSpeedrunHelper.GetPlayerModel();

                WarpData warpDataTmp = new WarpData(playerModelTmp.GetPos(), playerModelTmp.GetRot().eulerAngles, newUserWarpText, playerModelTmp.currRegionSetId, newInventoryData);

                if (saveHealthToggle)
                {
                    warpDataTmp.PlayerHealth = playerModelTmp.currHealth;
                }
                if (saveEnergyToggle)
                {
                    warpDataTmp.PlayerEnergy = playerModelTmp.currEnergy;
                }
                if (saveNewbucksToggle)
                {
                    warpDataTmp.PlayerNewbucks = playerModelTmp.currency;
                }

                UserWarps.AddUserWarp(warpDataTmp);

                newUserWarpText = "New warp";

                warpsToolbarTab = 1; // switch to Custom tab to indicate the save state was added and to show it in the list
            }
        }
        #endregion

        #region Logic

        #endregion
    }
}
