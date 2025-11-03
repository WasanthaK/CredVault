namespace CredVault.Mobile.Services;

public static class ApiConfiguration
{
    // 🌐 Azure Production - Azure API Management Gateway
    public static class Azure
    {
        public const string ApiGatewayBaseUrl = "https://apim-wallet-dev.azure-api.net";
        public const string SubscriptionKey = "4a47f13f76d54eb999efc2036245ddc2";
        
        // Service base paths through APIM
        public const string WalletBasePath = "/wallet";
        public const string IdentityBasePath = "/identity";
        public const string ConsentBasePath = "/consent";
        public const string PaymentsBasePath = "/payments";
        
        // Full URLs
        public static string WalletBaseUrl => $"{ApiGatewayBaseUrl}{WalletBasePath}";
        public static string IdentityBaseUrl => $"{ApiGatewayBaseUrl}{IdentityBasePath}";
        public static string ConsentBaseUrl => $"{ApiGatewayBaseUrl}{ConsentBasePath}";
        public static string PaymentsBaseUrl => $"{ApiGatewayBaseUrl}{PaymentsBasePath}";
    }
    
    // 🐳 Local Development - Direct Docker microservice access
    public static class Development
    {
        public const string WalletBaseUrl = "http://localhost:7015";
        public const string IdentityBaseUrl = "http://localhost:7001";
        public const string ConsentBaseUrl = "http://localhost:7002";
        public const string PaymentsBaseUrl = "http://localhost:7004";
    }
    
    // 📱 Android Emulator (10.0.2.2 = host machine) - Direct microservice access
    public static class AndroidEmulator
    {
        public const string WalletBaseUrl = "http://10.0.2.2:7015";
        public const string IdentityBaseUrl = "http://10.0.2.2:7001";
        public const string ConsentBaseUrl = "http://10.0.2.2:7002";
        public const string PaymentsBaseUrl = "http://10.0.2.2:7004";
    }
    
    // 🍎 iOS Simulator (use actual IP of dev machine) - Direct microservice access
    // TODO: Update with your development machine's IP address
    public static class IOSSimulator
    {
        public const string WalletBaseUrl = "http://192.168.1.100:7015";
        public const string IdentityBaseUrl = "http://192.168.1.100:7001";
        public const string ConsentBaseUrl = "http://192.168.1.100:7002";
        public const string PaymentsBaseUrl = "http://192.168.1.100:7004";
    }
    
    // 🔧 Environment flag - Set to true to use Azure, false for local development
    public const bool UseAzure = true;

    public static string GetWalletBaseUrl()
    {
        // Use Azure if enabled, otherwise fall back to platform-specific local URLs
        if (UseAzure)
        {
            return Azure.WalletBaseUrl;
        }
        
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
        if (UseAzure)
        {
            return Azure.IdentityBaseUrl;
        }
        
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
        if (UseAzure)
        {
            return Azure.ConsentBaseUrl;
        }
        
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
        if (UseAzure)
        {
            return Azure.PaymentsBaseUrl;
        }
        
#if ANDROID
        return AndroidEmulator.PaymentsBaseUrl;
#elif IOS
        return IOSSimulator.PaymentsBaseUrl;
#else
        return Development.PaymentsBaseUrl;
#endif
    }
    
    // 🔑 Get subscription key for Azure requests
    public static string GetSubscriptionKey()
    {
        return UseAzure ? Azure.SubscriptionKey : string.Empty;
    }
}
