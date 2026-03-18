using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UModFramework.API;
using MonomiPark.SlimeRancher.DataModel;
using System;
using SRSpeedrunHelper.Warps;
using SRSpeedrunHelper.Timer;
using SRSpeedrunHelper.Spawners;
using SRSpeedrunHelper.Gordos;

namespace SRSpeedrunHelper
{
    [UMFHarmony(1)] //Set this to the number of harmony patches in your mod.
    [UMFScript]
    class SRSpeedrunHelper : MonoBehaviour
    {
        #region General GUI Variables
        // TODO: this probably sucks on 4K, consider redoing this to scale to high res displays. make scale option?
        private static readonly int windowSizeX = 700;
        private static readonly int windowSizeY = 500;
        private static readonly string windowTitle = "Speedrun Helper Menu";
        private static readonly int windowId = 1258;

        private static Rect windowRect = new Rect(Screen.width - windowSizeX, 0, windowSizeX, windowSizeY); // Window dimensions and default position. Appears in top-right corner
        internal static bool showMenu = true;

        internal static int currentToolbarTab = 0;
        private static readonly string[] toolbarTabTitles = //maybe should move these to individual GUI classes idk. refactor 2 when
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

        #region Game Timer Variables
        internal static GameTimer gameTimer;
        #endregion


        #region Spawner Variables
        // TODO: same as main window, consider dynamic/configurable window size for high resolutions
        private static readonly int spawnerWindowWidth = 300;
        private static readonly int spawnerWindowHeight = 450;
        private static readonly string spawnerWindowTitle = "Spawner Info";
        private static readonly int spawnerWindowId = 33734;

        private static Rect spawnerWindowRect = new Rect(Screen.width - spawnerWindowWidth, Screen.height - spawnerWindowHeight, spawnerWindowWidth, spawnerWindowHeight); // Bottom-right corner

        private RaycastHit rayHit = new RaycastHit();
        private SpawnerInfoNode targetSpawner;

        private bool showSpawners = false;

        public static bool spawnerShowTriggerRate = true;
        public static bool spawnerShowAvgNextSpawn = true;
        public static bool spawnerShowNextSpawnTime = true;
        public static bool spawnerShowCountRange = true;
        public static bool spawnerConvertToPercentage = false;
        #endregion

        #region GIF Recorder Variables
        private const float GIF_LENGTH_MIN = 3.5f; // 3.5 = game's default
        private const float GIF_LENGTH_MAX = 10.0f; //TODO: why 10 seconds max?

        private bool gifLengthWasChanged = false;

        private readonly FieldInfo gifLengthField = typeof(GifRecorder).GetField("GIF_LENGTH", BindingFlags.Static | BindingFlags.NonPublic);
        #endregion

        #region Misc Variables
        private static bool disableEnergyRecovery = false;
        public static bool disableFirestorms = false;
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

            UMFGUI.RegisterBind("BindSavestate1", SRSHConfig.bind_userWarp1.ToString(), () => UserWarps.WarpPlayer(UserWarps.GetWarpDataByIndex(0)));
            UMFGUI.RegisterBind("BindSavestate2", SRSHConfig.bind_userWarp2.ToString(), () => UserWarps.WarpPlayer(UserWarps.GetWarpDataByIndex(1)));
            UMFGUI.RegisterBind("BindSavestate3", SRSHConfig.bind_userWarp3.ToString(), () => UserWarps.WarpPlayer(UserWarps.GetWarpDataByIndex(2)));
            UMFGUI.RegisterBind("BindSavestate4", SRSHConfig.bind_userWarp4.ToString(), () => UserWarps.WarpPlayer(UserWarps.GetWarpDataByIndex(3)));
            UMFGUI.RegisterBind("BindSavestate5", SRSHConfig.bind_userWarp5.ToString(), () => UserWarps.WarpPlayer(UserWarps.GetWarpDataByIndex(4)));
            UMFGUI.RegisterBind("BindSavestate6", SRSHConfig.bind_userWarp6.ToString(), () => UserWarps.WarpPlayer(UserWarps.GetWarpDataByIndex(5)));
            UMFGUI.RegisterBind("BindSavestate7", SRSHConfig.bind_userWarp7.ToString(), () => UserWarps.WarpPlayer(UserWarps.GetWarpDataByIndex(6)));
            UMFGUI.RegisterBind("BindSavestate8", SRSHConfig.bind_userWarp8.ToString(), () => UserWarps.WarpPlayer(UserWarps.GetWarpDataByIndex(7)));
            UMFGUI.RegisterBind("BindSavestate9", SRSHConfig.bind_userWarp9.ToString(), () => UserWarps.WarpPlayer(UserWarps.GetWarpDataByIndex(8)));
            UMFGUI.RegisterBind("BindSavestate10", SRSHConfig.bind_userWarp10.ToString(), () => UserWarps.WarpPlayer(UserWarps.GetWarpDataByIndex(9)));
            UMFGUI.RegisterBind("BindSavestate11", SRSHConfig.bind_userWarp11.ToString(), () => UserWarps.WarpPlayer(UserWarps.GetWarpDataByIndex(10)));
            UMFGUI.RegisterBind("BindSavestate12", SRSHConfig.bind_userWarp12.ToString(), () => UserWarps.WarpPlayer(UserWarps.GetWarpDataByIndex(11)));

