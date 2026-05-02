# TwilioVoice.Maui

Cross-platform .NET MAUI library for [Twilio Voice](https://www.twilio.com/voice) SDK. Provides a unified API for making and receiving voice calls on iOS and Android.

## Installation

```
dotnet add package TwilioVoice.Maui
```

Requires .NET 10+ with iOS 15+ or Android 8+ (API 26).

## Setup

You need the Twilio Voice binding projects for your platform:

### iOS

Add the `TwilioVoiceDotnetIos` binding project to your solution, then initialize:

```csharp
TwilioVoice.Maui.TwilioVoiceService.Instance.Initialize();
await TwilioVoice.Maui.TwilioVoiceService.Instance.SetupVoiceAsync();
```

### Android

Add the `TwilioVoiceDotnetAndroid` binding project and Firebase Messaging, then initialize:

```csharp
TwilioVoice.Maui.TwilioVoiceService.Instance.Initialize();
TwilioVoice.Maui.TwilioVoiceService.Instance.SetFcmToken("YOUR_FCM_TOKEN");
await TwilioVoice.Maui.TwilioVoiceService.Instance.SetupVoiceAsync();
```

## Usage

### Make a call

```csharp
TwilioVoice.Maui.TwilioVoiceService.Instance.CallStarted += (to) => Console.WriteLine($"Calling {to}");
TwilioVoice.Maui.TwilioVoiceService.Instance.CallConnected += (to) => Console.WriteLine($"Connected to {to}");
TwilioVoice.Maui.TwilioVoiceService.Instance.CallEnded += (to) => Console.WriteLine($"Call with {to} ended");

TwilioVoice.Maui.TwilioVoiceService.Instance.MakeCall("+1234567890");
```

### Hang up

```csharp
TwilioVoice.Maui.TwilioVoiceService.Instance.HangUp();
```

### Mute/unmute

```csharp
TwilioVoice.Maui.TwilioVoiceService.Instance.SetMuted(true);
```

### Handle incoming calls

```csharp
TwilioVoice.Maui.TwilioVoiceService.Instance.IncomingCallReceived += (from) =>
{
    // Show incoming call UI
    TwilioVoice.Maui.TwilioVoiceService.Instance.AcceptIncomingCall();
    // or TwilioVoice.Maui.TwilioVoiceService.Instance.RejectIncomingCall();
};
```

## Events

| Event | Description |
|-------|-------------|
| `CallStarted` | Outgoing call initiated |
| `CallRinging` | Remote side ringing |
| `CallConnected` | Call established |
| `CallEnded` | Call disconnected or failed |
| `IncomingCallReceived` | Incoming call invite received |
| `IncomingCallAnswered` | Incoming call accepted |

## Requirements

- .NET 10+
- iOS 15+ with PushKit entitlement
- Android 8+ (API 26) with Firebase Cloud Messaging
- Twilio Voice SDK (iOS v6.3+ / Android v6.9+)

---

<p align="center">
  <a href="https://paypal.me/ibrahimelkady1">
    <img src="https://raw.githubusercontent.com/stefan-niedermann/paypal-donate-button/master/paypal-donate-button.png" alt="Donate with PayPal" height="48">
  </a>
  <br>
  <sub>If this project helped you, consider supporting my work ❤️</sub>
</p>
