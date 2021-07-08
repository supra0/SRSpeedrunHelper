using System;
using UModFramework.API;
using UnityEngine;

namespace SRSpeedrunHelper
{
    public class SRSHConfig
    {
        private static readonly string configVersion = "1.0";

        // General keybinds
        internal static KeyCode bind_showMenu;

        // User warp keybinds
        internal static KeyCode bind_userWarp1;
        internal static KeyCode bind_userWarp2;
        internal static KeyCode bind_userWarp3;
        internal static KeyCode bind_userWarp4;
        internal static KeyCode bind_userWarp5;
        internal static KeyCode bind_userWarp6;
        internal static KeyCode bind_userWarp7;
        internal static KeyCode bind_userWarp8;
        internal static KeyCode bind_userWarp9;
        internal static KeyCode bind_userWarp10;
        internal static KeyCode bind_userWarp11;
        internal static KeyCode bind_userWarp12;

        // Timer keybinds
        internal static KeyCode bind_startTimer;
        internal static KeyCode bind_stopTimer;
        internal static KeyCode bind_resetTimer;

        // Ohter keybinds
        internal static KeyCode bind_spawnCrate;

        // Other config variables
        internal static bool showModWarning = true;
        internal static bool saveStateCloseMenu = false;

        // Spawner keybind
        //internal static KeyCode bind_activateSpawner;

        internal static void Load()
        {
            SRSpeedrunHelper.Log("Loading settings.");
            try
            {
                using (UMFConfig cfg = new UMFConfig())
                {
                    string cfgVer = cfg.Read("ConfigVersion", new UMFConfigString());
                    if (cfgVer != string.Empty && cfgVer != configVersion)
                    {
                        cfg.DeleteConfig(false);
                        SRSpeedrunHelper.Log("The config file was outdated and has been deleted. A new config will be generated.");
                    }

                    //cfg.Write("SupportsHotLoading", new UMFConfigBool(false)); //Uncomment if your mod can't be loaded once the game has started.
                    cfg.Write("ModDependencies", new UMFConfigStringArray(new string[] { "" })); //A comma separated list of mod/library names that this mod requires to function. Format: SomeMod:1.50,SomeLibrary:0.60
                    cfg.Read("LoadPriority", new UMFConfigString("Normal"));
                    cfg.Write("MinVersion", new UMFConfigString("0.53.0"));
                    cfg.Write("MaxVersion", new UMFConfigString("0.54.99999.99999")); //This will prevent the mod from being loaded after the next major UMF release
                    cfg.Write("UpdateURL", new UMFConfigString(""));
                    cfg.Write("ConfigVersion", new UMFConfigString(configVersion));

                    SRSpeedrunHelper.Log("Finished UMF Settings.");

                    // Show menu bind(s)
                    bind_showMenu = cfg.Read("BindShowMenu", new UMFConfigKeyCode(KeyCode.Tilde, true), "Shows the SRSpeedrunHelper menu.");

                    // User warp binds
                    bind_userWarp1 = cfg.Read("BindSavestate1", new UMFConfigKeyCode(KeyCode.F1, true), "Runs the 1st custom save state.");
                    bind_userWarp2 = cfg.Read("BindSavestate2", new UMFConfigKeyCode(KeyCode.F2, true), "Runs the 2nd custom save state.");
                    bind_userWarp3 = cfg.Read("BindSavestate3", new UMFConfigKeyCode(KeyCode.F3, true), "Runs the 3rd custom save state.");
                    bind_userWarp4 = cfg.Read("BindSavestate4", new UMFConfigKeyCode(KeyCode.F4, true), "Runs the 4th custom save state.");
                    bind_userWarp5 = cfg.Read("BindSavestate5", new UMFConfigKeyCode(KeyCode.F5, true), "Runs the 5th custom save state.");
                    bind_userWarp6 = cfg.Read("BindSavestate6", new UMFConfigKeyCode(KeyCode.F6, true), "Runs the 6th custom save state.");
                    bind_userWarp7 = cfg.Read("BindSavestate7", new UMFConfigKeyCode(KeyCode.F7, true), "Runs the 7th custom save state.");
                    bind_userWarp8 = cfg.Read("BindSavestate8", new UMFConfigKeyCode(KeyCode.F8, true), "Runs the 8th custom save state.");
                    bind_userWarp9 = cfg.Read("BindSavestate9", new UMFConfigKeyCode(KeyCode.F9, true), "Runs the 9th custom save state.");
                    bind_userWarp10 = cfg.Read("BindSavestate10", new UMFConfigKeyCode(KeyCode.F10, true), "Runs the 10th custom save state.");
                    bind_userWarp11 = cfg.Read("BindSavestate11", new UMFConfigKeyCode(KeyCode.F11, true), "Runs the 11th custom save state.");
                    bind_userWarp12 = cfg.Read("BindSavestate12", new UMFConfigKeyCode(KeyCode.F12, true), "Runs the 12th custom save state.");

                    // Timer binds
                    bind_startTimer = cfg.Read("BindStartTimer", new UMFConfigKeyCode(KeyCode.Alpha0, true), "Start the ingame timer.");
                    bind_stopTimer = cfg.Read("BindStopTimer", new UMFConfigKeyCode(KeyCode.Minus, true), "Stop the ingame timer.");
                    bind_resetTimer = cfg.Read("BindResetTimer", new UMFConfigKeyCode(KeyCode.Equals, true), "Reset the ingame timer.");

                    // Other keybinds
                    bind_spawnCrate = cfg.Read("BindSpawnCrate", new UMFConfigKeyCode(KeyCode.None, true), "Spawns a crate in front of the player.");

                    // Activate spawner bind(s)
                    //bind_activateSpawner = cfg.Read("BindActivateSpawner", new UMFConfigKeyCode(KeyCode.Equals), "Force trigger the spawner you are looking at.";

                    // Other config variables
                    showModWarning = cfg.Read("ShowModWarning", new UMFConfigBool(true), "Show the warning on the main menu.");
                    saveStateCloseMenu = cfg.Read("SavestateCloseMenu", new UMFConfigBool(false), "Automatically close the menu when loading a save state from the menu");

                    SRSpeedrunHelper.Log("Finished loading settings.");
                }
            }
            catch (Exception e)
            {
                SRSpeedrunHelper.Log("Error loading mod settings: " + e.Message + "(" + e.InnerException?.Message + ")");
            }
        }
    }
}