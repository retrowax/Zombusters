using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZombustersWindows.Subsystem_Managers
{
    static class Resolution
    {
        static private GraphicsDeviceManager graphicsDeviceManager = null;

        static private int width = 800;
        static private int height = 600;
        static private int virtualWidth = 1280;
        static private int virtualHeight = 720;
        static private Matrix scaleMatrix;
        static private bool isFullScreen = false;
        static private bool isDirtyMatrix = true;

        static public void Init(ref GraphicsDeviceManager device)
        {
            width = device.PreferredBackBufferWidth;
            height = device.PreferredBackBufferHeight;
            graphicsDeviceManager = device;
            isDirtyMatrix = true;
#if !WINDOWS_UAP
            ApplyResolutionSettings();
#endif
        }


        static public Matrix getTransformationMatrix()
        {
            if (isDirtyMatrix) RecreateScaleMatrix();

            return scaleMatrix;
        }

        static public void SetResolution(int Width, int Height, bool FullScreen)
        {
            width = Width;
            height = Height;

            isFullScreen = FullScreen;

            ApplyResolutionSettings();
        }

        static public void SetVirtualResolution(int Width, int Height)
        {
            virtualWidth = Width;
            virtualHeight = Height;

            isDirtyMatrix = true;
        }

        static private void ApplyResolutionSettings()
        {

#if XBOX360
           isFullScreen = true;
#endif

            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (isFullScreen == false)
            {
                if ((width <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                    && (height <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height))
                {
                    graphicsDeviceManager.PreferredBackBufferWidth = width;
                    graphicsDeviceManager.PreferredBackBufferHeight = height;
                    graphicsDeviceManager.IsFullScreen = isFullScreen;
                    graphicsDeviceManager.ApplyChanges();
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate through the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if ((dm.Width == width) && (dm.Height == height))
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        graphicsDeviceManager.PreferredBackBufferWidth = width;
                        graphicsDeviceManager.PreferredBackBufferHeight = height;
                        graphicsDeviceManager.IsFullScreen = isFullScreen;
                        graphicsDeviceManager.ApplyChanges();
                    }
                }
            }

            isDirtyMatrix = true;

            width = graphicsDeviceManager.PreferredBackBufferWidth;
            height = graphicsDeviceManager.PreferredBackBufferHeight;
        }

        /// <summary>
        /// Sets the device to use the draw pump
        /// Sets correct aspect ratio
        /// </summary>
        static public void BeginDraw()
        {
            // Start by reseting viewport to (0,0,1,1)
            FullViewport();
            // Clear to Black
            graphicsDeviceManager.GraphicsDevice.Clear(Color.Black);
            // Calculate Proper Viewport according to Aspect Ratio
            ResetViewport();
            // and clear that
            // This way we are gonna have black bars if aspect ratio requires it and
            // the clear color on the rest
            graphicsDeviceManager.GraphicsDevice.Clear(Color.CornflowerBlue);
        }

        static private void RecreateScaleMatrix()
        {
            isDirtyMatrix = false;
            scaleMatrix = Matrix.CreateScale(
                           (float)graphicsDeviceManager.GraphicsDevice.Viewport.Width / virtualWidth,
                           (float)graphicsDeviceManager.GraphicsDevice.Viewport.Width / virtualWidth,
                           1f);
        }


        static public void FullViewport()
        {
            Viewport viewport = new Viewport();
            viewport.X = viewport.Y = 0;
            viewport.Width = width;
            viewport.Height = height;
            graphicsDeviceManager.GraphicsDevice.Viewport = viewport;
        }

        /// <summary>
        /// Get virtual Mode Aspect Ratio
        /// </summary>
        /// <returns>aspect ratio</returns>
        static public float GetVirtualAspectRatio()
        {
            return (float)virtualWidth / (float)virtualHeight;
        }

        static public void ResetViewport()
        {
            float targetAspectRatio = GetVirtualAspectRatio();
            // figure out the largest area that fits in this resolution at the desired aspect ratio
            int width = graphicsDeviceManager.PreferredBackBufferWidth;
            int height = (int)(width / targetAspectRatio + .5f);
            bool changed = false;

            if (height > graphicsDeviceManager.PreferredBackBufferHeight)
            {
                height = graphicsDeviceManager.PreferredBackBufferHeight;
                // PillarBox
                width = (int)(height * targetAspectRatio + .5f);
                changed = true;
            }

            // set up the new viewport centered in the backbuffer
            Viewport viewport = new Viewport
            {
                X = (graphicsDeviceManager.PreferredBackBufferWidth / 2) - (width / 2),
                Y = (graphicsDeviceManager.PreferredBackBufferHeight / 2) - (height / 2),
                Width = width,
                Height = height,
                MinDepth = 0,
                MaxDepth = 1
            };

            if (changed)
            {
                isDirtyMatrix = true;
            }

            graphicsDeviceManager.GraphicsDevice.Viewport = viewport;
        }

    }
}
