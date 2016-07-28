#region File Description
//-----------------------------------------------------------------------------
// IScreenFactory.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace GameStateManagement
{
    /// <summary>
    /// Defines an object that can create a screen when given its type.
    /// 
    /// The ScreenManager attempts to handle tombstoning on Windows Phone by creating an XML
    /// document that has a list of the screens currently in the manager. When the game is
    /// reactivated, the ScreenManager needs to create instances of those screens. However
    /// since there is no restriction that a particular GameScreen subclass has a parameterless
    /// constructor, there is no way the ScreenManager alone could create those instances.
    /// 
    /// IScreenFactory fills this gap by providing an interface the game should implement to
    /// act as a translation from type to instance. The ScreenManager locates the IScreenFactory
    /// from the Game.Services collection and passes each screen type to the factory, expecting
    /// to get the correct GameScreen out.
    /// 
    /// If your game screens all have parameterless constructors, the minimal implementation of
    /// this interface would look like this:
    /// 
    /// return Activator.CreateInstance(screenType) as GameScreen;
    /// 
    /// If you have screens with constructors that take arguments, you will need to ensure that
    /// you can read these arguments from storage or generate new ones, then construct the screen
    /// based on the type.
    /// 
    /// The ScreenFactory type in the sample game has the minimal implementation along with some
    /// extra comments showing a potentially more complex example of how to implement IScreenFactory.
    /// </summary>
    public interface IScreenFactory
    {
        /// <summary>
        /// Creates a GameScreen from the given type.
        /// </summary>
        /// <param name="screenType">The type of screen to create.</param>
        /// <returns>The newly created screen.</returns>
        GameScreen CreateScreen(Type screenType, ZombustersWindows.MyGame game);
    }

    public class ScreenFactory : IScreenFactory
    {
        public GameScreen CreateScreen(Type screenType, ZombustersWindows.MyGame game)
        {
            // All of our screens have empty constructors so we can just use Activator
            //return Activator.CreateInstance(screenType) as GameScreen;

            if (screenType == typeof(ZombustersWindows.LogoScreen))
            {
                ZombustersWindows.LogoScreen screen = new ZombustersWindows.LogoScreen(game);
                return screen;
            }
            else if (screenType == typeof(ZombustersWindows.StartScreen))
            {
                ZombustersWindows.StartScreen screen = new ZombustersWindows.StartScreen(game);
                return screen;
            }
            else if (screenType == typeof(ZombustersWindows.MenuScreen))
            {
                ZombustersWindows.MenuScreen screen = new ZombustersWindows.MenuScreen();
                return screen;
            }
            else if (screenType == typeof(ZombustersWindows.ExtrasMenuScreen))
            {
                ZombustersWindows.ExtrasMenuScreen screen = new ZombustersWindows.ExtrasMenuScreen();
                return screen;
            }
            else if (screenType == typeof(ZombustersWindows.HowToPlayScreen))
            {
                ZombustersWindows.HowToPlayScreen screen = new ZombustersWindows.HowToPlayScreen();
                return screen;
            }
            else if (screenType == typeof(ZombustersWindows.LeaderBoardScreen))
            {
                ZombustersWindows.LeaderBoardScreen screen = new ZombustersWindows.LeaderBoardScreen();
                return screen;
            }
            else if (screenType == typeof(ZombustersWindows.CreditsScreen))
            {
                ZombustersWindows.CreditsScreen screen = new ZombustersWindows.CreditsScreen(true);
                return screen;
            }
            else if (screenType == typeof(ZombustersWindows.SelectPlayerScreen))
            {
                ZombustersWindows.SelectPlayerScreen screen = new ZombustersWindows.SelectPlayerScreen(false);
                return screen;
            }
            else if (screenType == typeof(ZombustersWindows.GamePlayScreen))
            {
                //Zombusters.GamePlayScreen screen = new Zombusters.GamePlayScreen(game, Zombusters.Subsystem_Managers.CLevel.Level.One, Zombusters.Subsystem_Managers.CSubLevel.SubLevel.One);
                ZombustersWindows.MenuScreen screen = new ZombustersWindows.MenuScreen();
                return screen;
            }


            ZombustersWindows.LogoScreen logoScreen = new ZombustersWindows.LogoScreen(game);
            return logoScreen;
            

            //return Activator.CreateInstance(screenType) as GameScreen;

            // If we had more complex screens that had constructors or needed properties set,
            // we could do that before handing the screen back to the ScreenManager. For example
            // you might have something like this:
            //
            // if (screenType == typeof(MySuperGameScreen))
            // {
            //     bool value = GetFirstParameter();
            //     float value2 = GetSecondParameter();
            //     MySuperGameScreen screen = new MySuperGameScreen(value, value2);
            //     return screen;
            // }
            //
            // This lets you still take advantage of constructor arguments yet participate in the
            // serialization process of the screen manager. Of course you need to save out those
            // values when deactivating and read them back, but that means either IsolatedStorage or
            // using the PhoneApplicationService.Current.State dictionary.
        }
    }

}
