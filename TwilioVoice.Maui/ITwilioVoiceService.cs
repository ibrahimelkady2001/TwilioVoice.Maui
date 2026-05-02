namespace TwilioVoice.Maui;

public interface ITwilioVoiceService
{
    bool IsInitialized { get; }

    event Action<string>? CallStarted;
    event Action<string>? CallRinging;
    event Action<string>? CallConnected;
    event Action<string>? CallEnded;
    event Action<string>? IncomingCallReceived;
    event Action<string>? IncomingCallAnswered;

    void Initialize();
    Task SetupVoiceAsync();
    void MakeCall(string to);
    void HangUp();
    void SetMuted(bool muted);
    void AcceptIncomingCall();
    void RejectIncomingCall();
}
