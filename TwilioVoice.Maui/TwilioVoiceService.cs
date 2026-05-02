namespace TwilioVoice.Maui;

public class TwilioVoiceService
{
    private static ITwilioVoiceService? _instance;
    private static readonly object _lock = new();

    public static ITwilioVoiceService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= CreatePlatformService();
                }
            }
            return _instance;
        }
    }

    private static ITwilioVoiceService CreatePlatformService()
    {
#if IOS
        return new Platforms.iOS.TwilioVoiceService();
#elif ANDROID
        return new Platforms.Android.TwilioVoiceService();
#else
        throw new PlatformNotSupportedException("TwilioVoice.Maui supports iOS and Android only.");
#endif
    }
}
