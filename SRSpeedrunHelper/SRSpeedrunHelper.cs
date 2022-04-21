using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UModFramework.API;
using MonomiPark.SlimeRancher.DataModel;
using System;

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
        private static bool showMenu = true;

        private static int currentToolbarTab = 0;
        private static readonly string[] toolbarTabTitles =
        {
            "Save States",
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

        #region Save State Variables
        public static readonly int WARP_NAME_MAX_LENGTH = 30;

        private static Vector2 warpsScrollPosition = Vector2.zero;
        private static int warpsToolbarTab = 0;

        private static string[] warpsToolbarTabTitles =
        {
            "Presets",
            "Custom",
            "Create"
        };

        private static bool saveAmmoToggle = false;
        private static bool saveHealthToggle = true;
        private static bool saveEnergyToggle = true;
        private static bool saveNewbucksToggle = false;

        private List<WarpData> userWarps;
        private bool refreshUserWarpsFlag = true;
        private string newUserWarpText = "New save state";
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
        private bool spawnerShowAvgNextSpawn = true;
        private bool spawnerShowNextSpawnTime = true; // TODO: Show the actual next possible spawn time
        private bool spawnerShowCountRange = true;
        private bool spawnerConvertToPercentage = false;
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

            UMFGUI.RegisterBind("BindSavestate1", SRSHConfig.bind_userWarp1.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(0)));
            UMFGUI.RegisterBind("BindSavestate2", SRSHConfig.bind_userWarp2.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(1)));
            UMFGUI.RegisterBind("BindSavestate3", SRSHConfig.bind_userWarp3.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(2)));
            UMFGUI.RegisterBind("BindSavestate4", SRSHConfig.bind_userWarp4.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(3)));
            UMFGUI.RegisterBind("BindSavestate5", SRSHConfig.bind_userWarp5.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(4)));
            UMFGUI.RegisterBind("BindSavestate6", SRSHConfig.bind_userWarp6.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(5)));
            UMFGUI.RegisterBind("BindSavestate7", SRSHConfig.bind_userWarp7.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(6)));
            UMFGUI.RegisterBind("BindSavestate8", SRSHConfig.bind_userWarp8.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(7)));
            UMFGUI.RegisterBind("BindSavestate9", SRSHConfig.bind_userWarp9.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(8)));
            UMFGUI.RegisterBind("BindSavestate10", SRSHConfig.bind_userWarp10.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(9)));
            UMFGUI.RegisterBind("BindSavestate11", SRSHConfig.bind_userWarp11.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(10)));
            UMFGUI.RegisterBind("BindSavestate12", SRSHConfig.bind_userWarp12.ToString(), () => WarpPlayer(UserWarps.GetWarpDataByIndex(11)));

            UMFGUI.RegisterBind("BindStartTimer", SRSHConfig.bind_startTimer.ToString(), StartTimer);
            UMFGUI.RegisterBind("BindStopTimer", SRSHConfig.bind_stopTimer.ToString(), StopTimer);
            UMFGUI.RegisterBind("BindResetTimer", SRSHConfig.bind_resetTimer.ToString(), ResetTimer);

            UMFGUI.RegisterBind("BindSpawnCrate", SRSHConfig.bind_spawnCrate.ToString(), SpawnCrate);

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

            if(Levels.isMainMenu() || Levels.isSpecial())
            {
                SpawnerInfoNode.ClearNodes();
                showSpawners = false;
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
                if(showMenu && IsGamePaused())
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

                                        if(SRSHConfig.saveStateCloseMenu)
                                        {
                                            showMenu = false;
                                            SRSingleton<PauseMenu>.Instance.UnPauseGame();
                                        }
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
                                if (GUILayout.Button("Load"))
                                {
                                    WarpPlayer(warpData);

                                    if (SRSHConfig.saveStateCloseMenu)
                                    {
                                        showMenu = false;
                                        SRSingleton<PauseMenu>.Instance.UnPauseGame();
                                    }
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

                            GUILayout.EndScrollView();
                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button("Save save states to disk (Warning: will overwrite any save states currently present in config file)"))
                            {
                                UserWarps.WriteToFile();
                            }
                            if (GUILayout.Button("Load save states from disk"))
                            {
                                UserWarps.LoadFromFile();
                                RefreshUserWarpList();
                            }
                            
                            break;

                        case (2):
                            // Create custom warps tab
                            GUILayout.Label("Save state name", LABEL_STYLE_BOLD);
                            newUserWarpText = GUILayout.TextField(newUserWarpText, WARP_NAME_MAX_LENGTH);

                            GUILayout.Label("\nOptions", LABEL_STYLE_BOLD);

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
                                        Log("Slot " + i.ToString());
                                        newInventoryData.AddSlot(playerStateTmp.Ammo.GetSlotName(i), playerStateTmp.Ammo.GetSlotCount(i));
                                        Log("Id: " + playerStateTmp.Ammo.GetSlotName(i).ToString());
                                        Log("Count: " + playerStateTmp.Ammo.GetSlotCount(i));
                                    }
                                }

                                PlayerModel playerModelTmp = GetPlayerModel();
                                
                                WarpData warpDataTmp = new WarpData(playerModelTmp.GetPos(), playerModelTmp.GetRot().eulerAngles, newUserWarpText, playerModelTmp.currRegionSetId, newInventoryData);

                                if(saveHealthToggle)
                                {
                                    warpDataTmp.PlayerHealth = playerModelTmp.currHealth;
                                }
                                if(saveEnergyToggle)
                                {
                                    warpDataTmp.PlayerEnergy = playerModelTmp.currEnergy;
                                }
                                if(saveNewbucksToggle)
                                {
                                    warpDataTmp.PlayerNewbucks = playerModelTmp.currency;
                                }

                                UserWarps.AddUserWarp(warpDataTmp);

                                newUserWarpText = "New save state";
                                refreshUserWarpsFlag = true;

                                warpsToolbarTab = 1; // switch to Custom tab to indicate the save state was added and to show it in the list
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

                    spawnerConvertToPercentage = GUILayout.Toggle(spawnerConvertToPercentage, "Show probabilities/weights in percentage rather than decimal");
                    spawnerShowCountRange = GUILayout.Toggle(spawnerShowCountRange, "Show minimum and maximum amount of slimes spawned from this spawner");
                    spawnerShowTriggerRate = GUILayout.Toggle(spawnerShowTriggerRate, "Show spawn chance of spawners once triggered");
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

                    GUILayout.Label("Warning: Changing this value will clear the current GIF buffer\n(i.e. the last x seconds of recording will be erased)\n", LABEL_STYLE_DEFAULT);

                    // Spawn a crate in front of the player
                    if(GUILayout.Button("Spawn crate"))
                    {
                        SpawnCrate();
                    }

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
            if(Levels.isMainMenu() || Levels.isSpecial())
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

            if(warpData.PlayerHealth != null)
            {
                playerModelTmp.SetHealth((float)warpData.PlayerHealth);
                playerModelTmp.healthBurstAfter = SceneContext.Instance.TimeDirector.WorldTime() + 300.0;
            }
            if(warpData.PlayerEnergy != null)
            {
                playerModelTmp.SetEnergy((float)warpData.PlayerEnergy);
                playerModelTmp.energyRecoverAfter = SceneContext.Instance.TimeDirector.WorldTime() + 300.0;
            }
            if(warpData.PlayerNewbucks != null)
            {
                playerModelTmp.SetCurrency((int)warpData.PlayerNewbucks);
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

                float weightsSum = 0.0f;

                if(spawnerConvertToPercentage)
                {
                    foreach(SlimeSet.Member slimeSet in constraint.slimeset.members)
                    {
                        weightsSum += slimeSet.weight;
                    }
                }

                foreach (SlimeSet.Member slimeSet in constraint.slimeset.members)
                {
                    string tmp = slimeSet.prefab.ToString();
                    text += tmp.Substring(0, tmp.IndexOf(" "));
                    
                    if(spawnerConvertToPercentage)
                    {
                        text += ": " + (slimeSet.weight / weightsSum) * 100 + "%\n";
                    }
                    else
                    {
                        text += ": " + slimeSet.weight + "\n";
                    }
                    
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

        #region Misc methods
        PlayerModel GetPlayerModel()
        {
            return SRSingleton<SceneContext>.Instance.GameModel.GetPlayerModel();
        }

        void SpawnCrate()
        {
            if(Levels.isMainMenu() || Levels.isSpecial())
            {
                return;
            }

            Transform playerTransform = SceneContext.Instance.Player.transform;
            Vector3 cratePos = playerTransform.TransformPoint(Vector3.forward * 5 + new Vector3(0, 1.8f, 0));

            GameObject cratePrefab = GameContext.Instance.LookupDirector.GetPrefab(Identifiable.Id.CRATE_REEF_01);
            SRBehaviour.InstantiateActor(cratePrefab, GetPlayerModel().currRegionSetId, cratePos, Quaternion.identity);
        }

        // Returns whether or not the game is paused
        // Logic mostly copied from Pause method, may want to merge some functionality
        bool IsGamePaused()
        {
            TimeDirector timeDirector = null;
            try
            {
                timeDirector = SRSingleton<SceneContext>.Instance.TimeDirector;
            }
            catch(Exception e)
            {
                Log(e.Message);
            }

            if(!timeDirector)
            {
                return false;
            }

            return timeDirector.HasPauser();
        }
        #endregion

    }
}