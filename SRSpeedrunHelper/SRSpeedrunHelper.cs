using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UModFramework.API;
using MonomiPark.SlimeRancher.DataModel;

namespace SRSpeedrunHelper
{
    [UMFHarmony(0)] //Set this to the number of harmony patches in your mod.
    [UMFScript]
    class SRSpeedrunHelper : MonoBehaviour
    {
        #region General GUI Variables
        private static readonly int windowSizeX = 700;
        private static readonly int windowSizeY = 500;
        private static readonly string windowTitle = "Speedrun Helper Menu";
        private static readonly int windowId = 1258;

        private static Rect windowRect = new Rect(Screen.width - windowSizeX, 0, windowSizeX, windowSizeY); // Window dimensions and default position. Appears in top-right corner
        private static bool showMenu = false;

        private static int currentToolbarTab = 0;
        private static readonly string[] toolbarTabTitles =
        {
            "Warps",
            "Timer",
            "Gordos",
            "Spawner Info",
            "Misc."
        };

        private static Rect modWarningRect = new Rect(10, 0, 0, 0);
        #endregion

        #region GUI Skins
        internal static readonly GUIStyle LABEL_STYLE_DEFAULT = new GUIStyle();
        internal static readonly GUIStyle LABEL_STYLE_BOLD = new GUIStyle();
        internal static readonly GUIStyle TEXT_STYLE_HEADER = new GUIStyle();
        internal static readonly GUIStyle TEXT_STYLE_MOD_WARNING = new GUIStyle();
        #endregion

        #region Warp Variables
        public static readonly int WARP_NAME_MAX_LENGTH = 30;

        private static Vector2 warpsScrollPosition = Vector2.zero;
        private static int warpsToolbarTab = 0;

        private static string[] warpsToolbarTabTitles =
        {
            "Presets",
            "Custom"
        };

        private static bool saveAmmoToggle = false;

        private List<WarpData> userWarps;
        private bool refreshUserWarpsFlag = true;
        private string newUserWarpText = "New warp";
        #endregion

        #region Game Timer Variables
        private GameTimer gameTimer;
        #endregion

        #region Gordo Variables
        private static Vector2 gordoScrollPosition = Vector2.zero;
        #endregion

        #region Spawner Variables
        private static readonly int spawnerWindowWidth = 300;
        private static readonly int spawnerWindowHeight = 450;
        private static readonly string spawnerWindowTitle = "Spawner Info";
        private static readonly int spawnerWindowId = 33734;

        private static Rect spawnerWindowRect = new Rect(Screen.width - spawnerWindowWidth, Screen.height - spawnerWindowHeight, spawnerWindowWidth, spawnerWindowHeight); // Bottom-right corner

        private RaycastHit rayHit = new RaycastHit();
        private SpawnerInfoNode targetSpawner;

        private bool showSpawners = false;
        private bool spawnerShowTriggerRate = true;
        private bool spawnerConvertToPercentage = false;
        private bool spawnerShowAvgNextSpawn = true;
        private bool spawnerShowNextSpawnTime = true; // TODO: Show the actual next possible spawn time
        private bool spawnerShowCountRange = true;
        #endregion

        #region GIF Recorder Variables
        private const float GIF_LENGTH_MIN = 3.5f; // 3.5 = game's default
        private const float GIF_LENGTH_MAX = 10.0f;

        private bool gifLengthWasChanged = false;

        private readonly FieldInfo gifLengthField = typeof(GifRecorder).GetField("GIF_LENGTH", BindingFlags.Static | BindingFlags.NonPublic);
        #endregion

        #region Unity/UMF and Initialization
        internal static void Log(string text, bool clean = false)
        {
            using (UMFLog log = new UMFLog()) log.Log(text, clean);
        }

        [UMFConfig]
        public static void LoadConfig()
        {
            SRSHConfig.Load();
        }

