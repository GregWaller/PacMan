using System.Collections.Generic;
using UnityEngine;

namespace LongRoadGames.PacMan
{
    public enum AudioClipID
    {
        GameStart,

        Death,
        EatFruit,
        EatGhost,
        Munch_1,
        Munch_2,

        Extend,
        PowerPellet,
    }

    public class AudioController : ScriptableObject
    {
        #region Singleton
        private static AudioController _instance;
        public static AudioController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = CreateInstance<AudioController>();
                return _instance;
            }
        }
        private AudioController() { }
        #endregion

        private Dictionary<AudioClipID, AudioClip> _audioClips;
        private AudioSource _sfx;
        private AudioSource _loops;
        private int _chompIDX;
        private AudioClipID[] _chomps;

        public void Initialize(AudioSource sfxSource, AudioSource loopSource)
        {
            _sfx = sfxSource;
            _loops = loopSource;
            _audioClips = new Dictionary<AudioClipID, AudioClip>()
            {
                { AudioClipID.GameStart, Resources.Load<AudioClip>("Sounds/game_start") },
                { AudioClipID.Death, Resources.Load<AudioClip>("Sounds/death") },
                { AudioClipID.EatFruit, Resources.Load<AudioClip>("Sounds/eat_fruit") },
                { AudioClipID.EatGhost, Resources.Load<AudioClip>("Sounds/eat_ghost") },
                { AudioClipID.Munch_1, Resources.Load<AudioClip>("Sounds/munch_1") },
                { AudioClipID.Munch_2, Resources.Load<AudioClip>("Sounds/munch_2") },
                { AudioClipID.Extend, Resources.Load<AudioClip>("Sounds/extend") },
                { AudioClipID.PowerPellet, Resources.Load<AudioClip>("Sounds/power_pellet") },
            };

            _chompIDX = 0;
            _chomps = new AudioClipID[2]
            {
                AudioClipID.Munch_1,
                AudioClipID.Munch_2
            };
        }

        public void Play(AudioClipID clipID)
        {
            if (_audioClips.ContainsKey(clipID))
                _sfx.PlayOneShot(_audioClips[clipID]);
        }
        public void PlayChomp()
        {
            Play(_chomps[_chompIDX]);
            _chompIDX = 1 - _chompIDX;
        }

        public void StartLoop(AudioClipID clipID)
        {
            _loops.clip = _audioClips[clipID];
            _loops.Play();
        }
        public void StopLoop()
        {
            _loops.Stop();
        }

        public float Duration(AudioClipID clipID)
        {
            if (_audioClips.ContainsKey(clipID))
                return _audioClips[clipID].length;
            return 0.0f;
        }
    }
}
