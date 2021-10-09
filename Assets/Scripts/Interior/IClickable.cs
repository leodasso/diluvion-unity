
/// <summary>
/// Interface for interior stuff that's interactible. Don't use this for UI!
/// </summary>
public interface IClickable {

    void OnClick();

    void OnRelease();

    void OnPointerEnter();

    void OnPointerExit();

    void OnFocus();

    void OnDefocus();
}
