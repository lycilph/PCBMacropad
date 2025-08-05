namespace MacropadConfigurator.Services;

public class OverlayService
{
    public event EventHandler OverlayVisibilityChanged = (_,_) => { };

    private bool isOverlayVisible = false;
    public bool IsOverlayVisible
    {
        get => isOverlayVisible;
        set
        {
            if (isOverlayVisible != value)
            {
                isOverlayVisible = value;
                OnOverlayVisibilityChanged();
            }
        }
    }

    public void ShowOverlay() => IsOverlayVisible = true;
    public void HideOverlay() => IsOverlayVisible = false;
    public void ToggleOverlay() => IsOverlayVisible = !IsOverlayVisible;

    private void OnOverlayVisibilityChanged() => OverlayVisibilityChanged?.Invoke(this, EventArgs.Empty);
}
