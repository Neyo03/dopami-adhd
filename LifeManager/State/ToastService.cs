namespace LifeManager.State;

public enum ToastType { Success, Error, Warning, Info }

public record ToastMessage(Guid Id, string Message, ToastType Type);

public class ToastService
{
    private readonly List<ToastMessage> _messages = [];
    public IReadOnlyList<ToastMessage> Messages => _messages;

    public event Action? OnChange;

    public void Show(string message, ToastType type = ToastType.Success, int durationMs = 3000)
    {
        var toast = new ToastMessage(Guid.NewGuid(), message, type);
        _messages.Add(toast);
        NotifyStateChanged();

        _ = Task.Run(async () =>
        {
            await Task.Delay(durationMs);
            _messages.Remove(toast);
            NotifyStateChanged();
        });
    }

    public void Dismiss(Guid id)
    {
        _messages.RemoveAll(m => m.Id == id);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
