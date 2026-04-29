namespace OrderForge.Client.Services;

public enum ToastSeverity
{
    Success,
    Warning,
    Error,
}

public sealed class ToastInstance
{
    public Guid Id { get; init; }
    public string? Title { get; init; }
    public string Message { get; init; } = "";
    public ToastSeverity Severity { get; init; }
    public DateTime CreatedUtc { get; set; }
    public DateTime DismissAtUtc { get; set; }
    public bool IsPaused { get; set; }
    public TimeSpan? PauseRemaining { get; set; }
    public bool ShowCloseButton => Severity != ToastSeverity.Success;
}

public interface IToastService
{
    IReadOnlyList<ToastInstance> Toasts { get; }
    string? LiveRegionText { get; }
    event EventHandler? Changed;
    void ShowSuccess(string message, string? title = null);
    void ShowWarning(string message, string? title = null);
    void ShowError(string message, string? title = null);
    void SetPaused(Guid id, bool paused);
    void Remove(Guid id);
}
