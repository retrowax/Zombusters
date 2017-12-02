using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using GameStateManagement;
using ZombustersWindows.Localization;

namespace ZombustersWindows
{
    public struct ControllerEvent
    {
        public int finalFrame;
        public PlayerIndex player;
        public float leftMotor;
        public float rightMotor;
    }

    public class InputManager : GameComponent
    {
        ControllerDisconnectScreen cdScreen1, cdScreen2, cdScreen3, cdScreen4;
        MyGame game;
        InputState input;

        public InputManager(MyGame game)
            : base(game)
        {
            cdScreen1 = null;
            cdScreen2 = null;
            cdScreen3 = null;
            cdScreen4 = null;
            this.game = game;
            Vibrations = new List<ControllerEvent>();
        }

        public override void Update(GameTime gameTime)
        {

#if XBOX
            // Detect disconnected controller
            if ((!GamePad.GetState(game.Main.Controller).IsConnected) &&
                (cdScreen1 == null) && game.Main.IsPlaying && game.currentPlayers[0].status == ObjectStatus.Active)
            {
                // Display Screen to wait for reconnect
                cdScreen1 = new ControllerDisconnectScreen(game.Main.Controller);
                game.screenManager.AddScreen(cdScreen1);

                if (game.GamePlayStatus == GameplayState.Playing)
                {
                    bPaused = game.BeginPause();
                    game.GamePlayStatus = GameplayState.Pause;
                }
            }
            else if ((!GamePad.GetState(game.Player2.Controller).IsConnected) &&
                (cdScreen2 == null) && game.currentPlayers[1].status == ObjectStatus.Active)
            {
                // Display Screen to wait for reconnect
                cdScreen2 = new ControllerDisconnectScreen(game.Player2.Controller);
                game.screenManager.AddScreen(cdScreen2);

                if (game.GamePlayStatus == GameplayState.Playing)
                {
                    bPaused = game.BeginPause();
                    game.GamePlayStatus = GameplayState.Pause;
                }
            }
            else if ((!GamePad.GetState(game.Player3.Controller).IsConnected) &&
                (cdScreen3 == null) && game.currentPlayers[2].status == ObjectStatus.Active)
            {
                // Display Screen to wait for reconnect
                cdScreen3 = new ControllerDisconnectScreen(game.Player3.Controller);
                game.screenManager.AddScreen(cdScreen3);

                if (game.GamePlayStatus == GameplayState.Playing)
                {
                    bPaused = game.BeginPause();
                    game.GamePlayStatus = GameplayState.Pause;
                }
            }
            else if ((!GamePad.GetState(game.Player4.Controller).IsConnected) &&
                (cdScreen4 == null) && game.currentPlayers[3].status == ObjectStatus.Active)
            {
                // Display Screen to wait for reconnect
                cdScreen4 = new ControllerDisconnectScreen(game.Player4.Controller);
                game.screenManager.AddScreen(cdScreen4);

                if (game.GamePlayStatus == GameplayState.Playing)
                {
                    bPaused = game.BeginPause();
                    game.GamePlayStatus = GameplayState.Pause;
                }
            }
            else
            {
                if ((GamePad.GetState(game.Main.Controller).IsConnected) && (cdScreen1 != null))
                {
                    cdScreen1 = null;
                }

                if ((GamePad.GetState(game.Player2.Controller).IsConnected) && (cdScreen2 != null))
                {
                    cdScreen2 = null;
                }

                if ((GamePad.GetState(game.Player3.Controller).IsConnected) && (cdScreen3 != null))
                {
                    cdScreen3 = null;
                }

                if ((GamePad.GetState(game.Player4.Controller).IsConnected) && (cdScreen4 != null))
                {
                    cdScreen4 = null;
                }

                // Detect whether a guest has signed in
                PlayerIndex guest;
                if (CheckNewPlayersStart(game.Main.Controller, out guest))
                {
                    game.InitializePlayers(guest);
                }
                if (!bPaused)
                {
                    currentFrame++;

                    Vector2 pOne = Vector2.Zero;
                    Vector2 pTwo = Vector2.Zero;
                    Vector2 pThree = Vector2.Zero;
                    Vector2 pFour = Vector2.Zero;
                    // Clear the list of expired events;
                    for (int i = 0; i < Vibrations.Count; i++)
                    {
                        // Add up all the vibrations for each player
                        if (Vibrations[i].finalFrame < currentFrame)
                            Vibrations.RemoveAt(i);
                        else
                        {
                            switch (Vibrations[i].player)
                            {
                                case PlayerIndex.One:
                                    pOne.X += Vibrations[i].leftMotor;
                                    pOne.Y += Vibrations[i].rightMotor;
                                    break;
                                case PlayerIndex.Two:
                                    pTwo.X += Vibrations[i].leftMotor;
                                    pTwo.Y += Vibrations[i].rightMotor;
                                    break;
                                case PlayerIndex.Three:
                                    pThree.X += Vibrations[i].leftMotor;
                                    pThree.Y += Vibrations[i].rightMotor;
                                    break;
                                case PlayerIndex.Four:
                                    pFour.X += Vibrations[i].leftMotor;
                                    pFour.Y += Vibrations[i].rightMotor;
                                    break;
                            }
                        }
                        // Play the vibrations for each player
                        if (game.GamePlayStatus == GameplayState.Playing)
                        {
                            GamePad.SetVibration(PlayerIndex.One, pOne.X, pOne.Y);
                            GamePad.SetVibration(PlayerIndex.Two, pTwo.X, pTwo.Y);
                            GamePad.SetVibration(PlayerIndex.Three, pThree.X, pThree.Y);
                            GamePad.SetVibration(PlayerIndex.Four, pFour.X, pFour.Y);
                        }
                        else
                        {
                            if (GamePad.GetState(game.Main.Controller).IsConnected)
                            {
                                GamePad.SetVibration(game.Main.Controller, 0, 0);
                            }
                            if (GamePad.GetState(game.Player2.Controller).IsConnected)
                            {
                                GamePad.SetVibration(game.Player2.Controller, 0, 0);
                            }
                            if (GamePad.GetState(game.Player3.Controller).IsConnected)
                            {
                                GamePad.SetVibration(game.Player3.Controller, 0, 0);
                            }
                            if (GamePad.GetState(game.Player4.Controller).IsConnected)
                            {
                                GamePad.SetVibration(game.Player4.Controller, 0, 0);
                            }
                        }
                    }
                }
            }
#endif
            // update our virtual thumbsticks
            VirtualThumbsticks.Update();


            base.Update(gameTime);
        }

