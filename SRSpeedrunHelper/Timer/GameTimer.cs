using System;
using UnityEngine;

namespace SRSpeedrunHelper.Timer
{
    class GameTimer : MonoBehaviour
    {
        
        public bool showTimer;
        // Options
        public bool showMilliseconds = true;
        public bool pauseWhileGamePaused;
        public bool pauseOnMainMenu;
        public bool pauseWhileLoading = true;

        private string displayString;
        private bool running;
        private double timePassed;

        // TODO: Add a way to change the timer style in options
        private GUIStyle timerStyle = new GUIStyle
        {
            fontSize = 32,
            wordWrap = false
        };
        internal static Color activeColor = Color.white; //Regular color
        internal static Color pausedColor = Color.gray; //Color when timer is paused
        internal static Color fasterTimeColor = Color.green; //Color when there's a new PB for the session
        internal static Color slowerTimeColor = Color.red; //Color when your time is slower than/has passed your PB for the session

        // Attempt to get the timer in the top-right corner, with some padding
        private static readonly float TIMER_WIDTH = 150;
        private static readonly float TIMER_HEIGHT = Screen.height / 12;
        // TODO: Let users move this
        private static readonly Rect timerRect = new Rect(Screen.width - TIMER_WIDTH - 25, 0 + 25, TIMER_WIDTH, TIMER_HEIGHT); // appear at top right of screen

        void Awake()
        {
            SetTimerColor(activeColor);
            UpdateDisplayString();
        }

        void Update()
        { 
            if(running)
            {
                // Check if timer update should be blocked
                if(pauseWhileLoading && WorldUtil.IsGameLoading() || pauseOnMainMenu && Levels.isMainMenu() || pauseWhileGamePaused && SRSpeedrunHelper.IsPauseMenuActive())
                {
                    return;
                }

                timePassed += Time.unscaledDeltaTime;
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
            if(showTimer)
            {
                running = true;
            }
        }

        public void PauseTimer()
        {
            if(showTimer)
            {
                running = false;
            }
        }

        // TODO: Stop timer should mean that this "run" has ended. Color should change and result should be saved(?)
        public void StopTimer()
        {
            if(showTimer)
            {
                running = false;
            }
        }

        public void ResetTimer()
        {
            running = false;
            timePassed = 0;
            UpdateDisplayString();
        }

        public void SetTimerColor(Color color)
        {
            timerStyle.normal.textColor = color;
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
