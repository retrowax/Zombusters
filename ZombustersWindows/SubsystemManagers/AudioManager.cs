using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace ZombustersWindows
{
    
    public class AudioManager : DrawableGameComponent
    {
        private float FXVolume;
        private float MusicVolume;
        public AudioManager(Game game)
            : base(game)
        {
#if WINDOWS_PHONE
            FXVolume = 1.0f;
            MusicVolume = 0.9f;
#else
            FXVolume = 0.7f;
            MusicVolume = 0.5f;
#endif

            Sounds = new Queue<SoundEffectInstance>(100);
            random = new Random();
        }
        private SoundEffect Shot;
        private SoundEffect Explosion;
        private SoundEffect ZombieDying;
        private SoundEffect WomanScream;
        private SoundEffect ManScream;

        private SoundEffect FlameThrower;
        private SoundEffectInstance FlameThrowerInstance;

        private SoundEffect MachineGun;
        private SoundEffectInstance MachineGunInstance;

#if WINDOWS_PHONE || WINDOWS
        private SoundEffectInstance ShotInstance;
        private SoundEffectInstance ZombieDyingInstance;
        private SoundEffectInstance WomanScreamInstance;
        private SoundEffectInstance ManScreamInstance;
        //private SoundEffectInstance SplashInstance;
#endif

        private Random random;
#if !WINDOWS_PHONE && !WINDOWS
        /AudioEngine audioEngine;
        //WaveBank waveBank;
        //SoundBank soundBank;
        //AudioCategory audioCategory;

        //// Cue so we can hang on to the sound of the engine.
        //Cue engineSound = null;
#endif

        //private Song Music;
        //private SoundEffect MenuLoop;
        //private SoundEffectInstance MenuLoopInstance;

        //private SoundEffect Seeker; // Looped
        //private SoundEffectInstance SeekerInstance;

        private SoundEffect Splash; //Splash Logo Sound

        public override void Initialize()
        {
#if !WINDOWS_PHONE
            //audioEngine = new AudioEngine("Content\\default.xgs");
            //waveBank = new WaveBank(audioEngine, "Content\\Wave Bank.xwb");
            //soundBank = new SoundBank(audioEngine, "Content\\Sound Bank.xsb");
#endif

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //Music = Game.Content.Load<Song>("Music/Song1");
            MediaPlayer.Volume = MusicVolume;
            MediaPlayer.IsRepeating = true;

            /* REVISARRR
            MenuLoop = Game.Content.Load<SoundEffect>("Music/MenuLoop");
            MenuLoopInstance = MenuLoop.CreateInstance();
            MenuLoopInstance.IsLooped = true;
             */

            //Seeker = Game.Content.Load<SoundEffect>("SoundFX/engine_2");

            // We call CreateInstance to get a permanent handle
            // on the seeker sound, and set it's pitch, volume,
            // and looping accordingly.
            //SeekerInstance = Seeker.CreateInstance();
            //SeekerInstance.Volume = FXVolume;
            //SeekerInstance.Pitch = 0.75f;
            //SeekerInstance.IsLooped = true;

            /*Shot = Game.Content.Load<SoundEffect>("SoundFX/pistol_shot");


            Explosion = Game.Content.Load<SoundEffect>("SoundFX/explosion1");
            ZombieDying = Game.Content.Load<SoundEffect>("SoundFX/zombiedying");
            WomanScream = Game.Content.Load<SoundEffect>("SoundFX/womanscream");
            ManScream = Game.Content.Load<SoundEffect>("SoundFX/critedu");

            FlameThrower = Game.Content.Load<SoundEffect>("SoundFX/flamethrower");
            FlameThrowerInstance = FlameThrower.CreateInstance();
            FlameThrowerInstance.Volume = FXVolume;
            //FlameThrowerInstance.Pitch = 1.4f;
            FlameThrowerInstance.IsLooped = false;

            MachineGun = Game.Content.Load<SoundEffect>("SoundFX/machinegun");
            MachineGunInstance = MachineGun.CreateInstance();
            MachineGunInstance.Volume = FXVolume;
            //FlameThrowerInstance.Pitch = 1.4f;
            MachineGunInstance.IsLooped = false;

            Splash = Game.Content.Load<SoundEffect>("SoundFX/LogoSplashSound");*/

#if !WINDOWS_PHONE
            // Get the category.
            //audioCategory = audioEngine.GetCategory("Global");
            //audioCategory.SetVolume(this.FXVolume * 1.5f);
#endif
            /*ShotInstance = Shot.CreateInstance();
            ShotInstance.Volume = FXVolume;
            ShotInstance.IsLooped = false;

            ZombieDyingInstance = ZombieDying.CreateInstance();
            ZombieDyingInstance.Volume = FXVolume;
            ZombieDyingInstance.IsLooped = false;

            WomanScreamInstance = WomanScream.CreateInstance();
            WomanScreamInstance.Volume = FXVolume;
            WomanScreamInstance.IsLooped = false;

            ManScreamInstance = ManScream.CreateInstance();
            ManScreamInstance.Volume = FXVolume;
            ManScreamInstance.IsLooped = false;*/

            //SplashInstance = Splash.CreateInstance();
            //SplashInstance.Volume = FXVolume;
            //SplashInstance.IsLooped = false;


            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            // This will help keep the size of the Queue to a minimum.
            for (int i = 0; i < Sounds.Count; i++)
            {
                if (Sounds.Peek().State == SoundState.Stopped)
                    Sounds.Dequeue();
                else
                    break;
            }

#if !WINDOWS_PHONE
            //FlameThrowerInstance.Volume = FXVolume;
//            audioCategory.SetVolume(this.FXVolume * 1.5f);
//            audioEngine.Update();
#endif

            base.Update(gameTime);
        }

        private Queue<SoundEffectInstance> Sounds;
        private void AddSound(SoundEffect sound, float volume, float pitch)
        {
            /*SoundEffectInstance handle = sound.CreateInstance();
            handle.Volume = volume;
            handle.Pitch = pitch;
            handle.Play();
            Sounds.Enqueue(handle);*/
        }
        public void PlaySplashSound()
        {
#if !WINDOWS_PHONE
            if (!bPaused)
                AddSound(Splash, 1.0f, 0.0f);
#else
            SplashInstance.Play();
#endif
        }
        public void PlayShot()
        {
            if (!bPaused)
            {
                if ((!bPaused) && (ShotInstance.State != SoundState.Playing))
                    ShotInstance.Play();
            }
        }
        public void PlayZombieDying()
        {
            if (!bPaused)
            {
                if (random.Next(0, 3) == 1)
                {
                    if ((!bPaused) && (ZombieDyingInstance.State != SoundState.Playing))
                        ZombieDyingInstance.Play();
                }
            }
        }
        public void PlayWomanScream()
        {
            if (!bPaused)
            {
                if ((!bPaused) && (WomanScreamInstance.State != SoundState.Playing))
                    WomanScreamInstance.Play();
            }
        }

        public void PlayManScream()
        {
            if (!bPaused)
            {
                if ((!bPaused) && (ManScreamInstance.State != SoundState.Playing))
                    ManScreamInstance.Play();
            }
        }

        public void PlayFlameThrower()
        {
            if ((!bPaused) && (FlameThrowerInstance.State != SoundState.Playing))
                FlameThrowerInstance.Play();
        }
        public void StopFlameThrower()
        {
            if (FlameThrowerInstance.State == SoundState.Playing)
                FlameThrowerInstance.Stop();
        }

        public void PlayMachineGun()
        {
            if (!bPaused)
            {
                if ((!bPaused) && (MachineGunInstance.State != SoundState.Playing))
                    MachineGunInstance.Play();
            }
            //if ((!bPaused) && (MachineGunInstance.State != SoundState.Playing))
                //MachineGunInstance.Play();
        }
        public void StopMachineGun()
        {
            if (MachineGunInstance.State == SoundState.Playing)
                MachineGunInstance.Stop();
        }

        public void PlayExplosion()
        {
#if !WINDOWS_PHONE
            if (!bPaused)
                AddSound(Explosion, this.FXVolume, 0);
#endif
        }
        //public void PlaySeeker()
        //{
        //    if ((!bPaused) && (SeekerInstance.State != SoundState.Playing))
        //        SeekerInstance.Play();
        //}
        //public void StopSeeker()
        //{
        //    SeekerInstance.Stop();
        //}
        public void PlayMusic()
        {
            /* REVISARRR
            if (Music != null)
            {
                MediaPlayer.Play(Music);
            }
             */
        }
        public void PlayMenuMusic()
        {
            /* REVISARR
            if (MenuLoop != null)
            {
                MenuLoopInstance.Play();
            }
             */
        }
        public void StopMenuMusic()
        {
            //MenuLoopInstance.Stop();
        }


        private bool bPaused = false;
        public bool IsPaused { get { return bPaused; } }
        public void PauseSounds()
        {
            bPaused = true;
            //SeekerInstance.Pause();
            foreach (SoundEffectInstance item in Sounds)
            {
                if (item.State == SoundState.Playing)
                    item.Pause();
            }
        }
        public void PauseAll()
        {
            PauseSounds();
            MediaPlayer.Pause();
        }               
        public void ResumeAll()
        {
            if (bPaused)
            {
                MediaPlayer.Resume();

                //if (SeekerInstance.State == SoundState.Paused)
                //    SeekerInstance.Resume();
                
                foreach (SoundEffectInstance item in Sounds)
                {
                    if (item.State == SoundState.Paused)
                        item.Resume();
                }
                bPaused = false;
            }
        }
        public void SetOptions(float fxVolume, float musicVolume)
        {
            this.FXVolume = fxVolume;
            this.MusicVolume = musicVolume;
            //if (SeekerInstance != null)
            //{
            //    SeekerInstance.Volume = this.FXVolume;
            //}
            MediaPlayer.Volume = this.MusicVolume;
            foreach (SoundEffectInstance item in Sounds)
            {
                item.Volume = this.FXVolume;
            }

            if (ShotInstance != null)
            {
                ShotInstance.Volume = FXVolume;
                ZombieDyingInstance.Volume = FXVolume;
                WomanScreamInstance.Volume = FXVolume;
                ManScreamInstance.Volume = FXVolume;
                MachineGunInstance.Volume = FXVolume;
                FlameThrowerInstance.Volume = FXVolume;
            }
        }
    }
}