            UMFGUI.RegisterBind("BindStartTimer", SRSHConfig.bind_startTimer.ToString(), () => gameTimer.StartTimer());
            UMFGUI.RegisterBind("BindPauseTimer", SRSHConfig.bind_pauseTimer.ToString(), () => gameTimer.PauseTimer());
            UMFGUI.RegisterBind("BindResetTimer", SRSHConfig.bind_resetTimer.ToString(), () => gameTimer.ResetTimer());

            UMFGUI.RegisterBind("BindSpawnCrate", SRSHConfig.bind_spawnCrate.ToString(), SpawnCrate);
            UMFGUI.RegisterBind("ForceSpawnTrigger", SRSHConfig.bind_forceSpawnTrigger.ToString(), ForceSpawnTrigger);

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
            TimerGUI.RegisterTimer(gameTimer);

            // Pre-load custom user warps
            UserWarps.LoadWarps();
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
            if(disableEnergyRecovery)
            {
                SetEnergyRecoverAfter(double.PositiveInfinity);
            }

            if(showSpawners)
            {
                // TODO: For efficiency, try hijacking the player's Raycast that's used to identify what Identifiable is being looked at
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
                        targetSpawner?.SetIsBeingLookedAt(false);
                        targetSpawner = null;
                    }
                }
                else
                {
                    targetSpawner?.SetIsBeingLookedAt(false);
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
        public void OnGUI()
        {
            // Modify GUI skin values
            // Have to do this in OnGUI for compatability with other mods (mainly SRCheatMenu)
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

        internal void ShowMenu(int winId)
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
                    WarpGUI.DoGUI();
                    break;

                case (1):
                    TimerGUI.DoGUI();
                    break;

                case (2):
                    GordoGUI.DoGUI();
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
                    spawnerShowNextSpawnTime = GUILayout.Toggle(spawnerShowNextSpawnTime, "Show the time that must be passed in order for this spawner to trigger");
                    //spawnerShowNextSpawnTime = GUILayout.Toggle(spawnerShowNextSpawnTime, "Show the next time this spawner can be triggered"); requires reflection, stored in SpawnerTriggerModel
                    break;

                case (4): // Misc Settings
                    // Energy settings
                    GUILayout.Label("Energy Settings", LABEL_STYLE_BOLD);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Set Energy to 0"))
                    {
                        SetPlayerEnergy(0f);
                    }

                    if (disableEnergyRecovery != GUILayout.Toggle(disableEnergyRecovery, "Disable Energy recovery"))
                    {
                        disableEnergyRecovery = !disableEnergyRecovery;
                        if (!disableEnergyRecovery)
                        {
                            SetEnergyRecoverAfter(SceneContext.Instance.TimeDirector.WorldTime() + 300.0);
                        }
                    }
                    GUILayout.EndHorizontal();


                    // Gif Recorder settings
                    GUILayout.Label("\nGIF Settings", LABEL_STYLE_BOLD);
                    OptionsDirector optionsDirector = SRSingleton<GameContext>.Instance.OptionsDirector;

                    if(gifLengthWasChanged)
                    {
                        optionsDirector.bufferForGif = true;
                    }

                    float currGifLength = (float)gifLengthField.GetValue(null);
                    GUILayout.Label("GIF Length: " + currGifLength.ToString("0.## sec."), LABEL_STYLE_DEFAULT);
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

                    GUILayout.Label("\nOther", LABEL_STYLE_BOLD);
                    if(disableFirestorms != GUILayout.Toggle(disableFirestorms, "Disable Firestorms"))
                    {
                        disableFirestorms = !disableFirestorms;
                        SetFirestormsActive(!disableFirestorms);
                    }

                    // Spawn a crate in front of the player
                    if (GUILayout.Button("Spawn crate"))
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

        private void ShowSpawnerMenu(int winId)
        {
            if(winId != spawnerWindowId)
            {
                Log("Warning: Wrong window ID passed to ShowSpawnerMenu.");
                return;
            }

            GUILayout.Label(targetSpawner.GetInfoText(), LABEL_STYLE_DEFAULT);
        }
        #endregion

        #region Spawner Logic
        void ForceSpawnTrigger()
        {
            targetSpawner?.ForceSpawn();
        }
        #endregion

        #region Misc Methods
        internal static PlayerModel GetPlayerModel()
        {
            return SceneContext.Instance.GameModel.GetPlayerModel();
        }

        internal static void SpawnCrate()
        {
            if(Levels.isMainMenu() || Levels.isSpecial())
            {
                return;
            }

            Transform playerTransform = SceneContext.Instance.Player.transform;
            Vector3 cratePos = playerTransform.TransformPoint(Vector3.forward * 5 + new Vector3(0, 1.8f, 0));

            GameObject cratePrefab = GameContext.Instance.LookupDirector.GetPrefab(Identifiable.Id.CRATE_REEF_01);
            SRBehaviour.InstantiateActor(cratePrefab, GetPlayerModel().currRegionSetId, cratePos, Quaternion.identity); //TODO: orient in same direction player is facing (horizontally/yaw)?
        }

        internal static void SetPlayerEnergy(float energy)
        {
            PlayerModel playerModel = GetPlayerModel();
            playerModel.SetEnergy(energy);
            if(disableEnergyRecovery)
            {
                SetEnergyRecoverAfter(double.PositiveInfinity);
            }
            else
            {
                SetEnergyRecoverAfter(SceneContext.Instance.TimeDirector.WorldTime() + 300.0);
            }
        }

        internal static void SetEnergyRecoverAfter(double time)
        {
            GetPlayerModel().energyRecoverAfter = time;
        }

        internal static void SetFirestormsActive(bool active)
        {
            WorldModel worldModel = SceneContext.Instance.GameModel.GetWorldModel();

            // TODO: maybe better if this would be a patch of FirestormActivator (or the relevant class) that just checks if our setting is checked or not when a firestorm is meant to go off. worth it or no?
            if(active)
            {
                // Attempt to restore regular firestorm behavior
                // Set the same way as in FirestormActivator.MaybeStartFirestorm
                TimeDirector timeDirector = SceneContext.Instance.TimeDirector;
                worldModel.nextFirestormTime = timeDirector.HoursFromNow(Randoms.SHARED.GetInRange(8f, 15f));
            }
            else
            {
                // End firestorm if one is active
                worldModel.endFirestormTime = worldModel.worldTime;
                // Ensure the next firestorm will never happen
                // Also implemented in Patch_FirestormActivator to ensure the setting persists across loads
                worldModel.nextFirestormTime = double.PositiveInfinity;
                worldModel.nextFirecolumnTime = double.PositiveInfinity;
            }
        }

        // Returns whether or not the game is paused
        // Logic mostly copied from Pause method, may want to merge some functionality
        internal static bool IsGamePaused()
        {
            // TODO: Double check that this try-catch is necessary
            TimeDirector timeDirector = null;
            try
            {
                timeDirector = SceneContext.Instance.TimeDirector;
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

        internal static bool IsPauseMenuActive()
        {
            return PauseMenu.Instance.pauseUI.activeSelf;
        }

        internal static void ForceUnpause()
        {
            PauseMenu.Instance?.UnPauseGame();
        }
        #endregion

    }
}