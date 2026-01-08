namespace VogelWedding.Services;

using System;

public class ToastService : IToastService
{
    public event Action<string, string>? OnToast;

    public void ShowSuccess(string message) => OnToast?.Invoke(message, "success");
    public void ShowError(string message) => OnToast?.Invoke(message, "danger");
    public void ShowInfo(string message) => OnToast?.Invoke(message, "info");
    public void ShowWarning(string message) => OnToast?.Invoke(message, "warning");
}