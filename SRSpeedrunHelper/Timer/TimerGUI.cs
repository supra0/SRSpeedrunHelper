using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SRSpeedrunHelper.Timer
{
    internal static class TimerGUI
    {
        private static GameTimer gameTimer;

        internal static void DoGUI()
        {
            // Timer settings
            gameTimer.showTimer = GUILayout.Toggle(gameTimer.showTimer, "Show timer");

            if (gameTimer.showTimer)
            {
                GUILayout.Label("Controls", SRSpeedrunHelper.LABEL_STYLE_BOLD);
                GUILayout.Label("Tip: Change keybinds in the UMF settings! (Shift+F10)", SRSpeedrunHelper.LABEL_STYLE_DEFAULT);
                if (GUILayout.Button("Start timer (Bind: " + SRSHConfig.bind_startTimer.ToString() + ")"))
                {
                    gameTimer?.StartTimer();
                }
                else if (GUILayout.Button("Pause timer (Bind: " + SRSHConfig.bind_pauseTimer.ToString() + ")"))
                {
                    gameTimer?.PauseTimer();
                }
                else if (GUILayout.Button("Reset timer (Bind: " + SRSHConfig.bind_resetTimer.ToString() + ")"))
                {
                    gameTimer?.ResetTimer();
                }

                GUILayout.Label("\nOptions", SRSpeedrunHelper.LABEL_STYLE_BOLD);
                gameTimer.showMilliseconds = GUILayout.Toggle(gameTimer.showMilliseconds, "Show milliseconds");
                gameTimer.pauseWhileLoading = GUILayout.Toggle(gameTimer.pauseWhileLoading, "Pause timer while the game is loading");
                gameTimer.pauseOnMainMenu = GUILayout.Toggle(gameTimer.pauseOnMainMenu, "Pause timer while on main menu");
                gameTimer.pauseWhileGamePaused = GUILayout.Toggle(gameTimer.pauseWhileGamePaused, "Pause timer while the game is paused");
            }
        }

        internal static void RegisterTimer(GameTimer gt)
        {
            gameTimer = gt;
        }
    }
}
