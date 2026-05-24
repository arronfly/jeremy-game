using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 音频管理器 - 程序化生成所有音效和音乐
/// 所有音频通过代码生成，无需外部音频文件
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const int SAMPLE_RATE = 44100;

    private Dictionary<string, AudioClip> soundCache = new Dictionary<string, AudioClip>();

    private AudioSource musicSource;
    private AudioSource[] sfxSources;
    private int sfxSourceCount = 4;
    private int sfxIndex = 0;

    private float musicVolume = 0.5f;
    private float sfxVolume = 0.7f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
        GenerateAllSounds();
    }

    void SetupAudioSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        sfxSources = new AudioSource[sfxSourceCount];
        for (int i = 0; i < sfxSourceCount; i++)
        {
            sfxSources[i] = gameObject.AddComponent<AudioSource>();
            sfxSources[i].loop = false;
            sfxSources[i].playOnAwake = false;
            sfxSources[i].volume = sfxVolume;
        }
    }

    public void PlaySFX(string name, float volume = 1f, float pitch = 1f)
    {
        if (!soundCache.ContainsKey(name))
        {
            Debug.LogWarning("AudioManager: SFX not found - " + name);
            return;
        }

        AudioSource source = sfxSources[sfxIndex];
        sfxIndex = (sfxIndex + 1) % sfxSourceCount;

        source.clip = soundCache[name];
        source.volume = sfxVolume * volume;
        source.pitch = pitch;
        source.Play();
    }

    public void PlayMusic(string name)
    {
        if (!soundCache.ContainsKey(name))
        {
            Debug.LogWarning("AudioManager: Music not found - " + name);
            return;
        }

        musicSource.clip = soundCache[name];
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        for (int i = 0; i < sfxSourceCount; i++)
        {
            sfxSources[i].volume = sfxVolume;
        }
    }

    // ================================================================
    // Sound Generation
    // ================================================================

    void GenerateAllSounds()
    {
        GenerateGunSounds();
        GenerateExplosionSound();
        GenerateFootstepSound();
        GenerateReloadSound();
        GenerateUISounds();
        GenerateRoundSounds();
        GenerateBGM();

        Debug.Log("AudioManager: All sounds generated. Total: " + soundCache.Count);
    }

    // ---- Gun Sounds ----

    void GenerateGunSounds()
    {
        GenerateARShoot();
        GenerateSniperShoot();
        GenerateSMGShoot();
        GenerateShotgunShoot();
    }

    void GenerateARShoot()
    {
        int samples = 4410; // 0.1s
        float[] noise = GenerateWhiteNoise(samples);

        // Exponential decay envelope
        for (int i = 0; i < samples; i++)
        {
            noise[i] *= (float)Math.Pow(0.95, i);
        }

        // Low-pass filter
        ApplyLowPass(noise, 0.7f);

        // Low-frequency thump: 100Hz sine, very short
        float[] thump = CreateSineWave(100f, 882, 0.4f);
        // Fade thump quickly
        for (int i = 0; i < thump.Length; i++)
        {
            thump[i] *= 1f - (i / (float)thump.Length);
        }

        float[] mixed = MixSamples(noise, thump);

        // Normalize
        NormalizeSamples(mixed, 0.8f);

        soundCache["ar_shoot"] = CreateClip("ar_shoot", mixed);
    }

    void GenerateSniperShoot()
    {
        int samples = 13230; // 0.3s
        float[] noise = GenerateWhiteNoise(samples);

        // Strong initial transient (first 500 samples at full volume)
        for (int i = 0; i < samples; i++)
        {
            if (i < 500)
            {
                noise[i] *= 1.0f;
            }
            else
            {
                noise[i] *= (float)Math.Pow(0.97, i - 500);
            }
        }

        // More bass emphasis - low-pass aggressively
        ApplyLowPass(noise, 0.8f);

        // Add bass sine
        float[] bass = CreateSineWave(60f, samples, 0.5f);
        for (int i = 0; i < samples; i++)
        {
            bass[i] *= (float)Math.Pow(0.98, i);
        }

        float[] mixed = MixSamples(noise, bass);

        // Echo: delayed copy at 2000 samples later, 0.3 volume
        for (int i = 2000; i < samples; i++)
        {
            mixed[i] += mixed[i - 2000] * 0.3f;
        }

        NormalizeSamples(mixed, 0.85f);
        soundCache["sniper_shoot"] = CreateClip("sniper_shoot", mixed);
    }

    void GenerateSMGShoot()
    {
        int samples = 2205; // 0.05s
        float[] noise = GenerateWhiteNoise(samples);

        // Sharp transient, very fast decay
        for (int i = 0; i < samples; i++)
        {
            if (i < 100)
            {
                noise[i] *= 1.0f;
            }
            else
            {
                noise[i] *= (float)Math.Pow(0.9, i - 100);
            }
        }

        // Higher pitch - less aggressive low-pass (let more highs through)
        ApplyLowPass(noise, 0.4f);

        // Mix with a high-frequency click
        float[] click = CreateSineWave(1200f, 200, 0.3f);
        float[] mixed = MixSamples(noise, click);

        NormalizeSamples(mixed, 0.75f);
        soundCache["smg_shoot"] = CreateClip("smg_shoot", mixed);
    }

    void GenerateShotgunShoot()
    {
        int samples = 8820; // 0.2s
        float[] noise = GenerateWhiteNoise(samples);

        // Double burst: two noise bursts 500 samples apart
        for (int i = 0; i < samples; i++)
        {
            // First burst: 0-1500
            // Gap/dip: 1500-2000
            // Second burst: 2000-3500
            // Then decay
            if (i < 1500)
            {
                noise[i] *= (float)Math.Pow(0.96, i);
            }
            else if (i < 2000)
            {
                noise[i] *= 0.3f * (float)Math.Pow(0.95, i - 1500);
            }
            else if (i < 3500)
            {
                noise[i] *= 0.8f * (float)Math.Pow(0.96, i - 2000);
            }
            else
            {
                noise[i] *= (float)Math.Pow(0.94, i - 3500);
            }
        }

        ApplyLowPass(noise, 0.7f);

        // Rich low frequency: 100Hz sine mixed in
        float[] bass = CreateSineWave(100f, samples, 0.4f);
        for (int i = 0; i < samples; i++)
        {
            bass[i] *= (float)Math.Pow(0.96, i);
        }

        float[] mixed = MixSamples(noise, bass);
        NormalizeSamples(mixed, 0.85f);
        soundCache["shotgun_shoot"] = CreateClip("shotgun_shoot", mixed);
    }

    // ---- Explosion ----

    void GenerateExplosionSound()
    {
        int samples = 22050; // 0.5s
        float[] noise = GenerateWhiteNoise(samples);

        // Slow decay
        for (int i = 0; i < samples; i++)
        {
            noise[i] *= (float)Math.Pow(0.997, i);
        }

        ApplyLowPass(noise, 0.75f);

        // Heavy low frequency component: 60Hz sine, louder at start
        float[] bass = CreateSineWave(60f, samples, 0.6f);
        for (int i = 0; i < samples; i++)
        {
            bass[i] *= (float)Math.Pow(0.995, i);
        }

        float[] mixed = MixSamples(noise, bass);

        // Echo: delayed copy at 4000 samples, 0.4 volume
        for (int i = 4000; i < samples; i++)
        {
            mixed[i] += mixed[i - 4000] * 0.4f;
        }

        NormalizeSamples(mixed, 0.9f);
        soundCache["explosion"] = CreateClip("explosion", mixed);
    }

    // ---- Footstep ----

    void GenerateFootstepSound()
    {
        int samples = 882; // 0.02s
        float[] noise = GenerateWhiteNoise(samples);

        // Quick attack and decay
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)samples;
            // Quick ramp up, then quick decay
            if (t < 0.1f)
            {
                noise[i] *= t / 0.1f;
            }
            else
            {
                noise[i] *= (float)Math.Pow(0.92, i - (int)(samples * 0.1f));
            }
        }

        // Band-pass filter: high-pass then low-pass to keep only mids
        ApplyHighPass(noise, 0.6f);
        ApplyLowPass(noise, 0.5f);

        NormalizeSamples(noise, 0.5f);
        soundCache["footstep"] = CreateClip("footstep", noise);
    }

    // ---- Reload ----

    void GenerateReloadSound()
    {
        int samples = 4410; // 0.1s
        float[] noise = GenerateWhiteNoise(samples);

        // Metallic click: sine wave 800Hz, fast decay
        float[] click = CreateSineWave(800f, samples, 0.5f);
        for (int i = 0; i < samples; i++)
        {
            click[i] *= (float)Math.Pow(0.93, i);
        }

        // Noise for texture, very low volume
        for (int i = 0; i < samples; i++)
        {
            noise[i] *= 0.15f * (float)Math.Pow(0.95, i);
        }

        float[] mixed = MixSamples(click, noise);

        NormalizeSamples(mixed, 0.65f);
        soundCache["reload"] = CreateClip("reload", mixed);
    }

    // ---- UI Sounds ----

    void GenerateUISounds()
    {
        GenerateUIClick();
        GenerateUIHover();
    }

    void GenerateUIClick()
    {
        int samples = 2205; // 0.05s
        float[] sine = CreateSineWave(1000f, samples, 0.4f);

        // Quick decay
        for (int i = 0; i < samples; i++)
        {
            sine[i] *= (float)Math.Pow(0.94, i);
        }

        NormalizeSamples(sine, 0.5f);
        soundCache["ui_click"] = CreateClip("ui_click", sine);
    }

    void GenerateUIHover()
    {
        int samples = 1102; // 0.025s
        float[] sine = CreateSineWave(800f, samples, 0.15f);

        // Very quiet, quick decay
        for (int i = 0; i < samples; i++)
        {
            sine[i] *= (float)Math.Pow(0.92, i);
        }

        NormalizeSamples(sine, 0.3f);
        soundCache["ui_hover"] = CreateClip("ui_hover", sine);
    }

    // ---- Round Sounds ----

    void GenerateRoundSounds()
    {
        GenerateRoundStart();
        GenerateRoundEnd();
        GenerateVictory();
        GenerateKillFeed();
    }

    void GenerateRoundStart()
    {
        int samples = 13230; // 0.3s
        float[] c4 = CreateSineWave(261.63f, samples, 0.4f);
        float[] e4 = CreateSineWave(329.63f, samples, 0.4f);

        float[] mixed = MixSamples(c4, e4);

        // Trumpet-like envelope: quick attack, sustain, then decay
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)samples;
            if (t < 0.05f)
            {
                mixed[i] *= t / 0.05f;
            }
            else if (t < 0.6f)
            {
                mixed[i] *= 1.0f;
            }
            else
            {
                mixed[i] *= (1f - t) / 0.4f;
            }
        }

        NormalizeSamples(mixed, 0.6f);
        soundCache["round_start"] = CreateClip("round_start", mixed);
    }

    void GenerateRoundEnd()
    {
        int samples = 13230; // 0.3s
        float[] tone = new float[samples];

        // Descending tone: C4 (261Hz) to A3 (220Hz) over 0.3s
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)samples;
            float freq = 261.63f + (220f - 261.63f) * t;
            tone[i] = 0.4f * (float)Math.Sin(2.0 * Math.PI * freq * i / SAMPLE_RATE);
        }

        // Envelope
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)samples;
            if (t < 0.05f)
            {
                tone[i] *= t / 0.05f;
            }
            else if (t < 0.7f)
            {
                tone[i] *= 1.0f;
            }
            else
            {
                tone[i] *= (1f - t) / 0.3f;
            }
        }

        NormalizeSamples(tone, 0.55f);
        soundCache["round_end"] = CreateClip("round_end", tone);
    }

    void GenerateVictory()
    {
        int samples = 26460; // 0.6s
        float[] tone = new float[samples];

        // Ascending arpeggio: C4 -> E4 -> G4 -> C5, each note 0.15s
        float[] freqs = { 261.63f, 329.63f, 392f, 523.25f };
        int noteLength = samples / 4; // 6615 samples per note

        for (int note = 0; note < 4; note++)
        {
            float freq = freqs[note];
            int start = note * noteLength;
            int end = (note == 3) ? samples : start + noteLength;

            for (int i = start; i < end; i++)
            {
                float localT = (i - start) / (float)noteLength;
                float envelope;
                if (localT < 0.05f)
                {
                    envelope = localT / 0.05f;
                }
                else if (localT < 0.7f)
                {
                    envelope = 1.0f;
                }
                else
                {
                    envelope = (1f - localT) / 0.3f;
                }

                tone[i] = 0.35f * envelope * (float)Math.Sin(2.0 * Math.PI * freq * (i - start) / SAMPLE_RATE);
            }
        }

        NormalizeSamples(tone, 0.65f);
        soundCache["victory"] = CreateClip("victory", tone);
    }

    void GenerateKillFeed()
    {
        int samples = 4410; // 0.1s
        float[] sine = CreateSineWave(2000f, samples, 0.3f);

        // Quick "ding" decay
        for (int i = 0; i < samples; i++)
        {
            sine[i] *= (float)Math.Pow(0.96, i);
        }

        NormalizeSamples(sine, 0.5f);
        soundCache["kill_feed"] = CreateClip("kill_feed", sine);
    }

    // ---- Background Music (8-second loop, 120 BPM) ----

    void GenerateBGM()
    {
        int samples = 352800; // 8s at 44100Hz
        float[] bgm = new float[samples];

        int samplesPerBeat = SAMPLE_RATE / 2; // 22050 samples per beat at 120 BPM
        int samplesPerEighth = samplesPerBeat / 2; // 11025 samples per 8th note

        // Bass line pattern: C(2beats) F(2beats) G(2beats) C(2beats) = 8 beats total
        float[] bassFreqs = { 65.41f, 65.41f, 87.31f, 87.31f, 98f, 98f, 65.41f, 65.41f };

        for (int i = 0; i < samples; i++)
        {
            float sample = 0f;

            // ---- Kick drum: every beat (0s, 0.5s, 1s, ...) ----
            int positionInBeat = i % samplesPerBeat;
            if (positionInBeat < 2205) // 0.05s kick duration
            {
                float kickT = positionInBeat / 2205f;
                // Pitch drops from 150Hz to 60Hz
                float kickFreq = 150f - 90f * kickT;
                sample += 0.25f * (1f - kickT) * (float)Math.Sin(2.0 * Math.PI * kickFreq * positionInBeat / SAMPLE_RATE);
            }

            // ---- Snare: beats 2 and 4 within each 4-beat bar ----
            // Beats are at positions: 0, 1, 2, 3, 4, 5, 6, 7
            int currentBeat = i / samplesPerBeat;
            int posInBeatForSnare = i % samplesPerBeat;
            bool isSnareBeat = (currentBeat % 4 == 1 || currentBeat % 4 == 3);

            if (isSnareBeat && posInBeatForSnare < 4410) // 0.1s snare
            {
                float snareT = posInBeatForSnare / 4410f;
                float snareEnv = (float)Math.Pow(0.96, posInBeatForSnare);
                // Noise-based snare
                float snareNoise = (float)(random.NextDouble() * 2.0 - 1.0);
                sample += 0.12f * snareEnv * snareNoise;
                // Snare body tone
                sample += 0.06f * snareEnv * (float)Math.Sin(2.0 * Math.PI * 200 * posInBeatForSnare / SAMPLE_RATE);
            }

            // ---- Hi-hat: every 8th note ----
            int posInEighth = i % samplesPerEighth;
            if (posInEighth < 441) // ~0.01s tick
            {
                float hhEnv = (float)Math.Pow(0.9, posInEighth);
                float hhNoise = (float)(random.NextDouble() * 2.0 - 1.0);
                sample += 0.06f * hhEnv * hhNoise;
            }

            // ---- Bass line ----
            float bassFreq = bassFreqs[currentBeat];
            float bassEnv = 0.7f;
            // Subtle envelope per beat
            if (posInBeatForSnare < 500)
            {
                bassEnv = 0.7f * (posInBeatForSnare / 500f);
            }
            sample += 0.18f * bassEnv * (float)Math.Sin(2.0 * Math.PI * bassFreq * i / SAMPLE_RATE);

            bgm[i] = sample;
        }

        // Normalize the whole thing gently
        NormalizeSamples(bgm, 0.6f);

        soundCache["bgm"] = CreateClip("bgm", bgm);
    }

    // ================================================================
    // Helper Methods
    // ================================================================

    private System.Random random = new System.Random(42);

    float[] GenerateWhiteNoise(int sampleCount)
    {
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = (float)(random.NextDouble() * 2.0 - 1.0);
        }
        return samples;
    }

    void ApplyEnvelope(float[] samples, float attackTime, float decayTime)
    {
        int attackSamples = (int)(attackTime * SAMPLE_RATE);
        int decayStart = samples.Length - (int)(decayTime * SAMPLE_RATE);

        if (attackSamples > samples.Length) attackSamples = samples.Length;
        if (decayStart < 0) decayStart = 0;
        if (decayStart > samples.Length) decayStart = samples.Length;

        for (int i = 0; i < samples.Length; i++)
        {
            if (i < attackSamples)
            {
                samples[i] *= (float)i / attackSamples;
            }
            else if (i >= decayStart)
            {
                int decaySamples = samples.Length - decayStart;
                if (decaySamples > 0)
                {
                    samples[i] *= (float)(samples.Length - i) / decaySamples;
                }
            }
        }
    }

    void ApplyLowPass(float[] samples, float factor)
    {
        for (int i = 1; i < samples.Length; i++)
        {
            samples[i] = samples[i - 1] * factor + samples[i] * (1f - factor);
        }
    }

    void ApplyHighPass(float[] samples, float factor)
    {
        for (int i = 1; i < samples.Length; i++)
        {
            samples[i] = samples[i] * (1f - factor) + samples[i - 1] * factor;
        }
    }

    float[] MixSamples(float[] a, float[] b)
    {
        int maxLength = Math.Max(a.Length, b.Length);
        float[] mixed = new float[maxLength];

        for (int i = 0; i < maxLength; i++)
        {
            float sampleA = (i < a.Length) ? a[i] : 0f;
            float sampleB = (i < b.Length) ? b[i] : 0f;
            mixed[i] = sampleA + sampleB;
        }

        return mixed;
    }

    float[] CreateSineWave(float frequency, int sampleCount, float amplitude)
    {
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = amplitude * (float)Math.Sin(2.0 * Math.PI * frequency * i / SAMPLE_RATE);
        }
        return samples;
    }

    void NormalizeSamples(float[] samples, float targetPeak)
    {
        float peak = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            float abs = Math.Abs(samples[i]);
            if (abs > peak) peak = abs;
        }

        if (peak > 0.0001f)
        {
            float scale = targetPeak / peak;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scale;
            }
        }
    }

    AudioClip CreateClip(string name, float[] samples)
    {
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SAMPLE_RATE, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
