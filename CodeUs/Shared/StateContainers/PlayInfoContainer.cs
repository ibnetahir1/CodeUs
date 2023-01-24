using CodeUs.Shared.Models;

namespace CodeUs.Shared.StateContainers
{
    public class PlayInfoContainer
    {
        public PlayInfo? Value { get; set; }

        public event Action? OnStateChange;

        public void SetValue(PlayInfo value)
        {
            Value = value;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnStateChange?.Invoke();
    }
}
