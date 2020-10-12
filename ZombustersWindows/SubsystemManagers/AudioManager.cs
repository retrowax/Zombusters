using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
            FXVolume = 0.7f;
            MusicVolume = 0.5f;
            Sounds = new Queue<SoundEffectInstance>(100);
            random = new Random();
        }
        private SoundEffect Shot;
        private SoundEffect Shotgun;
        private SoundEffect Explosion;
        private SoundEffect ZombieDying;
        private SoundEffect WomanScream;
        private SoundEffect ManScream;

        private SoundEffect FlameThrower;
        private SoundEffectInstance FlameThrowerInstance;

        private SoundEffect MachineGun;
        private SoundEffectInstance MachineGunInstance;

        private SoundEffectInstance ShotInstance;
        private SoundEffectInstance ShotGunInstance;
        private SoundEffectInstance ZombieDyingInstance;
        private SoundEffectInstance WomanScreamInstance;
        private SoundEffectInstance ManScreamInstance;

        private readonly Random random;

        private SoundEffect Splash;

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            MediaPlayer.Volume = MusicVolume;
            MediaPlayer.IsRepeating = true;

            Shot = Game.Content.Load<SoundEffect>("SoundFX/pistol_shot");
            Shotgun = Game.Content.Load<SoundEffect>("SoundFX/shotgun");

            Explosion = Game.Content.Load<SoundEffect>("SoundFX/explosion1");
            ZombieDying = Game.Content.Load<SoundEffect>("SoundFX/zombiedying");
            WomanScream = Game.Content.Load<SoundEffect>("SoundFX/womanscream");
            ManScream = Game.Content.Load<SoundEffect>("SoundFX/critedu");

            FlameThrower = Game.Content.Load<SoundEffect>("SoundFX/flamethrower");
            FlameThrowerInstance = FlameThrower.CreateInstance();
            FlameThrowerInstance.Volume = FXVolume;
            FlameThrowerInstance.IsLooped = false;

            MachineGun = Game.Content.Load<SoundEffect>("SoundFX/machinegun");
            MachineGunInstance = MachineGun.CreateInstance();
            MachineGunInstance.Volume = FXVolume;
            MachineGunInstance.IsLooped = false;

            Splash = Game.Content.Load<SoundEffect>("SoundFX/LogoSplashSound");

            ShotInstance = Shot.CreateInstance();
            ShotInstance.Volume = FXVolume;
            ShotInstance.IsLooped = false;

            ShotGunInstance = Shotgun.CreateInstance();
            ShotGunInstance.Volume = FXVolume;
            ShotGunInstance.IsLooped = false;

            ZombieDyingInstance = ZombieDying.CreateInstance();
            ZombieDyingInstance.Volume = FXVolume;
            ZombieDyingInstance.IsLooped = false;

            WomanScreamInstance = WomanScream.CreateInstance();
            WomanScreamInstance.Volume = FXVolume;
            WomanScreamInstance.IsLooped = false;

            ManScreamInstance = ManScream.CreateInstance();
            ManScreamInstance.Volume = FXVolume;
            ManScreamInstance.IsLooped = false;

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

            base.Update(gameTime);
        }

        private Queue<SoundEffectInstance> Sounds;
        private void AddSound(SoundEffect sound, float volume, float pitch)
        {
            SoundEffectInstance handle = sound.CreateInstance();
            handle.Volume = volume;
            handle.Pitch = pitch;
            handle.Play();
            Sounds.Enqueue(handle);
        }
        public void PlaySplashSound()
        {
            if (!bPaused)
                AddSound(Splash, 1.0f, 0.0f);

        }
        public void PlayShot()
        {
            if (!bPaused)
            {
                if ((!bPaused) && (ShotInstance.State != SoundState.Playing))
                    ShotInstance.Play();
            }
        }

        public void PlayShotgun()
        {
            if (!bPaused)
            {
                if ((!bPaused) && (ShotGunInstance.State != SoundState.Playing))
                    ShotGunInstance.Play();
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
        }
        public void StopMachineGun()
        {
            if (MachineGunInstance.State == SoundState.Playing)
                MachineGunInstance.Stop();
        }

        public void PlayExplosion()
        {
            if (!bPaused)
                AddSound(Explosion, this.FXVolume, 0);

        }

        public void PlayMusic()
        {
        }

        public void PlayMenuMusic()
        {
        }

        public void StopMenuMusic()
        {
        }


        private bool bPaused = false;
        public bool IsPaused { get { return bPaused; } }
        public void PauseSounds()
        {
            bPaused = true;
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