        private int currentFrame = 0;
        private List<ControllerEvent> Vibrations;
        /// <summary>
        /// Add more vibration to a controller
        /// </summary>
        /// <param name="player">The controller to vibrate.</param>
        /// <param name="frames">The number of frames to vibrate.</param>
        /// <param name="leftMotor">The amount of vibration to apply to the left motor.</param>
        /// <param name="rightMotor">The amount of vibration to apply to the right motor.</param>
        private void AddVibration(PlayerIndex player, int frames, 
            float leftMotor, float rightMotor)
        {
            ControllerEvent vibe;
            vibe.finalFrame = currentFrame+frames;
            vibe.leftMotor = leftMotor;
            vibe.rightMotor = rightMotor;
            vibe.player = player;
            Vibrations.Add(vibe);
        }
        private bool bPaused = false;
        /// <summary>
        /// Pauses all controller vibration.
        /// </summary>
        public void BeginPause()
        {
            bPaused = true;
            //if (GamePad.GetState(game.Main.Controller).IsConnected)
            //{
            //    GamePad.SetVibration(game.Main.Controller, 0, 0);
            //}
            //if (GamePad.GetState(game.Player2.Controller).IsConnected)
            //{
            //    GamePad.SetVibration(game.Player2.Controller, 0, 0);
            //}
            //if (GamePad.GetState(game.Player3.Controller).IsConnected)
            //{
            //    GamePad.SetVibration(game.Player3.Controller, 0, 0);
            //}
            //if (GamePad.GetState(game.Player4.Controller).IsConnected)
            //{
            //    GamePad.SetVibration(game.Player4.Controller, 0, 0);
            //}
        }
        /// <summary>
        /// Resumes controller vibration (if any).
        /// </summary>
        public void EndPause()
        {
            bPaused = false;
        }

