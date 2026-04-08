using UnityEngine;

namespace BadNorth3D
{
    /// <summary>
    /// 音频配置 - 音效和音乐生成的详细参数
    /// AI可以通过修改这些参数来调整游戏的声音体验
    /// </summary>
    public static class AudioConfig
    {
        // ==================== 音效参数 ====================

        public static class SFX
        {
            // 攻击音效
            public static readonly SoundConfig ATTACK = new SoundConfig
            {
                BaseFrequency = 800f,
                Duration = 0.15f,
                WaveType = WaveType.Sawtooth,
                Volume = 0.7f,
                PitchEnvelope = new PitchEnvelope(800f, 200f),
                Envelope = new ADSREnvelope(0.001f, 0.02f, 0.5f, 0.1f)
            };

            // 命中音效
            public static readonly SoundConfig HIT = new SoundConfig
            {
                BaseFrequency = 200f,
                Duration = 0.1f,
                WaveType = WaveType.Square,
                Volume = 0.6f,
                PitchEnvelope = new PitchEnvelope(200f, 50f),
                Envelope = new ADSREnvelope(0.001f, 0.01f, 0.3f, 0.05f)
            };

            // 死亡音效
            public static readonly SoundConfig DEATH = new SoundConfig
            {
                BaseFrequency = 400f,
                Duration = 0.4f,
                WaveType = WaveType.Sine,
                Volume = 0.8f,
                PitchEnvelope = new PitchEnvelope(400f, 50f),
                Envelope = new ADSREnvelope(0.01f, 0.1f, 0.6f, 0.2f)
            };

            // UI点击音效
            public static readonly SoundConfig UI_CLICK = new SoundConfig
            {
                BaseFrequency = 1200f,
                Duration = 0.05f,
                WaveType = WaveType.Sine,
                Volume = 0.5f,
                Envelope = new ADSREnvelope(0.001f, 0.01f, 0.8f, 0.02f)
            };

            // 选择音效
            public static readonly SoundConfig SELECT = new SoundConfig
            {
                BaseFrequency = 600f,
                Duration = 0.1f,
                WaveType = WaveType.Sine,
                Volume = 0.6f,
                PitchEnvelope = new PitchEnvelope(400f, 800f),
                Envelope = new ADSREnvelope(0.001f, 0.02f, 0.7f, 0.05f)
            };

            // 波次完成音效（和弦）
            public static readonly ChordConfig WAVE_COMPLETE = new ChordConfig
            {
                Frequencies = new float[] { 523.25f, 659.25f, 783.99f }, // C5, E5, G5
                Duration = 0.5f,
                Volume = 0.7f,
                WaveType = WaveType.Sine
            };

            // 游戏失败音效（悲伤和弦）
            public static readonly ChordConfig GAME_OVER = new ChordConfig
            {
                Frequencies = new float[] { 392f, 349.23f, 329.63f }, // G4, F4, E4
                Duration = 1.0f,
                Volume = 0.8f,
                WaveType = WaveType.Sine,
                PitchDrop = true
            };
        }

        // ==================== 背景音乐配置 ====================

        public static class Music
        {
            // 基础音乐参数
            public const int TEMPO = 120;
            public const int BASE_NOTE = 261; // C4
            public const float BEAT_INTERVAL = 0.5f; // 120 BPM = 0.5秒/拍

            // 和弦进行
            public static readonly string[] CHORD_PROGRESSION = { "I", "IV", "V", "I" };

            // 大调音阶
            public static readonly float[] MAJOR_SCALE = {
                1.0f,    // C
                1.125f,  // D
                1.25f,   // E
                1.333f,  // F
                1.5f,    // G
                1.667f,  // A
                1.875f   // B
            };

            // 和弦构成
            public static class Chords
            {
                public static readonly float[] I = { 1f, 1.25f, 1.5f };      // C大调
                public static readonly float[] IV = { 1.333f, 1.667f, 2f };  // F大调
                public static readonly float[] V = { 1.5f, 1.875f, 2.25f };  // G大调
            }
        }
    }

    // ==================== 音频配置数据结构 ====================

    /// <summary>
    /// 声音配置
    /// </summary>
    public class SoundConfig
    {
        public float BaseFrequency;
        public float Duration;
        public WaveType WaveType;
        public float Volume;
        public PitchEnvelope PitchEnvelope;
        public ADSREnvelope Envelope;
    }

    /// <summary>
    /// 和弦配置
    /// </summary>
    public class ChordConfig
    {
        public float[] Frequencies;
        public float Duration;
        public float Volume;
        public WaveType WaveType;
        public bool PitchDrop = false;
    }

    /// <summary>
    /// 音调包络
    /// </summary>
    public class PitchEnvelope
    {
        public float StartFrequency;
        public float EndFrequency;

        public PitchEnvelope(float start, float end)
        {
            StartFrequency = start;
            EndFrequency = end;
        }
    }

    /// <summary>
    /// ADSR包络
    /// </summary>
    public class ADSREnvelope
    {
        public float Attack;
        public float Decay;
        public float Sustain;
        public float Release;

        public ADSREnvelope(float attack, float decay, float sustain, float release)
        {
            Attack = attack;
            Decay = decay;
            Sustain = sustain;
            Release = release;
        }
    }

    /// <summary>
    /// 波形类型
    /// </summary>
    public enum WaveType
    {
        Sine,
        Square,
        Sawtooth,
        Triangle
    }
}
