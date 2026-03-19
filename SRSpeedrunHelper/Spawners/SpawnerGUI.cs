using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SRSpeedrunHelper.Spawners
{
    internal static class SpawnerGUI
    {
        // TODO: same as main window, consider dynamic/configurable window size for high resolutions
        private static readonly int spawnerWindowWidth = 300;
        private static readonly int spawnerWindowHeight = 450;
        private static readonly string spawnerWindowTitle = "Spawner Info";
        private static readonly int spawnerWindowId = 33734;

        private static Rect spawnerWindowRect = new Rect(Screen.width - spawnerWindowWidth, Screen.height - spawnerWindowHeight, spawnerWindowWidth, spawnerWindowHeight); // Bottom-right corner

        internal static bool showSpawners = false;

        public static bool spawnerShowTriggerRate = true;
        public static bool spawnerShowAvgNextSpawn = true;
        public static bool spawnerShowNextSpawnTime = true;
        public static bool spawnerShowCountRange = true;
        public static bool spawnerConvertToPercentage = true;

        internal static void DoGUI()
        {
            bool newShowSpawners = GUILayout.Toggle(showSpawners, "Show slime spawners");
            if (newShowSpawners != showSpawners)
            {
                if (newShowSpawners)
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
        }
        internal static void DoSpawnerInfoGUI()
        {
            spawnerWindowRect = GUILayout.Window(spawnerWindowId, spawnerWindowRect, SpawnerGUI.ShowSpawnerMenu, spawnerWindowTitle);
        }

        private static void ShowSpawnerMenu(int winId)
        {
            if (winId != spawnerWindowId)
            {
                SRSpeedrunHelper.Log("Warning: Wrong window ID passed to ShowSpawnerMenu.");
                return;
            }

            GUILayout.Label(SRSpeedrunHelper.targetSpawner.GetInfoText(), SRSpeedrunHelper.LABEL_STYLE_DEFAULT);
        }
    }
}