        public void PlayShot(byte player)
        {
            // which controller?
            PlayerIndex index = game.currentPlayers[player].Player.Controller;

            // buzz it
            AddVibration(index, 8, 0.2f, 0.4f);
        }

        /// <summary>
        /// This method checks to see if any GamePads have pressed start.
        /// </summary>
        /// <param name="playerOne">The index of player one.</param>
        /// <returns>True if a player pressed Start, false otherwise.</returns>
        public static bool CheckPlayerOneStart(out PlayerIndex playerOne, InputState input)
        {
            playerOne = PlayerIndex.One;

            if (Keyboard.GetState().GetPressedKeys().Length > 0)
            {
                playerOne = PlayerIndex.One;
                return true;
            }

            // Read in our gestures
            foreach (GestureSample gesture in input.GetGestures())
            {
                // If we have a tap
                if (gesture.GestureType == GestureType.Tap)
                {
                    playerOne = PlayerIndex.One;
                    return true;
                }
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed)
            {
                playerOne = PlayerIndex.One;
                return true;
            }
            if (GamePad.GetState(PlayerIndex.Two).Buttons.Start == ButtonState.Pressed)
            {
                playerOne = PlayerIndex.Two;
                return true;
            }
            if (GamePad.GetState(PlayerIndex.Three).Buttons.Start == ButtonState.Pressed)
            {
                playerOne = PlayerIndex.Three;
                return true;
            }
            if (GamePad.GetState(PlayerIndex.Four).Buttons.Start == ButtonState.Pressed)
            {
                playerOne = PlayerIndex.Four;
                return true;
            }
            return false;
        }

        /// <summary>
        /// This method checks to see if any GamePads other than Player One have pressed start.
        /// </summary>
        /// <param name="playerOne">The index of player one.</param>
        /// <param name="playerTwo">The returned index of the lowest player who has pressed start.</param>
        /// <returns>True if player two pressed start, otherwise false.</returns>
        public static bool CheckNewPlayersStart(PlayerIndex playerOne, 
            out PlayerIndex playerTwo)
        {
            playerTwo = playerOne;
            if (IsSupported(
                GamePad.GetCapabilities(PlayerIndex.One).GamePadType) &&
                (GamePad.GetState(PlayerIndex.One).Buttons.Start 
                == ButtonState.Pressed) &&
                (playerOne != PlayerIndex.One))
            {
                playerTwo = PlayerIndex.One;
                return true;
            }
            if (IsSupported(
                GamePad.GetCapabilities(PlayerIndex.Two).GamePadType) &&
                (GamePad.GetState(PlayerIndex.Two).Buttons.Start 
                == ButtonState.Pressed) &&
                (playerOne != PlayerIndex.Two))
            {
                playerTwo = PlayerIndex.Two;
                return true;
            }
            if (IsSupported(
                GamePad.GetCapabilities(PlayerIndex.Three).GamePadType) &&
                (GamePad.GetState(PlayerIndex.Three).Buttons.Start 
                == ButtonState.Pressed) &&
                (playerOne != PlayerIndex.Three))
            {
                playerTwo = PlayerIndex.Three;
                return true;
            }
            if (IsSupported(
                GamePad.GetCapabilities(PlayerIndex.Four).GamePadType) &&
                (GamePad.GetState(PlayerIndex.Four).Buttons.Start 
                == ButtonState.Pressed) &&
                (playerOne != PlayerIndex.Four))
            {
                playerTwo = PlayerIndex.Four;
                return true;
            }
            return false;
        }




