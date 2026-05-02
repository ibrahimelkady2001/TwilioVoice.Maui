using CoreFoundation;
using Foundation;
using PushKit;
using Twilio.Voice.iOS;

namespace TwilioVoice.Maui.Platforms.iOS;

public class TwilioVoiceService : NSObject, ITwilioVoiceService, IPKPushRegistryDelegate
{
    TVOCall? _activeCall;
    TVOCallInvite? _pendingInvite;
    PKPushRegistry? _voipRegistry;
    TVODefaultAudioDevice? _audioDevice;
    string? _cachedToken;

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
        TwilioVoiceSDK.LogLevel = TVOLogLevel.Trace;
        _audioDevice = TVODefaultAudioDevice.AudioDevice();
        TwilioVoiceSDK.AudioDevice = _audioDevice;
        RegisterForVoIPPush();
        IsInitialized = true;
    }

    public async Task SetupVoiceAsync()
    {
        var token = await FetchTokenAsync();
        if (token == null) return;
        _cachedToken = token;
    }

    public void MakeCall(string to)
    {
        if (_cachedToken == null) return;
        CallStarted?.Invoke(to);
        _audioDevice!.Enabled = true;

        var connectOptions = TVOConnectOptions.OptionsWithAccessToken(_cachedToken, builder =>
        {
            builder.Params = new NSDictionary<NSString, NSString>(
                (NSString)"To", (NSString)to
            );
        });
        _activeCall = TwilioVoiceSDK.ConnectWithOptions(connectOptions, new CallDelegate(this));
    }

    public void HangUp()
    {
        _pendingInvite?.Reject();
        _pendingInvite = null;
        _activeCall?.Disconnect();
        _activeCall = null;
    }

    public void SetMuted(bool muted)
    {
        if (_activeCall != null)
            _activeCall.Muted = muted;
    }

    public void AcceptIncomingCall()
    {
        if (_pendingInvite == null) return;
        IncomingCallAnswered?.Invoke(_pendingInvite.From ?? "");
        _audioDevice!.Enabled = true;
        _activeCall = _pendingInvite.AcceptWithDelegate(new CallDelegate(this));
        _pendingInvite = null;
    }

    public void RejectIncomingCall()
    {
        _pendingInvite?.Reject();
        _pendingInvite = null;
        CallEnded?.Invoke("rejected");
    }

    void RegisterForVoIPPush()
    {
        _voipRegistry = new PKPushRegistry(DispatchQueue.MainQueue);
        _voipRegistry.Delegate = this;
        _voipRegistry.DesiredPushTypes = new NSSet(new NSObject[] { (NSString)"voip" });
    }

    [Export("pushRegistry:didUpdatePushCredentials:forType:")]
    public void DidUpdatePushCredentials(PKPushRegistry registry, PKPushCredentials credentials, string type)
    {
        if (_cachedToken != null)
            TwilioVoiceSDK.RegisterWithAccessToken(_cachedToken, credentials.Token, null);
    }

    [Export("pushRegistry:didInvalidatePushTokenForType:")]
    public void DidInvalidatePushToken(PKPushRegistry registry, string type)
    {
    }

    [Export("pushRegistry:didReceiveIncomingPushWithPayload:forType:withCompletionHandler:")]
    public void DidReceiveIncomingPush(PKPushRegistry registry, PKPushPayload payload, string type, Action completion)
    {
        if (payload.DictionaryPayload.ContainsKey(new NSString("twi_message_type")))
            TwilioVoiceSDK.HandleNotification(payload.DictionaryPayload, new NotificationDelegate(this), null);
        completion();
    }

    static async Task<string?> FetchTokenAsync()
    {
        await Task.CompletedTask;
        return null;
    }

    class CallDelegate : TVOCallDelegate
    {
        readonly TwilioVoiceService _owner;
        public CallDelegate(TwilioVoiceService owner) => _owner = owner;

        public override void CallDidStartRinging(TVOCall call) => _owner.CallRinging?.Invoke(call.To ?? "");
        public override void CallDidConnect(TVOCall call) => _owner.CallConnected?.Invoke(call.To ?? "");
        public override void CallDidDisconnectWithError(TVOCall call, NSError? error) => _owner.CallEnded?.Invoke(call.To ?? "");
        public override void CallDidFailToConnectWithError(TVOCall call, NSError error) => _owner.CallEnded?.Invoke(call.To ?? "");
    }

    class NotificationDelegate : TVONotificationDelegate
    {
        readonly TwilioVoiceService _owner;
        public NotificationDelegate(TwilioVoiceService owner) => _owner = owner;

        public override void CallInviteReceived(TVOCallInvite callInvite)
        {
            _owner._pendingInvite = callInvite;
            _owner.IncomingCallReceived?.Invoke(callInvite.From ?? "");
        }

        public override void CancelledCallInviteReceived(TVOCancelledCallInvite cancelledCallInvite, NSError? error)
        {
            _owner._pendingInvite = null;
            _owner.CallEnded?.Invoke("cancelled");
        }
    }
}
