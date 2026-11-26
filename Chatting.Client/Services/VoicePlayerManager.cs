using NAudio.Wave;

namespace Chatting.Client.Services;

/// <summary>
/// Quản lý phát tin nhắn thoại tập trung bằng NAudio, đồng bộ thanh tiến trình thời gian thực.
/// </summary>
public class VoicePlayerManager : IDisposable
{
    private static VoicePlayerManager? _instance;
    public static VoicePlayerManager Instance => _instance ??= new VoicePlayerManager();

    private WaveOutEvent? _waveOut;
    private AudioFileReader? _reader;
    private System.Windows.Forms.Timer? _progressTimer;

    public Guid? CurrentAttachmentId { get; private set; }
    public bool IsPlaying => _waveOut?.PlaybackState == PlaybackState.Playing;

    public event Action<Guid, float, TimeSpan, TimeSpan>? OnProgress;
    public event Action<Guid, bool>? OnStateChanged;
    public event Action<Guid>? OnStopped;

    public VoicePlayerManager()
    {
        _progressTimer = new System.Windows.Forms.Timer { Interval = 100 };
        _progressTimer.Tick += ProgressTimer_Tick;
    }

    private void ProgressTimer_Tick(object? sender, EventArgs e)
    {
        if (_reader == null || _waveOut == null || CurrentAttachmentId == null)
        {
            _progressTimer?.Stop();
            return;
        }

        try
        {
            var cur = _reader.CurrentTime;
            var tot = _reader.TotalTime;
            float progress = tot.TotalSeconds > 0 ? (float)(cur.TotalSeconds / tot.TotalSeconds) : 0f;
            if (progress > 1f) progress = 1f;

            OnProgress?.Invoke(CurrentAttachmentId.Value, progress, cur, tot);

            if (_waveOut.PlaybackState == PlaybackState.Stopped || cur >= tot)
            {
                Stop();
            }
        }
        catch
        {
            Stop();
        }
    }

    /// <summary>
    /// Bắt đầu hoặc tiếp tục phát tin nhắn thoại từ tệp âm thanh WAV cục bộ.
    /// </summary>
    public void Play(Guid attachmentId, string localWavPath)
    {
        if (CurrentAttachmentId == attachmentId && _waveOut != null)
        {
            if (_waveOut.PlaybackState == PlaybackState.Paused)
            {
                _waveOut.Play();
                _progressTimer?.Start();
                OnStateChanged?.Invoke(attachmentId, true);
                return;
            }
            else if (_waveOut.PlaybackState == PlaybackState.Playing)
            {
                Pause(attachmentId);
                return;
            }
        }

        // Dừng đoạn âm thanh đang phát trước đó (nếu có)
        Stop();

        try
        {
            CurrentAttachmentId = attachmentId;
            _reader = new AudioFileReader(localWavPath);
            _waveOut = new WaveOutEvent();
            _waveOut.Init(_reader);

            _waveOut.PlaybackStopped += (s, e) =>
            {
                if (CurrentAttachmentId == attachmentId)
                {
                    Stop();
                }
            };

            _waveOut.Play();
            _progressTimer?.Start();
            OnStateChanged?.Invoke(attachmentId, true);
        }
        catch (Exception)
        {
            Stop();
            throw;
        }
    }

    public void Pause(Guid attachmentId)
    {
        if (CurrentAttachmentId == attachmentId && _waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
        {
            _waveOut.Pause();
            _progressTimer?.Stop();
            OnStateChanged?.Invoke(attachmentId, false);
        }
    }

    public void Seek(Guid attachmentId, float percent)
    {
        if (CurrentAttachmentId == attachmentId && _reader != null)
        {
            try
            {
                percent = Math.Clamp(percent, 0f, 1f);
                long newPos = (long)(_reader.Length * percent);
                _reader.Position = newPos;
            }
            catch { }
        }
    }

    public void Stop()
    {
        _progressTimer?.Stop();

        Guid? oldId = CurrentAttachmentId;
        CurrentAttachmentId = null;

        try
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _waveOut = null;
        }
        catch { }

        try
        {
            _reader?.Dispose();
            _reader = null;
        }
        catch { }

        if (oldId.HasValue)
        {
            OnStateChanged?.Invoke(oldId.Value, false);
            OnStopped?.Invoke(oldId.Value);
        }
    }

    public void Dispose()
    {
        Stop();
        _progressTimer?.Dispose();
        _progressTimer = null;
    }
}