        /// <summary>
        /// This method takes a raw thumbstick value and returns values
        /// within the largest square inside the thumbstick range.  
        /// This gives diagonal movement the
        /// same maximum value as horizontal or vertical movement.
        /// </summary>
        /// <param name="stick">The raw thumbstick value.</param>
        /// <returns>The thumbstick value clamped to a square.</returns>
        public static Vector2 ClampStick(Vector2 stick)
        {
            // Draw a square inside the circle, and ignore anything beyond the square.
            // Then, divide by the size of the square to get 0-1 values.
            stick.X = (float)MathHelper.Clamp(stick.X, -.707f, .707f) / .707f;
            stick.Y = (float)MathHelper.Clamp(stick.Y, -.707f, .707f) / .707f;

            return stick;
        }

        /// <summary>
        /// This method takes a raw thumbstick value and returns a value
        /// expanded from a circle (the normal thumbstick range) into
        /// a square containing that circle.  
        /// This gives diagonal movement the same maximum value as horizontal
        /// and vertical movements, and provides an alternative to the
        /// ClampStick method.
        /// </summary>
        /// <param name="stick"></param>
        /// <returns></returns>
        public static Vector2 Circle2Square(Vector2 stick)
        {
            // Project a point on a circle onto the square that bounds the circle.
            Vector2 abs = new Vector2(Math.Abs(stick.X), Math.Abs(stick.Y));
            double theta = abs.X > abs.Y ? Math.Atan2(abs.Y, abs.X) : 
                Math.Atan2(abs.X, abs.Y);
            stick.X /= (float)Math.Cos(theta);
            stick.Y /= (float)Math.Cos(theta);
            return stick;
        }        
        
        /// <summary>
        /// Rather than reading input as linear between 0 and 1.0, we can apply
        /// a gentle curve so that values near 1.0 (the edge) are more distinct
        /// than values near the center.  This makes the input feel a little 
        /// more responsive.
        /// </summary>
        /// <param name="input">The raw thumbstick input.</param>
        /// <param name="curve">The power of the curve to apply.</param>
        /// <returns>The adjusted thumbstick input.</returns>
        public static Vector2 CurveInput(Vector2 input, float curve)
        {
            // Apply power curve to input
            Vector2 magnitude;
            magnitude.X = (float)Math.Pow(input.X, curve);
            magnitude.Y = (float)Math.Pow(input.Y, curve);

            // Use the new magnitude, preserve the original sign
            input.X = magnitude.X * Math.Sign(input.X);
            input.Y = magnitude.Y * Math.Sign(input.Y);

            return input;
        }

        /// <summary>
        /// This function determines if a controller is a type we support.
        /// We want to ignore any drums, guitars, etc. that might be
        /// plugged in.
        /// </summary>
        /// <param name="type">The type of controller being tested</param>
        /// <returns>true if the controller is supported by this game.</returns>
        private static bool IsSupported(GamePadType type)
        {
            return ((type == GamePadType.GamePad) ||
                    (type == GamePadType.ArcadeStick) ||
                    (type == GamePadType.FlightStick));
        }        
    }

    /// <summary>
    /// This screen is displayed when the main controller is unplugged, or when no
    /// controller is found at all.
    /// </summary>
    public class ControllerDisconnectScreen : ErrorScreen
    {
        PlayerIndex player;        
        public ControllerDisconnectScreen(PlayerIndex player)
            : base(Strings.ReconnectControllerString)
        {
            this.player = player;
        }
        public override void HandleInput(InputState input)
        {
            if (input.GetCurrentGamePadStates()[(int)player].IsConnected)
            {
                ExitScreen();
            }
            base.HandleInput(input);
        }

    }
}
