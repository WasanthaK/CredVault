namespace CredVault.Mobile.Services;

public static class ApiConfiguration
{
    // localhost for Windows/Mac development - Direct microservice access
    public static class Development
    {
        public const string WalletBaseUrl = "http://localhost:7015";
        public const string IdentityBaseUrl = "http://localhost:7001";
        public const string ConsentBaseUrl = "http://localhost:7002";
        public const string PaymentsBaseUrl = "http://localhost:7004";
    }
    
    // Android Emulator (10.0.2.2 = host machine) - Direct microservice access
    public static class AndroidEmulator
    {
        public const string WalletBaseUrl = "http://10.0.2.2:7015";
        public const string IdentityBaseUrl = "http://10.0.2.2:7001";
        public const string ConsentBaseUrl = "http://10.0.2.2:7002";
        public const string PaymentsBaseUrl = "http://10.0.2.2:7004";
    }
    
    // iOS Simulator (use actual IP of dev machine) - Direct microservice access
    // TODO: Update with your development machine's IP address
    public static class IOSSimulator
    {
        public const string WalletBaseUrl = "http://192.168.1.100:7015";
        public const string IdentityBaseUrl = "http://192.168.1.100:7001";
        public const string ConsentBaseUrl = "http://192.168.1.100:7002";
        public const string PaymentsBaseUrl = "http://192.168.1.100:7004";
    }

    public static string GetWalletBaseUrl()
    {
#if ANDROID
        return AndroidEmulator.WalletBaseUrl;
#elif IOS
        return IOSSimulator.WalletBaseUrl;
#else
        return Development.WalletBaseUrl;
#endif
    }

    public static string GetIdentityBaseUrl()
    {
#if ANDROID
        return AndroidEmulator.IdentityBaseUrl;
#elif IOS
        return IOSSimulator.IdentityBaseUrl;
#else
        return Development.IdentityBaseUrl;
#endif
    }

    public static string GetConsentBaseUrl()
    {
#if ANDROID
        return AndroidEmulator.ConsentBaseUrl;
#elif IOS
        return IOSSimulator.ConsentBaseUrl;
#else
        return Development.ConsentBaseUrl;
#endif
    }

    public static string GetPaymentsBaseUrl()
    {
#if ANDROID
        return AndroidEmulator.PaymentsBaseUrl;
#elif IOS
        return IOSSimulator.PaymentsBaseUrl;
#else
        return Development.PaymentsBaseUrl;
#endif
    }
}
