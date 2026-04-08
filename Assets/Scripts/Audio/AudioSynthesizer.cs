using UnityEngine;
using System.Collections.Generic;

namespace BadNorth3D
{
    /// <summary>
    /// 实时音频合成器 - 类似Web Audio API的音频生成系统
    /// 使用OnAudioFilterRead实时生成音频数据
    /// </summary>
    public class AudioSynthesizer : MonoBehaviour
    {
        public static AudioSynthesizer Instance { get; private set; }

        [Header("音频设置")]
        [Range(20f, 20000f)]
        public float masterVolume = 0.5f;
        public int sampleRate = 44100;
        public int bufferSize = 1024;

        [Header("音效设置")]
        public float sfxVolume = 0.7f;
        public float musicVolume = 0.3f;

        // 活跃的声音列表
        private List<Voice> activeVoices = new List<Voice>();
        private List<Voice> musicVoices = new List<Voice>();
        private float[] audioBuffer;
        private int bufferIndex = 0;

        // 音乐生成器
        private MusicGenerator musicGenerator;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 添加AudioSource组件
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = true;
            source.loop = true;
            source.volume = 1f;

            musicGenerator = new MusicGenerator();
        }

        void Start()
        {
            // 开始生成背景音乐
            StartBackgroundMusic();
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            // 清空缓冲区
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0f;
            }

            // 混合所有音效声音
            float sfxVolumeScaled = sfxVolume * masterVolume;
            for (int i = activeVoices.Count - 1; i >= 0; i--)
            {
                Voice voice = activeVoices[i];
                if (!voice.IsAlive)
                {
                    activeVoices.RemoveAt(i);
                    continue;
                }

                for (int sample = 0; sample < data.Length; sample += channels)
                {
                    float sampleValue = voice.GetSample() * sfxVolumeScaled;
                    data[sample] += sampleValue;
                    if (channels == 2)
                    {
                        data[sample + 1] += sampleValue;
                    }
                }
            }

            // 混合所有音乐声音
            float musicVolumeScaled = musicVolume * masterVolume;
            for (int i = musicVoices.Count - 1; i >= 0; i--)
            {
                Voice voice = musicVoices[i];
                if (!voice.IsAlive)
                {
                    musicVoices.RemoveAt(i);
                    continue;
                }

                for (int sample = 0; sample < data.Length; sample += channels)
                {
                    float sampleValue = voice.GetSample() * musicVolumeScaled;
                    data[sample] += sampleValue;
                    if (channels == 2)
                    {
                        data[sample + 1] += sampleValue;
                    }
                }
            }

