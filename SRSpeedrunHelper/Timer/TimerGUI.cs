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
                if (GUILayout.Button("Start timer"))
                {
                    gameTimer?.StartTimer();
                }
                else if (GUILayout.Button("Stop timer"))
                {
                    gameTimer?.StopTimer();
                }
                else if (GUILayout.Button("Reset timer"))
                {
                    gameTimer?.ResetTimer();
                }
                gameTimer.showMilliseconds = GUILayout.Toggle(gameTimer.showMilliseconds, "Show milliseconds");
            }
        }

        internal static void RegisterTimer(GameTimer gt)
        {
            gameTimer = gt;
        }
    }
}
