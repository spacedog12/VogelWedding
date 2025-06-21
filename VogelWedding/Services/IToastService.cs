namespace VogelWedding.Services;

public interface IToastService
{
    event Action<string, string>? OnToast;
    void ShowSuccess(string message);
    void ShowError(string message);
}