            // 防止削波
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Mathf.Clamp(data[i], -1f, 1f);
            }
        }

        #region 音效播放

        /// <summary>
        /// 播放攻击音效
        /// </summary>
        public void PlayAttackSound()
        {
            // 快速下降的锯齿波 - 模拟挥击声
            Voice voice = new Voice(800f, 0.15f, Waveform.Sawtooth);
            voice.SetEnvelope(0.001f, 0.02f, 0.5f, 0.1f);
            voice.SetPitchEnvelope(800f, 200f);
            activeVoices.Add(voice);
        }

        /// <summary>
        /// 播放命中音效
        /// </summary>
        public void PlayHitSound()
        {
            // 短促的噪音脉冲
            Voice voice = new Voice(200f, 0.1f, Waveform.Square);
            voice.SetEnvelope(0.001f, 0.01f, 0.3f, 0.05f);
            voice.SetPitchEnvelope(200f, 50f);
            activeVoices.Add(voice);
        }

        /// <summary>
        /// 播放死亡音效
        /// </summary>
        public void PlayDeathSound()
        {
            // 下降的正弦波 - 模拟倒下声
            Voice voice = new Voice(400f, 0.4f, Waveform.Sine);
            voice.SetEnvelope(0.01f, 0.1f, 0.6f, 0.2f);
            voice.SetPitchEnvelope(400f, 50f);
            activeVoices.Add(voice);
        }

        /// <summary>
        /// 播放UI点击音效
        /// </summary>
        public void PlayUIClickSound()
        {
            // 高频短促音
            Voice voice = new Voice(1200f, 0.05f, Waveform.Sine);
            voice.SetEnvelope(0.001f, 0.01f, 0.8f, 0.02f);
            activeVoices.Add(voice);
        }

        /// <summary>
        /// 播放选择单位音效
        /// </summary>
        public void PlaySelectSound()
        {
            // 上升的音调
            Voice voice = new Voice(600f, 0.1f, Waveform.Sine);
            voice.SetEnvelope(0.001f, 0.02f, 0.7f, 0.05f);
            voice.SetPitchEnvelope(400f, 800f);
            activeVoices.Add(voice);
        }

        /// <summary>
        /// 播放波次完成音效
        /// </summary>
        public void PlayWaveCompleteSound()
        {
            // 胜利的和弦
            float[] frequencies = { 523.25f, 659.25f, 783.99f }; // C5, E5, G5
            foreach (float freq in frequencies)
            {
                Voice voice = new Voice(freq, 0.5f, Waveform.Sine);
                voice.SetEnvelope(0.01f, 0.1f, 0.7f, 0.3f);
                activeVoices.Add(voice);
            }
        }

        /// <summary>
        /// 播放游戏失败音效
        /// </summary>
        public void PlayGameOverSound()
        {
            // 下降的悲伤和弦
            float[] frequencies = { 392f, 349.23f, 329.63f }; // G4, F4, E4
            foreach (float freq in frequencies)
            {
                Voice voice = new Voice(freq, 1.0f, Waveform.Sine);
                voice.SetEnvelope(0.05f, 0.2f, 0.5f, 0.5f);
                voice.SetPitchEnvelope(freq, freq * 0.5f);
                activeVoices.Add(voice);
            }
        }

        #endregion

        #region 背景音乐

        private void StartBackgroundMusic()
        {
            musicGenerator.Start(musicVoices);
            InvokeRepeating(nameof(GenerateMusicBeat), 0f, 0.5f);
        }

        private void GenerateMusicBeat()
        {
            if (!isActiveAndEnabled) return;

            musicGenerator.GenerateBeat(musicVoices);
        }

        #endregion

        #region 声音类

        /// <summary>
        /// 单个声音实例
        /// </summary>
        private class Voice
        {
            private float frequency;
            private float duration;
            private float elapsedTime;
            private Waveform waveform;
            private bool isAlive;

            // ADSR包络
            private float attackTime;
            private float decayTime;
            private float sustainLevel;
            private float releaseTime;
            private float envelopePhase; // 0=attack, 1=decay, 2=sustain, 3=release, 4=done

            // 音调包络
            private float startFrequency;
            private float endFrequency;
            private bool usePitchEnvelope;

            private float sampleRate = 44100;
            private float phase;

            public Voice(float frequency, float duration, Waveform waveform)
            {
                this.frequency = frequency;
                this.duration = duration;
                this.waveform = waveform;
                this.elapsedTime = 0f;
                this.isAlive = true;
                this.envelopePhase = 0f;
                this.phase = 0f;
                this.usePitchEnvelope = false;

                // 默认包络
                this.attackTime = 0.01f;
                this.decayTime = 0.1f;
                this.sustainLevel = 0.7f;
                this.releaseTime = 0.1f;
            }

            public void SetEnvelope(float attack, float decay, float sustain, float release)
            {
                this.attackTime = attack;
                this.decayTime = decay;
                this.sustainLevel = sustain;
                this.releaseTime = release;
            }

            public void SetPitchEnvelope(float start, float end)
            {
                this.startFrequency = start;
                this.endFrequency = end;
                this.usePitchEnvelope = true;
            }

            public float GetSample()
            {
                if (!isAlive)
                    return 0f;

                // 计算包络
                float envelope = 0f;
                float totalADSRTime = attackTime + decayTime + releaseTime;

                if (elapsedTime < attackTime)
                {
                    // Attack phase
                    envelope = elapsedTime / attackTime;
                    envelopePhase = 0f;
                }
                else if (elapsedTime < attackTime + decayTime)
                {
                    // Decay phase
                    float decayProgress = (elapsedTime - attackTime) / decayTime;
                    envelope = 1f - (decayProgress * (1f - sustainLevel));
                    envelopePhase = 1f;
                }
                else if (elapsedTime < duration - releaseTime)
                {
                    // Sustain phase
                    envelope = sustainLevel;
                    envelopePhase = 2f;
                }
                else if (elapsedTime < duration)
                {
                    // Release phase
                    float releaseProgress = (elapsedTime - (duration - releaseTime)) / releaseTime;
                    envelope = sustainLevel * (1f - releaseProgress);
                    envelopePhase = 3f;
                }
                else
                {
                    // Done
                    isAlive = false;
                    return 0f;
                }

                // 计算当前频率
                float currentFreq = frequency;
                if (usePitchEnvelope)
                {
                    float t = elapsedTime / duration;
                    currentFreq = Mathf.Lerp(startFrequency, endFrequency, t);
                }

                // 生成波形
                float sample = GenerateWaveform(currentFreq, waveform);

                // 应用包络
                sample *= envelope;

                // 更新时间和相位
                elapsedTime += 1f / sampleRate;
                phase += (currentFreq / sampleRate);

                return sample;
            }

            private float GenerateWaveform(float freq, Waveform wave)
            {
                float t = phase % 1f;

                switch (wave)
                {
                    case Waveform.Sine:
                        return Mathf.Sin(2f * Mathf.PI * t);
                    case Waveform.Square:
                        return (t < 0.5f) ? 1f : -1f;
                    case Waveform.Sawtooth:
                        return 2f * (t - Mathf.Floor(t + 0.5f));
                    case Waveform.Triangle:
                        return 2f * Mathf.Abs(2f * (t - Mathf.Floor(t + 0.5f))) - 1f;
                    default:
                        return 0f;
                }
            }

            public bool IsAlive => isAlive;
        }

        #endregion

        #region 波形枚举

        public enum Waveform
        {
            Sine,
            Square,
            Sawtooth,
            Triangle
        }

        #endregion

        #region 音乐生成器

        /// <summary>
        /// 简单的程序化音乐生成器
        /// </summary>
        private class MusicGenerator
        {
            private int currentBeat = 0;
            private float[] majorScale = { 1f, 1.125f, 1.25f, 1.333f, 1.5f, 1.667f, 1.875f }; // 大调音阶
            private float baseNote = 261.63f; // C4

            private string[] chordProgression = { "I", "IV", "V", "I" }; // I-IV-V-I 和弦进行
            private int currentChord = 0;

            public void Start(List<Voice> voices)
            {
                // 初始化背景低音
            }

            public void GenerateBeat(List<Voice> voices)
            {
                // 每4拍切换和弦
                if (currentBeat % 4 == 0)
                {
                    currentChord = (currentChord + 1) % chordProgression.Length;
                }

                // 生成低音线
                if (currentBeat % 2 == 0)
                {
                    PlayBassNote(voices);
                }

                // 生成旋律（偶尔）
                if (Random.value > 0.7f)
                {
                    PlayMelodyNote(voices);
                }

                currentBeat++;
            }

            private void PlayBassNote(List<Voice> voices)
            {
                float[] chordNotes = GetChordNotes(chordProgression[currentChord]);
                float freq = chordNotes[0] * baseNote * 0.5f; // 低八度

                Voice voice = new Voice(freq, 0.8f, Waveform.Sine);
                voice.SetEnvelope(0.05f, 0.1f, 0.6f, 0.3f);
                voices.Add(voice);
            }

            private void PlayMelodyNote(List<Voice> voices)
            {
                float[] chordNotes = GetChordNotes(chordProgression[currentChord]);
                float freq = chordNotes[Random.Range(0, chordNotes.Length)] * baseNote;

                // 随机八度
                if (Random.value > 0.5f)
                {
                    freq *= 2f;
                }

                Voice voice = new Voice(freq, 0.3f, Waveform.Sine);
                voice.SetEnvelope(0.02f, 0.05f, 0.5f, 0.2f);
                voices.Add(voice);
            }

            private float[] GetChordNotes(string chord)
            {
                switch (chord)
                {
                    case "I": // C大调和弦
                        return new float[] { 1f, 1.25f, 1.5f };
                    case "IV": // F大调和弦
                        return new float[] { 1.333f, 1.667f, 2f };
                    case "V": // G大调和弦
                        return new float[] { 1.5f, 1.875f, 2.25f };
                    default:
                        return new float[] { 1f, 1.25f, 1.5f };
                }
            }
        }

        #endregion
    }
}
