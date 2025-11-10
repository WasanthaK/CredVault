# Manual Deployment Guide - Real API Integration

## What Was Changed

The app has been updated to use **real Azure APIs** instead of mock data:

### Changes Made:
1. ✅ **DashboardViewModel.cs** - Removed all mock data
   - Fetches real activity logs from Azure Wallet API
   - Gets real username from SecureStorage (not hardcoded "Alex")
   
2. ✅ **ApiConfiguration.cs** - Configured Azure endpoints
   - APIM Gateway: `https://apim-wallet-dev.azure-api.net`
   - Subscription Key: `4a47f13f76d54eb999efc2036245ddc2`
   
3. ✅ **AuthHeaderHandler.cs** - Auto-injects subscription key
   - All API calls include the APIM subscription key header

4. ✅ **LoginViewModel.cs** - Fixed build error
   - Removed non-existent interface reference

## Expected Behavior After Deployment

**Before (Mock Data):**
- Username: "Alex" (hardcoded)
- Activity: "National ID verified successfully"
- Activity: "Shared Driver's License"

**After (Real Data):**
- Username: From SecureStorage (empty if not logged in)
- Activity: Empty or real data from Azure API
- All API calls go through Azure APIM gateway

## Manual Deployment Options

### Option 1: Using Visual Studio 2022 (Recommended)

1. Open **Visual Studio 2022**
2. Open `CredVault.sln`
3. Make sure emulator is running and showing home screen
4. Select **Debug** configuration
5. Select your Android emulator from device dropdown
6. Press **F5** or click the green ▶️ Run button
7. Visual Studio will automatically deploy the updated app

### Option 2: Using Command Line (If ADB is working)

```powershell
# 1. Check emulator is online
& "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe" devices
# Should show: emulator-5554   device (not offline)

# 2. Uninstall old version
& "$env:LOCALAPPDATA\Android\Sdk\platform-tools\adb.exe" uninstall com.companyname.credvault.mobile

# 3. Build and deploy
dotnet build src/CredVault.Mobile/CredVault.Mobile.csproj -t:Run -f net9.0-android
```

### Option 3: Build APK and Install Manually

```powershell
# 1. Build the APK
dotnet publish src/CredVault.Mobile/CredVault.Mobile.csproj -f net9.0-android -c Debug

# 2. Find the APK at:
# src\CredVault.Mobile\bin\Debug\net9.0-android\publish\com.companyname.credvault.mobile-Signed.apk

# 3. Drag and drop the APK onto the running emulator window
```

## Troubleshooting

### Emulator Shows "offline"
- Close and restart the emulator
- Wait for home screen to fully load (30-60 seconds)
- Run: `adb kill-server && adb start-server`

### Still Seeing Old Mock Data
- Uninstall the app from emulator: Settings → Apps → CredVault → Uninstall
- Redeploy using one of the options above

### Build Errors
- Run: `dotnet clean src/CredVault.Mobile/CredVault.Mobile.csproj`
- Delete `bin` and `obj` folders
- Rebuild

## Testing the Real API Integration

Once deployed, test:

1. **Login** - Should store token and username in SecureStorage
2. **Dashboard** - Should call Azure API for activity logs
3. **Check Network Traffic** - All requests should go to `apim-wallet-dev.azure-api.net`
4. **Verify Headers** - Should include `Ocp-Apim-Subscription-Key` header

## Code References

### DashboardViewModel - Real API Call
```csharp
private async Task LoadRecentActivities()
{
    var result = await _walletService.GetActivityLogsAsync(page: 1, pageSize: 10);
    // Populates from real API response
}
```

### ApiConfiguration - Azure Setup
```csharp
public static class Azure
{
    public const string ApiGatewayBaseUrl = "https://apim-wallet-dev.azure-api.net";
    public const string SubscriptionKey = "4a47f13f76d54eb999efc2036245ddc2";
}
```

---

**Current Status:** Code is ready and built. Just needs to be deployed to emulator once ADB connection is stable.
