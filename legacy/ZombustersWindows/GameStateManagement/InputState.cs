#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
#endregion

namespace GameStateManagement
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
		# region private class data
        private readonly KeyboardState[] currentKeyboardStates;
        private readonly GamePadState[] currentGamePadStates;
        private readonly KeyboardState[] lastKeyboardStates;
        private readonly GamePadState[] lastGamePadStates;
        private MouseState currentMouseStates;
        private MouseState lastMouseStates;
        private List<GestureSample> gesturesList;
		#endregion

        #region Fields
        public const int MaxInputs = 4;
        public TouchCollection TouchState;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            currentKeyboardStates = new KeyboardState[MaxInputs];
            currentGamePadStates = new GamePadState[MaxInputs];
            lastKeyboardStates = new KeyboardState[MaxInputs];
            lastGamePadStates = new GamePadState[MaxInputs];
            currentMouseStates = new MouseState();
            lastMouseStates = new MouseState();
            gesturesList = new List<GestureSample>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Checks for a "menu up" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuUp
        {
            get
            {
                return IsNewKeyPress(Keys.Up) ||
                       IsNewButtonPress(Buttons.DPadUp) ||
                       IsNewButtonPress(Buttons.LeftThumbstickUp);
            }
        }


        /// <summary>
        /// Checks for a "menu down" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuDown
        {
            get
            {
                return IsNewKeyPress(Keys.Down) ||
                       IsNewButtonPress(Buttons.DPadDown) ||
                       IsNewButtonPress(Buttons.LeftThumbstickDown);
            }
        }


        /// <summary>
        /// Checks for a "menu select" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuSelect
        {
            get
            {
                return IsNewKeyPress(Keys.Space) ||
                       IsNewKeyPress(Keys.Enter) ||
                       IsNewButtonPress(Buttons.A) ||
                       IsNewButtonPress(Buttons.Start);
            }
        }


        /// <summary>
        /// Checks for a "menu cancel" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuCancel
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       IsNewButtonPress(Buttons.B) ||
                       IsNewButtonPress(Buttons.Back);
            }
        }


        /// <summary>
        /// Checks for a "pause the game" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool PauseGame
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       IsNewButtonPress(Buttons.Back) ||
                       IsNewButtonPress(Buttons.Start);
            }
        }
        #endregion

        #region Methods
        public KeyboardState[] GetCurrentKeyboardStates()
		{
			return currentKeyboardStates;
		}

        public GamePadState[] GetCurrentGamePadStates()
		{
			return currentGamePadStates;
		}

        public KeyboardState[] GetLastKeyboardStates()
		{
			return lastKeyboardStates;
		}

        public GamePadState[] GetLastGamePadStates()
		{
			return lastGamePadStates;
		}

        public MouseState GetCurrentMouseState()
        {
            return currentMouseStates;
        }

        public MouseState GetLastMouseState()
        {
            return lastMouseStates;
        }

        public List<GestureSample> GetGestures()
        {
            return gesturesList;
        }
		
        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                lastKeyboardStates[i] = currentKeyboardStates[i];
                lastGamePadStates[i] = currentGamePadStates[i];

                currentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
                currentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);  
            }

            lastMouseStates = currentMouseStates;
            currentMouseStates = Mouse.GetState();

            // Get the raw touch state from the TouchPanel
            TouchState = TouchPanel.GetState();

            // Read in any detected gestures into our list for the screens to later process
            gesturesList.Clear();
            if (TouchPanel.EnabledGestures != GestureType.None)
            {
                while (TouchPanel.IsGestureAvailable)
                {
                    gesturesList.Add(TouchPanel.ReadGesture());
                }
            }
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsNewKeyPress(Keys key)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsNewKeyPress(key, (PlayerIndex)i))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsNewKeyPress(Keys key, PlayerIndex playerIndex)
        {
            return (currentKeyboardStates[(int)playerIndex].IsKeyDown(key) &&
                    lastKeyboardStates[(int)playerIndex].IsKeyUp(key));
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update,
        /// by any player.
        /// </summary>
        public bool IsNewButtonPress(Buttons button)
        {
            for (int i = 0; i < MaxInputs; i++)
            {
                if (IsNewButtonPress(button, (PlayerIndex)i))
				{
                    return true;
				}
            }
            return false;
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update,
        /// by the specified player.
        /// </summary>
        public bool IsNewButtonPress(Buttons button, PlayerIndex playerIndex)
        {            
            return (currentGamePadStates[(int)playerIndex].IsButtonDown(button) &&
                    lastGamePadStates[(int)playerIndex].IsButtonUp(button));
        }


        /// <summary>
        /// Checks for a "menu select" input action from the specified player.
        /// </summary>
        public bool IsMenuSelect(PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Space, playerIndex) ||
                   IsNewKeyPress(Keys.Enter, playerIndex) ||
                   IsNewButtonPress(Buttons.A, playerIndex) ||
                   IsNewButtonPress(Buttons.Start, playerIndex);
        }


        /// <summary>
        /// Checks for a "menu cancel" input action from the specified player.
        /// </summary>
        public bool IsMenuCancel(PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Escape, playerIndex) ||
                   IsNewKeyPress(Keys.Back, playerIndex) ||
                   IsNewButtonPress(Buttons.B, playerIndex) ||
                   IsNewButtonPress(Buttons.Back, playerIndex);
        }
        #endregion
    }
}
