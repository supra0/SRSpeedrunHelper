using System;
using UnityEngine;

namespace SRSpeedrunHelper
{
    class GameTimer : MonoBehaviour
    {
        public bool showMilliseconds;
        public bool showTimer;

        private string displayString;
        private bool running;
        private double timePassed;

        private GUIStyle timerStyle;
        private static readonly Color activeColor = Color.white;
        private static readonly Color pausedColor = Color.gray;

        // Attempt to get the timer in the top-right corner, with some padding
        private static readonly float TIMER_WIDTH = 150;
        private static readonly float TIMER_HEIGHT = Screen.height / 12;
        private static readonly Rect timerRect = new Rect(Screen.width - TIMER_WIDTH - 25, 0 + 25, TIMER_WIDTH, TIMER_HEIGHT); // appear at top right of screen

        void Awake()
        {
            timerStyle = new GUIStyle();
            timerStyle.fontSize = 32;
            timerStyle.wordWrap = false;
            timerStyle.normal.textColor = activeColor;

            UpdateDisplayString();
        }

        void Update()
        { 
            if(running)
            {
                if(Levels.isMainMenu() || Levels.isSpecial())
                {
                    showTimer = false;
                    ResetTimer();
                }

                timePassed += Time.deltaTime;
                UpdateDisplayString();
            }
        }

        void OnGUI()
        {
            if(showTimer)
            {
                GUI.Label(timerRect, displayString, timerStyle);
            }
        }

        public void StartTimer()
        {
            running = true;
        }

        public void StopTimer()
        {
            running = false;
        }

        public void ResetTimer()
        {
            running = false;
            timePassed = 0;
            UpdateDisplayString();
        }

        private void UpdateDisplayString()
        {
            // Going to assume nobody will need it to go into hours
            // Format: MM:SS[.mmm]
            int numSeconds = (int)Math.Floor(timePassed);
            int minutes = numSeconds / 60;
            int currSecond = numSeconds % 60;

            displayString = string.Format("{0:00}:{1:00}", minutes, currSecond);

            if (showMilliseconds)
            {
                displayString += "." + string.Format("{0:.000}", timePassed).Split('.')[1];
            }
        }
    }
}
