# 🧪 CredVault Testing Guide

**Last Updated**: November 3, 2025

---

## 🚀 Quick Start Testing

Your app is now fully connected to **real Azure APIs**! No more mock data.

### Prerequisites

✅ App is configured to use Azure APIM  
✅ Subscription key is embedded in `ApiConfiguration.cs`  
✅ All endpoints point to: `https://apim-wallet-dev.azure-api.net`

---

## 📱 Testing Flow

### 1. Registration (Sign Up)

**Screen**: RegisterPage  
**API Endpoint**: `POST /identity/api/v1/identity/auth/register`

**Test Data**:
```
Email: test@example.com
Password: Test123!@#
Confirm Password: Test123!@#
```

**Expected Result**:
- ✅ Success message appears
- ✅ Redirects to login page after 2 seconds
- ✅ User created in Azure Identity API

**Possible Errors**:
- "Username or email already exists" - User already registered, try login instead
- "Rate limit exceeded" - Wait 60 seconds (100 calls/minute limit)
- Network error - Check internet connection

---

### 2. Login

**Screen**: LoginPage  
**API Endpoint**: `POST /identity/api/v1/identity/auth/login`

**Test Credentials**:
```
Username/Email: test@example.com
Password: Test123!@#
```

**Expected Result**:
- ✅ Tokens stored in SecureStorage
- ✅ Navigate to Dashboard
- ✅ Greeting shows username: "Good [morning/afternoon/evening], test"

**What Happens Behind the Scenes**:
1. App sends login request with subscription key header
2. Identity API validates credentials
3. Returns access_token, refresh_token, user profile
4. Tokens saved to secure storage
5. Dashboard loads

---

### 3. Dashboard

**Screen**: DashboardPage  
**API Endpoints**: 
- `GET /wallet/api/v1/wallet/Credential/holder/{holderId}` - Load credentials
- `GET /wallet/api/v1/wallet/Wallet/logs` - Load activity

**Expected Result**:
- ✅ Real greeting with username
- ✅ Credentials list (may be empty for new users)
- ✅ Recent Activity section with real logs
- ✅ "No credentials yet" message if no credentials exist

**What's Real Now**:
- ❌ ~~Mock "National ID verified" activity~~
- ✅ **Real activity logs from API**
- ❌ ~~Hardcoded "Alex" username~~
- ✅ **Real username from SecureStorage**

---

## 🔍 Debugging Tips

### Check API Responses

All API calls are logged to debug console. Look for:

```csharp
Debug.WriteLine($"✅ Login successful for user: {Username}");
Debug.WriteLine($"Failed to load credentials: {result.ErrorMessage}");
```

### Common Issues

#### 1. **401 Unauthorized**
- **Cause**: Missing or invalid subscription key
- **Fix**: Check `ApiConfiguration.cs` has correct key: `4a47f13f76d54eb999efc2036245ddc2`

#### 2. **429 Too Many Requests**
- **Cause**: Rate limit exceeded (100 calls/60 seconds)
- **Fix**: Wait 60 seconds or use secondary key

#### 3. **404 Not Found**
- **Cause**: Wrong endpoint path
- **Fix**: Ensure paths include base like `/wallet/api/v1/wallet/`

#### 4. **No credentials shown**
- **Not an error!** New users won't have credentials yet
- **Add credentials**: Use "Add Credential" button → scan QR or manual flow

#### 5. **Empty activity feed**
- **Not an error!** New users won't have activity history
- **Generate activity**: Add/share/verify credentials

---

## 🔐 Environment Configuration

### Current Setup

```csharp
// In ApiConfiguration.cs
public const bool UseAzure = true;  // ✅ Currently using Azure

// If you need to test locally:
public const bool UseAzure = false; // Switch to Docker localhost
```

### Azure Environment
- **Gateway**: `https://apim-wallet-dev.azure-api.net`
- **Subscription Key**: `4a47f13f76d54eb999efc2036245ddc2`
- **Rate Limit**: 100 calls per 60 seconds

### Local Docker Environment (Optional)
- **Wallet API**: `http://localhost:7015`
- **Identity API**: `http://localhost:7001`
- **Consent API**: `http://localhost:7002`
- **No subscription key needed**

---

## 📊 API Call Flow

### Login Flow
```
User enters credentials
    ↓
LoginViewModel.LoginAsync()
    ↓
IIdentityApiClient.LoginAsync() with subscription key
    ↓
Azure APIM Gateway → Identity Service
    ↓
Response with tokens
    ↓
Save to SecureStorage
    ↓
Navigate to Dashboard
```

### Dashboard Load Flow
```
Dashboard appears
    ↓
DashboardViewModel.Initialize()
    ↓
GetUserNameAsync() → Read from SecureStorage
    ↓
WalletService.GetCredentialsAsync() with auth token
    ↓
WalletService.GetActivityLogsAsync() with auth token
    ↓
Display real data
```

---

## 🧪 Test Scenarios

### Scenario 1: New User Registration
1. Open app
2. Click "Sign up" on login page
3. Enter email, password, confirm password
4. Click "Sign Up"
5. **Expected**: Success message → Auto navigate to login

### Scenario 2: Existing User Login
1. Open app
2. Enter registered credentials
3. Click "Log In"
4. **Expected**: Dashboard with greeting and empty state

### Scenario 3: Empty Dashboard
1. Login successfully
2. **Expected**: 
   - Greeting: "Good [time], [username]"
   - "No credentials yet" message
   - "Tap + to add your first credential"
   - Empty activity section or "No recent activity"

### Scenario 4: API Rate Limit
1. Make 100+ API calls rapidly
2. **Expected**: "Too many requests" error
3. Wait 60 seconds
4. **Expected**: API calls work again

---

## 📝 What's Been Removed (No More Mocks!)

❌ **Removed from DashboardViewModel**:
```csharp
// OLD MOCK CODE - REMOVED
RecentActivities.Add(new ActivityInfo
{
    Icon = "✅",
    Description = "National ID verified successfully",
    TimeAgo = "2 hours ago"
});
```

✅ **New Real Code**:
```csharp
// REAL API CALL
var result = await _walletService.GetActivityLogsAsync(page: 1, pageSize: 10);
foreach (var log in result.Data)
{
    RecentActivities.Add(new ActivityInfo
    {
        Icon = GetIconForActivityType(log.Action ?? ""),
        Description = log.Details ?? log.Action ?? "Unknown activity",
        TimeAgo = GetTimeAgo(log.Timestamp)
    });
}
```

---

## 🎯 Next Steps

After successful login and dashboard load:

1. ✅ **Add Credentials**: Test credential issuance flow
2. ✅ **View Credentials**: Test credential details page
3. ✅ **Share Credentials**: Test selective disclosure
4. ✅ **Scan QR**: Test verifier mode
5. ✅ **Settings**: Test profile and security settings

---

## 🆘 Need Help?

### Check Logs
- Visual Studio Output window → Debug output
- Android emulator Logcat (filtered by package name)

### Verify API Health
```powershell
$headers = @{ "Ocp-Apim-Subscription-Key" = "4a47f13f76d54eb999efc2036245ddc2" }
Invoke-RestMethod -Uri "https://apim-wallet-dev.azure-api.net/wallet/health" -Headers $headers
```

### Documentation
- `AZURE_API_ACCESS.md` - Full Azure API documentation
- `API_MAPPING.md` - UI to API endpoint mapping
- `ARCHITECTURE.md` - System architecture overview

---

**Status**: ✅ All systems connected to real Azure APIs  
**Mock Data**: ❌ Removed  
**Ready for Testing**: ✅ Yes
