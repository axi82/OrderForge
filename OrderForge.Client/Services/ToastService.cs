namespace OrderForge.Client.Services;

public sealed class ToastService : IToastService
{
    public const int MaxVisible = 4;
    public static readonly TimeSpan DedupeWindow = TimeSpan.FromSeconds(3);
    public static readonly TimeSpan SuccessDuration = TimeSpan.FromSeconds(4);
    public static readonly TimeSpan ErrorDuration = TimeSpan.FromSeconds(7);

    private readonly object _lock = new();
    private readonly List<ToastInstance> _toasts = new();
    private string? _liveRegionText;

    public event EventHandler? Changed;

    public string? LiveRegionText
    {
        get
        {
            lock (_lock)
                return _liveRegionText;
        }
    }

    public IReadOnlyList<ToastInstance> Toasts
    {
        get
        {
            lock (_lock)
                return _toasts.ToArray();
        }
    }

    public void ShowSuccess(string message, string? title = null) =>
        AddOrRefresh(ToastSeverity.Success, message, title, SuccessDuration);

    public void ShowWarning(string message, string? title = null) =>
        AddOrRefresh(ToastSeverity.Warning, message, title, ErrorDuration);

    public void ShowError(string message, string? title = null) =>
        AddOrRefresh(ToastSeverity.Error, message, title, ErrorDuration);

    public void SetPaused(Guid id, bool paused)
    {
        lock (_lock)
        {
            var toast = _toasts.FirstOrDefault(t => t.Id == id);
            if (toast is null || paused == toast.IsPaused)
                return;

            var now = DateTime.UtcNow;
            if (paused)
            {
                var remaining = toast.DismissAtUtc - now;
                toast.PauseRemaining = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
                toast.IsPaused = true;
            }
            else
            {
                toast.IsPaused = false;
                var resume = toast.PauseRemaining ?? TimeSpan.Zero;
                toast.DismissAtUtc = now + resume;
                toast.PauseRemaining = null;
            }

            RaiseChanged();
        }
    }

    public void Remove(Guid id)
    {
        lock (_lock)
        {
            var removed = _toasts.RemoveAll(t => t.Id == id) > 0;
            if (removed)
                RaiseChanged();
        }
    }

    private void AddOrRefresh(ToastSeverity severity, string message, string? title, TimeSpan duration)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            foreach (var t in _toasts)
            {
                if (t.Severity == severity
                    && t.Message == message
                    && t.Title == title
                    && now - t.CreatedUtc < DedupeWindow)
                {
                    t.CreatedUtc = now;
                    t.DismissAtUtc = now + duration;
                    t.IsPaused = false;
                    t.PauseRemaining = null;
                    SetLiveRegionText(title, message);
                    RaiseChanged();
                    return;
                }
            }

            while (_toasts.Count >= MaxVisible)
                _toasts.RemoveAt(0);

            var toast = new ToastInstance
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                Severity = severity,
                CreatedUtc = now,
                DismissAtUtc = now + duration,
            };
            _toasts.Add(toast);
            SetLiveRegionText(title, message);
            RaiseChanged();
        }
    }

    private void SetLiveRegionText(string? title, string message)
    {
        _liveRegionText = string.IsNullOrEmpty(title)
            ? message
            : $"{title}: {message}";
    }

    private void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
