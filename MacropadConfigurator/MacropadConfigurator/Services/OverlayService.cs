namespace MacropadConfigurator.Services;

public class OverlayService
{
    public event EventHandler? OverlayVisibilityChanged;

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

    public bool IsSpinnerVisible { get; set; } = false;

    public void ShowOverlay() => IsOverlayVisible = true;
    public void HideOverlay() => IsOverlayVisible = false;
    public void ToggleOverlay() => IsOverlayVisible = !IsOverlayVisible;

    public void ShowSpinner()
    {
        IsSpinnerVisible = true;
        ShowOverlay();
    }

    public void HideSpinner()
    {
        IsSpinnerVisible = false;
        HideOverlay();
    }

    private void OnOverlayVisibilityChanged() => OverlayVisibilityChanged?.Invoke(this, EventArgs.Empty);
}
