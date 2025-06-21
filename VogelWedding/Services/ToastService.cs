namespace VogelWedding.Services;

using System;

public class ToastService : IToastService
{
    public event Action<string, string>? OnToast;

    public void ShowSuccess(string message) => OnToast?.Invoke(message, "success");
    public void ShowError(string message) => OnToast?.Invoke(message, "danger");
}