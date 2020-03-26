using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ZombustersWindows.Subsystem_Managers;

namespace ZombustersWindows
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class MusicComponent : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Private fields
        private readonly ContentManager contentManager;
        private readonly Dictionary<string, Song> songsDictionary = new Dictionary<string, Song>();
        private readonly List<String> songsList = new List<string>();
        private Song currentSong = null;
        private Boolean isMusicPaused = new Boolean();
        private int nextSong = 0;
        private int prevSong = 0;
        private Boolean isShowingInfo = new Boolean();
        private SpriteFont font;
        private SpriteFont fontItalic;
        private SpriteFont fontSmallItalic;
        private Rectangle uiBounds;
        private Vector2 drawPosition;
        private Texture2D retroTraxLogoTexture;
        private Texture2D retroTraxBkgTexture;
        private Boolean buttonReleased = true;
        private Boolean changinSongManual = false;
        private readonly Random random = new Random(12);

        private bool FadeOut = false;
        private float logoSizeValue = 0.0f;
        private float logoSizeSpeed = 3.0f;
        private float timer;
        private bool ShowText = false;

        #endregion

        /// <summary>
        /// Gets the name of the currently playing song, or null if no song is playing.
        /// </summary>
        public string CurrentSong { get; private set; }

        /// <summary>
        /// Gets or sets the volume to play songs. 1.0f is max volume.
        /// </summary>
        public float MusicVolume
        {
            get { return MediaPlayer.Volume; }
            set { MediaPlayer.Volume = value; }
        }

        /// <summary>
        /// Gets whether a song is playing or paused (i.e. not stopped).
        /// </summary>
        public bool IsSongActive { get { return currentSong != null && MediaPlayer.State != MediaState.Stopped; } }

        /// <summary>
        /// Gets whether the current song is paused.
        /// </summary>
        public bool IsSongPaused { get { return currentSong != null && isMusicPaused; } }

        public MusicComponent(Game game)
            : base(game)
        {
            contentManager = ((MyGame)this.Game).Content;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            isMusicPaused = false;
            isShowingInfo = false;
            uiBounds = GetTitleSafeArea();
            drawPosition = new Vector2(uiBounds.X + 10,uiBounds.Bottom - 90);

            songsList.Add("Dancing on a Dime|Bare Wires|Live at WFMU on the Evan Funk Davies show");
            songsList.Add("High Dive|Black Math|Phantom Power");
            songsList.Add("Suck City|Black Math|Phantom Power");
            songsList.Add("Bad Attraction|Brad Sucks|I Don't Know What I'm Doing");
            songsList.Add("Understood by your Dad|Brad Sucks|Out of It");
            songsList.Add("Wolfram|Kraus|I Could Destroy You with a Single Thought");
            songsList.Add("High Ground|London To Tokyo|London To Tokyo");
            songsList.Add("Opening The Portal|Nuit Noire|split 7''");
            songsList.Add("I Don't Like You|The Black Bug|I Don't Like You 7''");
            songsList.Add("In The Hall Of The Mountain King|The Itchy Creeps|The Itchy Creeps");
            songsList.Add("As You Know|THIS CO.|THIS CO.");
            songsList.Add("Take It Away|THIS CO.|THIS CO.");

            nextSong = random.Next(0, songsList.Count - 1);
            prevSong = nextSong;
            MediaPlayer.MediaStateChanged += new EventHandler<EventArgs>(MediaPlayer_MediaStateChanged);

            base.Initialize();
        }


        protected override void LoadContent()
        {
            retroTraxBkgTexture = contentManager.Load<Texture2D>(@"InGame/GUI/retrotrax_anim_bkg");
            retroTraxLogoTexture = contentManager.Load<Texture2D>(@"InGame/GUI/retrotrax_anim");

            LoadSong("Dancing on a Dime", @"Music/BareWires_DancingOnADime");
            LoadSong("High Dive", @"Music/BlackMath_HighDive");
            LoadSong("Suck City", @"Music/BlackMath_SuckCity");
            LoadSong("Bad Attraction", @"Music/BradSucks_BadAttraction");
            LoadSong("Understood by your Dad", @"Music/BradSucks_UnderstoodByYourDad");
            LoadSong("Wolfram", @"Music/Kraus_Wolfram");
            LoadSong("High Ground", @"Music/LondonToTokyo_HighGround");
            LoadSong("Opening The Portal", @"Music/NuitNoire_OpeningThePortal");
            LoadSong("I Don't Like You", @"Music/TheBlackBug_IDontLikeYou");
            LoadSong("In The Hall Of The Mountain King", @"Music/TheItchyCreeps_ITHOTM");
            LoadSong("As You Know", @"Music/ThisCo_AsYouKnow");
            LoadSong("Take It Away", @"Music/ThisCo_TakeItAway");

            font = contentManager.Load<SpriteFont>(@"menu\ArialMenuInfo");
            fontItalic = contentManager.Load<SpriteFont>(@"menu\ArialMusic");
            fontSmallItalic = contentManager.Load<SpriteFont>(@"menu\ArialMusicItalic");

            base.LoadContent();
        }


        public void StartPlayingMusic(int songNum)
        {
            if (MediaPlayer.GameHasControl)
            {
                // Start playing the first song
                if (songNum != -1)
                {
                    nextSong = songNum;
                }

                PlaySong(songsList[nextSong], false);
            }
        }


        /// <summary>
        /// Loads a Song into the AudioManager.
        /// </summary>
        /// <param name="songName">Name of the song to load</param>
        public void LoadSong(string songName)
        {
            LoadSong(songName, songName);
        }

        /// <summary>
        /// Loads a Song into the AudioManager.
        /// </summary>
        /// <param name="songName">Name of the song to load</param>
        /// <param name="songPath">Path to the song asset file</param>
        public void LoadSong(string songName, string songPath)
        {
            if (songsDictionary.ContainsKey(songName))
            {
                throw new InvalidOperationException(string.Format("Song '{0}' has already been loaded", songName));
            }

            songsDictionary.Add(songName, contentManager.Load<Song>(songPath));
        }

        /// <summary>
        /// Starts playing the song with the given name. If it is already playing, this method
        /// does nothing. If another song is currently playing, it is stopped first.
        /// </summary>
        /// <param name="songName">Name of the song to play</param>
        public void PlaySong(string song)
        {
            if (MediaPlayer.GameHasControl)
            {
                string songName = song.Split('|')[0];

                PlaySong(songName, false);
            }
        }

        /// <summary>
        /// Starts playing the song with the given name. If it is already playing, this method
        /// does nothing. If another song is currently playing, it is stopped first.
        /// </summary>
        /// <param name="songName">Name of the song to play</param>
        /// <param name="loop">True if song should loop, false otherwise</param>
        public void PlaySong(string song, bool loop)
        {
            string songName = song.Split('|')[0];

            if (CurrentSong != songName)
            {
                if (currentSong != null)
                {
                    MediaPlayer.Stop();             
                }

                if (!songsDictionary.TryGetValue(songName, out currentSong))
                {
                    throw new ArgumentException(string.Format("Song '{0}' not found", songName));
                }

                CurrentSong = songName;

                isMusicPaused = false;
                MediaPlayer.IsRepeating = loop;

                if (currentSong != null)
                {
                    MediaPlayer.Play(currentSong);
                    isShowingInfo = true;
                }
                else
                {
                    PlayNextSong();
                }
 
                if (!Enabled)
                {
                    MediaPlayer.Pause();
                }
            }
        }

        /// <summary>
        /// Pauses the currently playing song. This is a no-op if the song is already paused,
        /// or if no song is currently playing.
        /// </summary>
        public void PauseSong()
        {
            if (currentSong != null && !isMusicPaused)
            {
                if (Enabled) MediaPlayer.Pause();
                isMusicPaused = true;
            }
        }

        /// <summary>
        /// Resumes the currently paused song. This is a no-op if the song is not paused,
        /// or if no song is currently playing.
        /// </summary>
        public void ResumeSong()
        {
            if (currentSong != null && isMusicPaused)
            {
                if (Enabled) MediaPlayer.Resume();
                isMusicPaused = false;
            }
        }

        /// <summary>
        /// Stops the currently playing song. This is a no-op if no song is currently playing.
        /// </summary>
        public void StopSong()
        {
            if (currentSong != null && MediaPlayer.State != MediaState.Stopped)
            {
                MediaPlayer.Stop();
                isMusicPaused = false;
                isShowingInfo = false;
            }
        }

        private void PlayRandomSong()
        {
            do
            {
                nextSong = random.Next(0, songsList.Count - 1);
            } while (nextSong == prevSong);

            PlaySong(songsList[nextSong], false);
            prevSong = nextSong;
        }

        private void PlayNextSong()
        {
            nextSong += 1;

            if (nextSong > songsList.Count - 1)
            {
                nextSong = 0;
            }

            changinSongManual = true;
            PlaySong(songsList[nextSong], false);
        }

        public void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
        {
            if (MediaPlayer.State == MediaState.Stopped && changinSongManual == false)
            {
                this.PlayRandomSong();
            }

            if (changinSongManual == true)
            {
                changinSongManual = false;
            }
        }

        private void UpdateLogoAnimation(GameTime gameTime)
        {
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (this.logoSizeValue < 1.0f && !FadeOut)
            {
                this.logoSizeValue = this.logoSizeValue + (timeDelta * this.logoSizeSpeed);
            }
            else
            {
                if (!FadeOut)
                {
                    ShowText = true;
                    this.logoSizeValue = 1.0f;

                    //Esperamos un par de segundos antes de hacer Fade Out del Logo.
                    timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (timer >= 5.0f)
                    {
                        FadeOut = true;
                        ShowText = false;
                    }
                }
                else
                    this.logoSizeValue = this.logoSizeValue - (timeDelta * this.logoSizeSpeed);
            }

            if (FadeOut && this.logoSizeValue < 0)
            {
                FadeOut = false;
                this.logoSizeValue = 0.0f;
                isShowingInfo = false;
                timer = 0;
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (MediaPlayer.GameHasControl)
            {
                if (currentSong != null && MediaPlayer.State == MediaState.Stopped)
                {
                    currentSong = null;
                    CurrentSong = null;
                    isMusicPaused = false;
                }

                // If the user change the song...
                if (((GamePad.GetState(((MyGame)this.Game).player1.Controller).Buttons.LeftShoulder == ButtonState.Pressed) || (GamePad.GetState(((MyGame)this.Game).player2.Controller).Buttons.LeftShoulder == ButtonState.Pressed) ||
                    (GamePad.GetState(((MyGame)this.Game).player3.Controller).Buttons.LeftShoulder == ButtonState.Pressed) || (GamePad.GetState(((MyGame)this.Game).player4.Controller).Buttons.LeftShoulder == ButtonState.Pressed))
                    && buttonReleased == true)
                {
                    if (ShowText == false)
                    {
                        this.PlayNextSong();
                        buttonReleased = false;
                    }
                }

                //Don't change the song until release.
                if ((GamePad.GetState(((MyGame)this.Game).player1.Controller).Buttons.LeftShoulder == ButtonState.Released) && (GamePad.GetState(((MyGame)this.Game).player2.Controller).Buttons.LeftShoulder == ButtonState.Released) &&
                    (GamePad.GetState(((MyGame)this.Game).player3.Controller).Buttons.LeftShoulder == ButtonState.Released) && (GamePad.GetState(((MyGame)this.Game).player4.Controller).Buttons.LeftShoulder == ButtonState.Released))
                {
                    buttonReleased = true;
                }

                if (isShowingInfo)
                {
                    this.UpdateLogoAnimation(gameTime);
                }
            }

            base.Update(gameTime);
        }

        public Rectangle GetTitleSafeArea()
        {
            PresentationParameters pp = ((MyGame)this.Game).GraphicsDevice.PresentationParameters;
            Rectangle retval = new Rectangle(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);        
            int offsetx = (pp.BackBufferWidth + 9) / 10;
            int offsety = (pp.BackBufferHeight + 9) / 10;
            retval.Inflate(-offsetx, -offsety);
            return retval;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            String[] song;

            SpriteBatch batch = ((MyGame)this.Game).screenManager.SpriteBatch;
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Resolution.getTransformationMatrix());

            if (isShowingInfo)
            {
                Vector2 position = new Vector2(drawPosition.X - Convert.ToInt32(this.logoSizeValue), drawPosition.Y - Convert.ToInt32(this.logoSizeValue));

                song = songsList[nextSong].Split('|');

                if (ShowText)
                {
                    batch.Draw(retroTraxBkgTexture, new Vector2(drawPosition.X + 50, drawPosition.Y + 2), new Color(255, 255, 255, MathHelper.Clamp(200, 0, 255)));
                }

                batch.Draw(retroTraxLogoTexture, position,
                        new Rectangle(0, 0, retroTraxLogoTexture.Width, retroTraxLogoTexture.Height), Color.White, 0.0f, Vector2.Zero, this.logoSizeValue, SpriteEffects.None, 1.0f);

                if (ShowText)
                {
                    // Draw Song Title
                    batch.DrawString(font, song[0], new Vector2(drawPosition.X + 105, drawPosition.Y + 8), Color.White);

                    // Draw Song Artist
                    batch.DrawString(fontItalic, song[1], new Vector2(drawPosition.X + 105, drawPosition.Y + 40), Color.White);

                    // Draw Song Album
                    batch.DrawString(fontSmallItalic, song[2], new Vector2(drawPosition.X + 105, drawPosition.Y + 70), Color.WhiteSmoke);
                }
            }

            batch.End();
        }
    }
}
