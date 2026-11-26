using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Chatting.Client.Services;

/// <summary>
/// Provides pure synthesized audio notifications using NAudio (no external .wav files required).
/// Supports incoming message chimes and looping ringtones for video/audio calls.
/// </summary>
public class SoundNotificationService : IDisposable
{
    private static readonly Lazy<SoundNotificationService> _instance = new(() => new SoundNotificationService());
    public static SoundNotificationService Instance => _instance.Value;

    private WaveOutEvent? _ringtoneOut;
    private CancellationTokenSource? _ringtoneCts;

    private SoundNotificationService() { }

    /// <summary>
    /// Plays a gentle 2-tone melodic chime ("ting") when a new message arrives.
    /// </summary>
    public void PlayMessageSound()
    {
        Task.Run(() =>
        {
            try
            {
                int sampleRate = 44100;
                int durationMs = 300;
                int sampleCount = (sampleRate * durationMs) / 1000;
                float[] buffer = new float[sampleCount];

                // Synthesize a harmonious dual-tone chime (880Hz + 1320Hz) with smooth fade-out
                for (int i = 0; i < sampleCount; i++)
                {
                    double t = (double)i / sampleRate;
                    double decay = Math.Exp(-12.0 * t); // Exponential volume decay

                    double tone1 = Math.Sin(2.0 * Math.PI * 880.0 * t);
                    double tone2 = Math.Sin(2.0 * Math.PI * 1320.0 * t);

                    buffer[i] = (float)((0.3 * tone1 + 0.2 * tone2) * decay);
                }

                WaveFormat format = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
                using WaveOutEvent waveOut = new();
                RawSourceWaveStream stream = new(new MemoryStream(FloatArrayToByteArray(buffer)), format);
                waveOut.Init(stream);
                waveOut.Play();

                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(20);
                }
            }
            catch { /* audio driver might be busy or unavailable */ }
        });
    }

    /// <summary>
    /// Starts playing a repeating incoming call telephone ringtone until stopped.
    /// </summary>
    public void StartRingtone()
    {
        StopRingtone();
        _ringtoneCts = new CancellationTokenSource();
        var token = _ringtoneCts.Token;

        Task.Run(async () =>
        {
            try
            {
                int sampleRate = 44100;
                WaveFormat format = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);

                // Standard 440Hz + 480Hz dual ring tone (1.5s ring, 2s silence)
                int ringSamples = (int)(sampleRate * 1.5);
                float[] ringBuffer = new float[ringSamples];
                for (int i = 0; i < ringSamples; i++)
                {
                    double t = (double)i / sampleRate;
                    // Envelope attack & release
                    double env = 1.0;
                    if (t < 0.05) env = t / 0.05;
                    else if (t > 1.45) env = (1.5 - t) / 0.05;

                    double s = (Math.Sin(2.0 * Math.PI * 440.0 * t) + Math.Sin(2.0 * Math.PI * 480.0 * t)) * 0.25 * env;
                    ringBuffer[i] = (float)s;
                }

                byte[] ringBytes = FloatArrayToByteArray(ringBuffer);

                while (!token.IsCancellationRequested)
                {
                    using (WaveOutEvent waveOut = new())
                    {
                        _ringtoneOut = waveOut;
                        using RawSourceWaveStream stream = new(new MemoryStream(ringBytes), format);
                        waveOut.Init(stream);
                        waveOut.Play();

                        while (waveOut.PlaybackState == PlaybackState.Playing && !token.IsCancellationRequested)
                        {
                            await Task.Delay(50, token);
                        }
                    }

                    if (token.IsCancellationRequested) break;
                    await Task.Delay(2000, token); // 2s pause between rings
                }
            }
            catch { }
        }, token);
    }

    /// <summary>
    /// Stops the ringing sound immediately.
    /// </summary>
    public void StopRingtone()
    {
        try
        {
            _ringtoneCts?.Cancel();
            _ringtoneCts?.Dispose();
            _ringtoneCts = null;

            if (_ringtoneOut != null)
            {
                _ringtoneOut.Stop();
                _ringtoneOut.Dispose();
                _ringtoneOut = null;
            }
        }
        catch { }
    }

    private WaveOutEvent? _callingOut;
    private CancellationTokenSource? _callingCts;

    /// <summary>
    /// Starts playing an outgoing ringback tone while waiting for receiver to pick up.
    /// </summary>
    public void StartCallingTone()
    {
        StopCallingTone();
        _callingCts = new CancellationTokenSource();
        var token = _callingCts.Token;

        Task.Run(async () =>
        {
            try
            {
                int sampleRate = 44100;
                WaveFormat format = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
                int ringSamples = (int)(sampleRate * 1.0);
                float[] ringBuffer = new float[ringSamples];
                for (int i = 0; i < ringSamples; i++)
                {
                    double t = (double)i / sampleRate;
                    double env = 1.0;
                    if (t < 0.05) env = t / 0.05;
                    else if (t > 0.95) env = (1.0 - t) / 0.05;

                    double s = (Math.Sin(2.0 * Math.PI * 440.0 * t) + Math.Sin(2.0 * Math.PI * 480.0 * t)) * 0.15 * env;
                    ringBuffer[i] = (float)s;
                }

                byte[] ringBytes = FloatArrayToByteArray(ringBuffer);

                while (!token.IsCancellationRequested)
                {
                    using (WaveOutEvent waveOut = new())
                    {
                        _callingOut = waveOut;
                        using RawSourceWaveStream stream = new(new MemoryStream(ringBytes), format);
                        waveOut.Init(stream);
                        waveOut.Play();

                        while (waveOut.PlaybackState == PlaybackState.Playing && !token.IsCancellationRequested)
                        {
                            await Task.Delay(50, token);
                        }
                    }

                    if (token.IsCancellationRequested) break;
                    await Task.Delay(2500, token);
                }
            }
            catch { }
        }, token);
    }

    /// <summary>
    /// Stops the outgoing ringback tone.
    /// </summary>
    public void StopCallingTone()
    {
        try
        {
            _callingCts?.Cancel();
            _callingCts?.Dispose();
            _callingCts = null;

            if (_callingOut != null)
            {
                _callingOut.Stop();
                _callingOut.Dispose();
                _callingOut = null;
            }
        }
        catch { }
    }

    private static byte[] FloatArrayToByteArray(float[] floatArray)
    {
        byte[] byteArray = new byte[floatArray.Length * 4];
        Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    public void Dispose()
    {
        StopRingtone();
        StopCallingTone();
    }
}
