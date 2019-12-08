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
        GameScreen CreateScreen(Type screenType);
    }

    public class ScreenFactory : IScreenFactory
    {
        public GameScreen CreateScreen(Type screenType)
        {
            if (screenType == typeof(ZombustersWindows.LogoScreen))
            {
                ZombustersWindows.LogoScreen screen = new ZombustersWindows.LogoScreen();
                return screen;
            }
            else if (screenType == typeof(ZombustersWindows.StartScreen))
            {
                ZombustersWindows.StartScreen screen = new ZombustersWindows.StartScreen();
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
                ZombustersWindows.MenuScreen screen = new ZombustersWindows.MenuScreen();
                return screen;
            }

            ZombustersWindows.LogoScreen logoScreen = new ZombustersWindows.LogoScreen();
            return logoScreen;
        }
    }
}
