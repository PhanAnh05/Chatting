using NAudio.Wave;

namespace Chatting.Client.Services;

/// <summary>
/// Records microphone audio to a WAV file using NAudio.
/// Supports Start, Stop, Preview (playback), and reports duration.
/// </summary>
public class AudioRecorderService : IDisposable
{
    private WaveInEvent? _waveIn;
    private WaveFileWriter? _writer;
    private WaveOutEvent? _waveOut;
    private AudioFileReader? _reader;
    private readonly object _writerLock = new();

    private string _outputPath = string.Empty;
    private DateTime _startTime;

    public bool IsRecording { get; private set; }
    public bool IsPlaying { get; private set; }
    public int DurationSeconds { get; private set; }

    public event Action<int>? OnRecordingTick; // fired every second with elapsed seconds
    public event Action? OnPlaybackStopped;

    private System.Windows.Forms.Timer? _ticker;

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Starts recording. Returns the temp file path.</summary>
    public string StartRecording()
    {
        StopPlayback();
        Cleanup();

        _outputPath = Path.Combine(Path.GetTempPath(), $"voice_{Guid.NewGuid():N}.wav");
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(44100, 1), // Mono 44.1 kHz
            BufferMilliseconds = 50
        };

        lock (_writerLock)
        {
            _writer = new WaveFileWriter(_outputPath, _waveIn.WaveFormat);
        }

        _waveIn.DataAvailable += (s, e) =>
        {
            lock (_writerLock)
            {
                _writer?.Write(e.Buffer, 0, e.BytesRecorded);
            }
        };

        _waveIn.RecordingStopped += (s, e) =>
        {
            lock (_writerLock)
            {
                if (_writer is not null)
                {
                    try { _writer.Flush(); } catch { }
                    try { _writer.Dispose(); } catch { }
                    _writer = null;
                }
            }
        };

        _waveIn.StartRecording();
        IsRecording = true;
        DurationSeconds = 0;
        _startTime = DateTime.UtcNow;

        _ticker = new System.Windows.Forms.Timer { Interval = 1000 };
        _ticker.Tick += (s, e) =>
        {
            DurationSeconds = (int)(DateTime.UtcNow - _startTime).TotalSeconds;
            OnRecordingTick?.Invoke(DurationSeconds);
        };
        _ticker.Start();

        return _outputPath;
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Stops recording and returns (filePath, durationSeconds).</summary>
    public (string FilePath, int Duration) StopRecording()
    {
        _ticker?.Stop();
        _ticker?.Dispose();
        _ticker = null;

        if (_waveIn is not null)
        {
            try
            {
                _waveIn.StopRecording();
            }
            catch { }
            try
            {
                _waveIn.Dispose();
            }
            catch { }
            _waveIn = null;
        }

        lock (_writerLock)
        {
            if (_writer is not null)
            {
                try
                {
                    _writer.Flush();
                }
                catch { }
                try
                {
                    _writer.Dispose();
                }
                catch { }
                _writer = null;
            }
        }

        DurationSeconds = (int)(DateTime.UtcNow - _startTime).TotalSeconds;
        IsRecording = false;
        return (_outputPath, DurationSeconds);
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Plays back the recorded file for preview.</summary>
    public void PlayPreview(string filePath)
    {
        StopPlayback();
        _reader = new AudioFileReader(filePath);
        _waveOut = new WaveOutEvent();
        _waveOut.Init(_reader);
        _waveOut.PlaybackStopped += (s, e) =>
        {
            IsPlaying = false;
            OnPlaybackStopped?.Invoke();
        };
        _waveOut.Play();
        IsPlaying = true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Plays back a WAV file from a byte array (e.g., downloaded voice message).</summary>
    public void PlayFromBytes(byte[] data)
    {
        StopPlayback();
        MemoryStream ms = new(data);
        WaveFileReader wfr = new(ms);
        _waveOut = new WaveOutEvent();
        _waveOut.Init(wfr);
        _waveOut.PlaybackStopped += (s, e) =>
        {
            IsPlaying = false;
            OnPlaybackStopped?.Invoke();
            wfr.Dispose();
            ms.Dispose();
        };
        _waveOut.Play();
        IsPlaying = true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void StopPlayback()
    {
        _waveOut?.Stop();
        _waveOut?.Dispose();
        _waveOut = null;
        _reader?.Dispose();
        _reader = null;
        IsPlaying = false;
    }

    public void Cleanup()
    {
        try
        {
            if (!string.IsNullOrEmpty(_outputPath) && File.Exists(_outputPath))
                File.Delete(_outputPath);
        }
        catch { }
    }

    public void Dispose()
    {
        if (IsRecording) StopRecording();
        StopPlayback();
        _ticker?.Dispose();
        Cleanup();
    }
}