		void Awake()
		{
			Log("SRSpeedrunHelper v" + UMFMod.GetModVersion().ToString(), true);
            UMFGUI.RegisterPauseHandler(Pause);

            // Register Keybinds
            UMFGUI.RegisterBind("BindShowMenu", SRSHConfig.bind_showMenu.ToString(), () => showMenu = !showMenu);

            UMFGUI.RegisterBind("BindUserWarp1", SRSHConfig.bind_userWarp1.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(0)));
            UMFGUI.RegisterBind("BindUserWarp2", SRSHConfig.bind_userWarp2.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(1)));
            UMFGUI.RegisterBind("BindUserWarp3", SRSHConfig.bind_userWarp3.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(2)));
            UMFGUI.RegisterBind("BindUserWarp4", SRSHConfig.bind_userWarp4.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(3)));
            UMFGUI.RegisterBind("BindUserWarp5", SRSHConfig.bind_userWarp5.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(4)));
            UMFGUI.RegisterBind("BindUserWarp6", SRSHConfig.bind_userWarp6.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(5)));
            UMFGUI.RegisterBind("BindUserWarp7", SRSHConfig.bind_userWarp7.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(6)));
            UMFGUI.RegisterBind("BindUserWarp8", SRSHConfig.bind_userWarp8.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(7)));
            UMFGUI.RegisterBind("BindUserWarp9", SRSHConfig.bind_userWarp9.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(8)));
            UMFGUI.RegisterBind("BindUserWarp10", SRSHConfig.bind_userWarp10.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(9)));
            UMFGUI.RegisterBind("BindUserWarp11", SRSHConfig.bind_userWarp11.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(10)));
            UMFGUI.RegisterBind("BindUserWarp12", SRSHConfig.bind_userWarp12.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(11)));

            UMFGUI.RegisterBind("BindStartTimer", SRSHConfig.bind_startTimer.ToString(), StartTimer);
            UMFGUI.RegisterBind("BindStopTimer", SRSHConfig.bind_stopTimer.ToString(), StopTimer);
            UMFGUI.RegisterBind("BindResetTimer", SRSHConfig.bind_resetTimer.ToString(), ResetTimer);

            // Initialize GUI Styles
            LABEL_STYLE_DEFAULT.fontSize = 16;
            LABEL_STYLE_DEFAULT.normal.textColor = Color.white;

            LABEL_STYLE_BOLD.fontSize = 16;
            LABEL_STYLE_BOLD.fontStyle = FontStyle.Bold;
            LABEL_STYLE_BOLD.normal.textColor = Color.white;

            TEXT_STYLE_MOD_WARNING.fontSize = 32;
            TEXT_STYLE_MOD_WARNING.fontStyle = FontStyle.Bold;
            TEXT_STYLE_MOD_WARNING.normal.textColor = Color.black;

            TEXT_STYLE_HEADER.fontSize = 24;
            TEXT_STYLE_HEADER.fontStyle = FontStyle.Bold;
            TEXT_STYLE_HEADER.normal.textColor = Color.white;

            gameTimer = gameObject.AddComponent<GameTimer>();
        }

        public static void Pause(bool pause)
        {
            TimeDirector timeDirector = null;
            try
            {
                timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
            }
            catch { }
            if (!timeDirector) return;
            if (pause)
            {
                if (!timeDirector.HasPauser()) timeDirector.Pause();
            }
            else timeDirector.Unpause();
        }

        void Update()
        {
            if(showSpawners)
            {
                if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f)), out rayHit, 40.0f))
                {
                    SpawnerInfoNode temp = rayHit.collider.GetComponent<SpawnerInfoNode>();
                    if(temp != null)
                    {
                        if(targetSpawner != temp)
                        {
                            targetSpawner?.SetIsBeingLookedAt(false);
                            temp.SetIsBeingLookedAt(true);
                        }
                        targetSpawner = temp;
                    }
                    else
                    {
                        if(targetSpawner != null)
                        {
                            targetSpawner.SetIsBeingLookedAt(false);
                        }
                        targetSpawner = null;
                    }
                }
                else
                {
                    if(targetSpawner != null)
                    {
                        targetSpawner.SetIsBeingLookedAt(false);
                    }
                    targetSpawner = null;
                }
            }
        }
        #endregion

        #region GUI
        void OnGUI()
        {
            // Modify GUI skin values
            // Have to do this in OnGUI for compatability with other mods (namely, SRCheatMenu)
            GUI.skin.button.fontSize = 16;
            GUI.skin.button.fontStyle = FontStyle.Normal;
            GUI.skin.textField.fontSize = 16;
            GUI.skin.textField.fontStyle = FontStyle.Normal;
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
            GUI.skin.toggle.fontStyle = FontStyle.Normal;
            GUI.skin.toggle.fontSize = 16;
            GUI.skin.toggle.fontStyle = FontStyle.Normal;
            GUI.skin.window.fontSize = 16;
            GUI.skin.window.fontStyle = FontStyle.Bold;

            if (!Levels.isMainMenu() && !Levels.isSpecial())
            {
                if(showMenu)
                {
                    windowRect = GUILayout.Window(windowId, windowRect, ShowMenu, windowTitle);
                }
                if(showSpawners && targetSpawner != null)
                {
                    spawnerWindowRect = GUILayout.Window(spawnerWindowId, spawnerWindowRect, ShowSpawnerMenu, spawnerWindowTitle);
                }
            }
            if(SRSHConfig.showModWarning && Levels.isMainMenu())
            {
                GUI.Label(modWarningRect, "Reminder: DO NOT submit runs with mods or mod loaders installed (including this one!)\nThis warning can be turned off in the mod's settings (Shift+F10)", TEXT_STYLE_MOD_WARNING);
            }
        }

        void ShowMenu(int winId)
        {
            if (windowId != winId)
            {
                Log("Warning: Wrong window ID passed to ShowMenu.");
                return;
            }

            currentToolbarTab = GUILayout.Toolbar(currentToolbarTab, toolbarTabTitles);
            switch (currentToolbarTab)
            {
                case (0):
                    // Warp settings
                    warpsToolbarTab = GUILayout.Toolbar(warpsToolbarTab, warpsToolbarTabTitles);
                    switch(warpsToolbarTab)
                    {
                        // Predefined warps
                        case (0):
                            // Lay out the labels and buttons for the predefined warps
                            foreach (KeyValuePair<WarpData[], string> area in WarpData.ALL_AREA_WARPS)
                            {
                                GUILayout.Label(area.Value, LABEL_STYLE_BOLD);

                                GUILayout.BeginHorizontal();
                                foreach (WarpData warp in area.Key)
                                {
                                    if (GUILayout.Button(warp.Name))
                                    {
                                        WarpPlayer(warp);
                                    }
                                }
                                GUILayout.EndHorizontal();
                            }
                            break;

                        // User warps
                        case (1):
                            warpsScrollPosition = GUILayout.BeginScrollView(warpsScrollPosition);

                            if (refreshUserWarpsFlag || userWarps == null)
                            {
                                RefreshUserWarpList();
                                refreshUserWarpsFlag = false;
                            }

                            // List user warps
                            foreach (WarpData warpData in userWarps)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label(warpData.Name, LABEL_STYLE_BOLD);
                                if (GUILayout.Button("Warp"))
                                {
                                    WarpPlayer(warpData);
                                }
                                else if (GUILayout.Button("Remove"))
                                {
                                    UserWarps.RemoveUserWarp(warpData);
                                    refreshUserWarpsFlag = true;
                                }
                                else if (GUILayout.Button("^"))
                                {
                                    UserWarps.MoveWarpUp(userWarps.IndexOf(warpData));
                                    refreshUserWarpsFlag = true;
                                }
                                else if (GUILayout.Button("v"))
                                {
                                    UserWarps.MoveWarpDown(userWarps.IndexOf(warpData));
                                    refreshUserWarpsFlag = true;
                                }

                                GUILayout.EndHorizontal();
                            }

                            // Add new user warp bar
                            GUILayout.BeginHorizontal();
                            newUserWarpText = GUILayout.TextField(newUserWarpText, WARP_NAME_MAX_LENGTH);
                            saveAmmoToggle = GUILayout.Toggle(saveAmmoToggle, "Save Ammo");
                            if (GUILayout.Button("Add"))
                            {
                                InventoryData newInventoryData = null;

                                if (saveAmmoToggle)
                                {
                                    PlayerState playerStateTmp = SRSingleton<SceneContext>.Instance.PlayerState;
                                    newInventoryData = new InventoryData(playerStateTmp.GetAmmoMode());

                                    for (int i = 0; i < playerStateTmp.Ammo.GetUsableSlotCount(); i++)
                                    {
                                        Log("Slot " + i.ToString());
                                        newInventoryData.AddSlot(playerStateTmp.Ammo.GetSlotName(i), playerStateTmp.Ammo.GetSlotCount(i));
                                        Log("Id: " + playerStateTmp.Ammo.GetSlotName(i).ToString());
                                        Log("Count: " + playerStateTmp.Ammo.GetSlotCount(i));
                                    }
                                }

                                PlayerModel playerModelTmp = GetPlayerModel();
                                UserWarps.AddUserWarp(new WarpData(playerModelTmp.GetPos(), playerModelTmp.GetRot().eulerAngles, newUserWarpText, playerModelTmp.currRegionSetId, newInventoryData));

                                newUserWarpText = "New warp";
                                refreshUserWarpsFlag = true;
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.EndScrollView();
                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button("Save warps to disk (Warning: will overwrite any warps currently present in config file)"))
                            {
                                UserWarps.WriteToFile();
                            }
                            if (GUILayout.Button("Load warps from disk"))
                            {
                                UserWarps.LoadFromFile();
                                RefreshUserWarpList();
                            }
                            
                            break;

                        default:
                            GUILayout.Label("You should never see this. Oops!");
                            break;
                    }

                    break;

                case (1):
                    // Timer settings
                    gameTimer.showTimer = GUILayout.Toggle(gameTimer.showTimer, "Show timer");

                    if (gameTimer.showTimer)
                    {
                        if (GUILayout.Button("Start timer"))
                        {
                            StartTimer();
                        }
                        else if (GUILayout.Button("Stop timer"))
                        {
                            StopTimer();
                        }
                        else if (GUILayout.Button("Reset timer"))
                        {
                            ResetTimer();
                        }
                        gameTimer.showMilliseconds = GUILayout.Toggle(gameTimer.showMilliseconds, "Show milliseconds");
                    }
                    break;

                case (2):
                    // Gordo settings
                    gordoScrollPosition = GUILayout.BeginScrollView(gordoScrollPosition);

                    GameModel gameModel = SRSingleton<SceneContext>.Instance.GameModel;

                    // Present them in the predefined order defined in GordoHelper.gordoIdsOrdered
                    // Excludes Party Gordos, Gold Gordos (Rush Mode), and snared Gordos
                    foreach(string gordoId in GordoHelper.gordoIdsOrdered)
                    {
                        GordoModel gordoModel = gameModel.GetGordoModel(gordoId);
                        string gordoName;

                        if(GordoHelper.gordoIdToName.TryGetValue(gordoId, out gordoName))
                        {
                            GUILayout.Label(gordoName, LABEL_STYLE_BOLD);
                        }

                        GUILayout.Label(GordoHelper.GetGordoStatus(gordoId), LABEL_STYLE_DEFAULT);
                        if(GUILayout.Button("Reset Gordo"))
                        {
                            GordoHelper.ResetGordo(gordoId);
                        }
                    }

                    GUILayout.EndScrollView();
                    break;

                case (3):
                    // Spawner view settings
                    bool newShowSpawners = GUILayout.Toggle(showSpawners, "Show slime spawners");
                    if(newShowSpawners != showSpawners)
                    {
                        if(newShowSpawners)
                        {
                            SpawnerInfoNode.ActivateNodes();
                        }
                        else
                        {
                            SpawnerInfoNode.DeactivateNodes();
                        }
                        showSpawners = newShowSpawners;
                    }

                    spawnerShowCountRange = GUILayout.Toggle(spawnerShowCountRange, "Show minimum and maximum amount of slimes spawned from this spawner");
                    spawnerShowTriggerRate = GUILayout.Toggle(spawnerShowTriggerRate, "Show spawn chance of spawners once triggered");
                    spawnerConvertToPercentage = GUILayout.Toggle(spawnerConvertToPercentage, "Show spawn chance in percentage rather than decimal");
                    spawnerShowAvgNextSpawn = GUILayout.Toggle(spawnerShowAvgNextSpawn, "Show average amount of time until the next possible spawn after a trigger");
                    //spawnerShowNextSpawnTime = GUILayout.Toggle(spawnerShowNextSpawnTime, "Show the next time this spawner can be triggered"); requires reflection, stored in SpawnerTriggerModel
                    break;

                case (4):
                    // Gif Recorder settings
                    GUILayout.Label("GIF Settings", LABEL_STYLE_BOLD);
                    OptionsDirector optionsDirector = SRSingleton<GameContext>.Instance.OptionsDirector;

                    if(gifLengthWasChanged)
                    {
                        optionsDirector.bufferForGif = true;
                    }

                    float currGifLength = (float)gifLengthField.GetValue(null);
                    GUILayout.Label("GIF Length (seconds): " + currGifLength.ToString("0.##"), LABEL_STYLE_DEFAULT);
                    float newGifLength = GUILayout.HorizontalSlider(currGifLength, GIF_LENGTH_MIN, GIF_LENGTH_MAX);
                    if (currGifLength != newGifLength)
                    {
                        // Change gif length
                        currGifLength = newGifLength;
                        gifLengthField.SetValue(null, currGifLength);

                        // Clear current gif buffer if GIF buffer is on in game settings
                        if(optionsDirector.bufferForGif)
                        {
                            gifLengthWasChanged = true;
                            optionsDirector.bufferForGif = false; // This will clear the gif buffer the next time GifRecorder.Update is called. Tried to clear it directly but ran into issues
                        }
                    }

                    GUILayout.Label("Warning: Changing this value will clear the current GIF buffer\n(i.e. the last x seconds of recording will be erased)", LABEL_STYLE_DEFAULT);

                    GUILayout.FlexibleSpace();
                    GUILayout.Label("More features to be added here in future versions. Taking requests on Discord!", LABEL_STYLE_BOLD);
                    break;

                default:
                    GUILayout.Label("It should be impossible to see this. Oops!");
                    break;
            }

            GUI.DragWindow();
        }

        void ShowSpawnerMenu(int winId)
        {
            if(winId != spawnerWindowId)
            {
                Log("Warning: Wrong window ID passed to ShowSpawnerMenu.");
                return;
            }

            GUILayout.Label(GenerateSpawnerWindowText(targetSpawner.SpawnerTrigger), LABEL_STYLE_DEFAULT);
        }
        #endregion

        #region Warp Logic
        void RefreshUserWarpList()
        {
            userWarps = UserWarps.GetUserWarps();
        }

        void WarpPlayer(WarpData warpData)
        {
            if(warpData == null)
            {
                return;
            }

            PlayerModel playerModelTmp = GetPlayerModel();
            playerModelTmp.SetTransform(warpData.Position, warpData.RotEuler);
            playerModelTmp.SetCurrRegionSet(warpData.RegionSetId);

            if(warpData.HasInventoryData())
            {
                SetPlayerSlots(warpData.InventoryData);
            }
        }

        void SetPlayerSlots(InventoryData inventoryData)
        {
            if(inventoryData == null)
            {
                return;
            }

            PlayerState playerStateTmp = SRSingleton<SceneContext>.Instance.PlayerState;
            playerStateTmp.SetAmmoMode(inventoryData.AmmoMode);

            playerStateTmp.Ammo.Clear();
            int slotNum = 0;

            foreach(InventoryData.IdentifiableCountPair slot in inventoryData.AmmoList)
            {
                playerStateTmp.Ammo.MaybeAddToSpecificSlot(slot.Id, null, slotNum, slot.Count, true);

                // Special case for Slimes
                // Since we don't pass an Identifiable to Ammo.MaybeAddToSpecificSlot, SlimeEmotion data never gets added (stays null)
                // This causes a bug when trying to shoot slimes added to ammo slots
                // Calling Ammo.Replace adds default SlimeEmotion data
                // Could probably do this in a less roundabout way using reflection (ammo slots are private)
                if(Identifiable.IsSlime(slot.Id))
                {
                    playerStateTmp.Ammo.Replace(slot.Id, slot.Id);
                }
                slotNum++;
            }
        }
        #endregion

        #region Timer Logic
        private void StartTimer()
        {
            gameTimer.showTimer = true; // if the start hotkey is pressed, make the timer visible
            if(gameTimer != null)
            {
                gameTimer.StartTimer();
            }
        }

        private void StopTimer()
        {
            if (gameTimer != null)
            {
                gameTimer.StopTimer();
            }
        }

        private void ResetTimer()
        {
            if (gameTimer != null)
            {
                gameTimer.ResetTimer();
            }
        }
        #endregion

        #region Spawner Logic
        /*
        void ActivateSpawner()
        {
            if(targetSpawner != null)
            {
                // reimplement spawning logic from SpawnerTrigger.OnTriggerEnter() but without the time/collider checks
            }
        }
        */

        string GenerateSpawnerWindowText(SpawnerTrigger st)
        {
            // Show slimes and times
            string text = "";
            foreach (DirectedActorSpawner.SpawnConstraint constraint in st.spawner.constraints)
            {
                string t = "";
                DirectedActorSpawner.TimeMode timeMode = constraint.window.timeMode;
                switch (timeMode)
                {
                    case DirectedActorSpawner.TimeMode.ANY:
                        t = "Any Time:";
                        break;
                    case DirectedActorSpawner.TimeMode.DAY:
                        t = "Daytime:";
                        break;
                    case DirectedActorSpawner.TimeMode.NIGHT:
                        t = "Nighttime:";
                        break;
                    default:
                        t = "Custom Time: " + constraint.window.startHour + "-" + constraint.window.endHour;
                        break;
                }
                text += t + "\n";

                foreach (SlimeSet.Member slimeSet in constraint.slimeset.members)
                {
                    string tmp = slimeSet.prefab.ToString();
                    text += tmp.Substring(0, tmp.IndexOf(" "));
                    text += ": " + slimeSet.weight.ToString() + "\n";
                }

                text += "\n";
            }

            // Add more data based on settings
            if (spawnerShowCountRange)
            {
                text += "Spawn amount: " + st.minSpawn + " - " + st.maxSpawn + "\n";
            }

            if (spawnerShowTriggerRate)
            {
                if(spawnerConvertToPercentage)
                {
                    text += "Spawn chance: " + st.chanceOfTrigger*100 + "%\n";
                }
                else
                {
                    text += "Spawn chance: " + st.chanceOfTrigger + "\n";
                }
            }
            if(spawnerShowAvgNextSpawn)
            {
                text += "Avg. hours until next spawn chance: " + st.avgGameHoursBetweenTrigger + "\n";
            }

            return text;
        }
        #endregion

        PlayerModel GetPlayerModel()
        {
            return SRSingleton<SceneContext>.Instance.GameModel.GetPlayerModel();
        }

	}
}