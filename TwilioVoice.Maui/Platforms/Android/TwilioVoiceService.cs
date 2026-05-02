using Android.App;
using Android.Content;
using Android.OS;
using Android.Telecom;
using Android.App;
using Android.Content;
using System.Collections.Generic;
using TwilioVoice;
using static TwilioVoice.Call;

namespace TwilioVoice.Maui.Platforms.Android;

public class TwilioVoiceService : Java.Lang.Object, ITwilioVoiceService, Call.IListener, IMessageListener, IRegistrationListener
{
    Call? _activeCall;
    CallInvite? _pendingInvite;
    string? _cachedToken;
    string? _cachedFcmToken;

    public bool IsInitialized { get; private set; }

    public event Action<string>? CallStarted;
    public event Action<string>? CallRinging;
    public event Action<string>? CallConnected;
    public event Action<string>? CallEnded;
    public event Action<string>? IncomingCallReceived;
    public event Action<string>? IncomingCallAnswered;

    public void Initialize()
    {
        if (IsInitialized) return;
        Voice.LogLevel = LogLevel.Debug;
        IsInitialized = true;
    }

    public async Task SetupVoiceAsync()
    {
        var token = await FetchTokenAsync();
        if (token == null) return;
        _cachedToken = token;
        if (_cachedFcmToken != null)
            RegisterWithTokens(_cachedToken, _cachedFcmToken);
    }

    public void SetFcmToken(string fcmToken)
    {
        _cachedFcmToken = fcmToken;
        if (_cachedToken != null)
            RegisterWithTokens(_cachedToken, fcmToken);
    }

    void RegisterWithTokens(string accessToken, string fcmToken)
    {
        Voice.Register(accessToken, Voice.RegistrationChannel.Fcm, fcmToken, this);
    }

    public void MakeCall(string to)
    {
        if (_cachedToken == null) return;
        CallStarted?.Invoke(to);

        _activeCall = Voice.Connect(global::Android.App.Application.Context, _cachedToken, this);
    }

    public void HangUp()
    {
        RejectIncomingCall();
        _activeCall?.Disconnect();
        _activeCall = null;
    }

    public void SetMuted(bool muted)
    {
        _activeCall?.Mute(muted);
    }

    public void AcceptIncomingCall()
    {
        if (_pendingInvite == null) return;
        IncomingCallAnswered?.Invoke("accepted");
        _activeCall = _pendingInvite.Accept(global::Android.App.Application.Context, null!, this);
        _pendingInvite = null;
    }

    public void RejectIncomingCall()
    {
        _pendingInvite?.Reject(global::Android.App.Application.Context);
        _pendingInvite = null;
        CallEnded?.Invoke("rejected");
    }

    public bool HandleMessage(IDictionary<string, string> data)
    {
        if (data.TryGetValue("twi_message_type", out var type) && type == "voice.invite")
        {
            Voice.HandleMessage(global::Android.App.Application.Context, data, this);
            return true;
        }
        return false;
    }

    public void OnRinging(Call call) => CallRinging?.Invoke(call.To ?? "");
    public void OnConnected(Call call) { _activeCall = call; CallConnected?.Invoke(call.To ?? ""); }
    public void OnReconnecting(Call call, CallException? exception) { }
    public void OnReconnected(Call call) { }
    public void OnDisconnected(Call call, CallException? exception) { _activeCall = null; CallEnded?.Invoke(call.To ?? ""); }
    public void OnConnectFailure(Call call, CallException exception) { _activeCall = null; CallEnded?.Invoke(call.To ?? ""); }
    public void OnCallQualityWarningsChanged(Call call, IList<Call.CallQualityWarning> currentWarnings, IList<Call.CallQualityWarning>? previousWarnings) { }

    public void OnCallInvite(CallInvite callInvite)
    {
        _pendingInvite = callInvite;
        IncomingCallReceived?.Invoke(callInvite.From ?? "");
    }

    public void OnCancelledCallInvite(CancelledCallInvite cancelledCallInvite, CallException? exception)
    {
        _pendingInvite = null;
        CallEnded?.Invoke("cancelled");
    }

    public void OnRegistered(string accessToken, string fcmToken) { }
    public void OnError(RegistrationException exception, string accessToken, string fcmToken) { }

    static async Task<string?> FetchTokenAsync()
    {
        await Task.CompletedTask;
        return null;
    }
}
