# OAuth Authentication Setup Guide

This guide explains how to set up Google and Microsoft OAuth authentication for the Teaching Assistant API.

## Overview

The Teaching Assistant API supports OAuth authentication via:
- **Google OAuth 2.0**
- **Microsoft Azure AD (Microsoft Account)**
- **GitHub OAuth 2.0**

After successful OAuth authentication, users receive a JWT token that can be used for API authentication.

## Google OAuth Setup

### Step 1: Create Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click **"Select a project"** → **"New Project"**
3. Enter project name: `Teaching Assistant` (or your preferred name)
4. Click **"Create"**

### Step 2: Enable Google+ API

1. In the Google Cloud Console, go to **"APIs & Services"** → **"Library"**
2. Search for **"Google+ API"** or **"Google Identity"**
3. Click **"Enable"**

### Step 3: Create OAuth 2.0 Credentials

1. Go to **"APIs & Services"** → **"Credentials"**
2. Click **"+ CREATE CREDENTIALS"** → **"OAuth client ID"**
3. If prompted, configure the OAuth consent screen:
   - **User Type**: External (or Internal if using Google Workspace)
   - **App name**: Teaching Assistant
   - **User support email**: Your email
   - **Developer contact information**: Your email
   - Click **"Save and Continue"**
   - Add scopes: `email`, `profile`, `openid`
   - Click **"Save and Continue"**
   - Add test users (if in testing mode)
   - Click **"Save and Continue"** → **"Back to Dashboard"**

4. Create OAuth Client ID:
   - **Application type**: Web application
   - **Name**: Teaching Assistant API
   - **Authorized JavaScript origins**:
     - `http://localhost:5000`
     - `http://localhost:5001`
     - `https://localhost:5000`
     - `https://localhost:5001`
     - Add your production URLs when deploying
   - **Authorized redirect URIs**:
     - `http://localhost:5001/api/auth/google-callback`
     - `https://localhost:5001/api/auth/google-callback`
     - Add your production callback URL when deploying
   - Click **"Create"**

5. **Copy the Client ID and Client Secret** - you'll need these for configuration

### Step 4: Configure the API

Add the credentials to your configuration:

**appsettings.Development.json**:
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  }
}
```

**Or via environment variables**:
```bash
Authentication__Google__ClientId=YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com
Authentication__Google__ClientSecret=YOUR_GOOGLE_CLIENT_SECRET
```

## GitHub OAuth Setup

### Step 1: Register OAuth App on GitHub

1. Go to [GitHub Developer Settings](https://github.com/settings/developers)
2. Click **"OAuth Apps"** → **"New OAuth App"**
3. Fill in the form:
   - **Application name**: Teaching Assistant
   - **Homepage URL**: 
     - Development: `http://localhost:5001`
     - Production: Your production URL
   - **Authorization callback URL**:
     - Development: `http://localhost:5001/api/auth/github-callback`
     - Production: `https://yourdomain.com/api/auth/github-callback`
   - Click **"Register application"**

### Step 2: Generate Client Secret

1. After registering, you'll see your **Client ID** (copy this)
2. Click **"Generate a new client secret"**
3. **Copy the secret immediately** - it won't be shown again!
4. Note: GitHub client secrets expire after a period - you'll need to regenerate if expired

### Step 3: Configure Email Access (Important)

GitHub may not provide email addresses if they're private. To ensure email access:

1. In your OAuth app settings, ensure you're requesting the `user:email` scope
2. Users may need to grant email access during authorization
3. The API will use `username@github.local` as a fallback if email is not available

### Step 4: Configure the API

Add the credentials to your configuration:

**appsettings.Development.json**:
```json
{
  "Authentication": {
    "GitHub": {
      "ClientId": "YOUR_GITHUB_CLIENT_ID",
      "ClientSecret": "YOUR_GITHUB_CLIENT_SECRET"
    }
  }
}
```

**Or via environment variables**:
```bash
Authentication__GitHub__ClientId=YOUR_GITHUB_CLIENT_ID
Authentication__GitHub__ClientSecret=YOUR_GITHUB_CLIENT_SECRET
```

## Microsoft OAuth Setup

### Step 1: Register Application in Azure AD

1. Go to [Azure Portal](https://portal.azure.com/)
2. Navigate to **"Azure Active Directory"** → **"App registrations"**
3. Click **"+ New registration"**
4. Fill in the form:
   - **Name**: Teaching Assistant
   - **Supported account types**: 
     - Select **"Accounts in any organizational directory and personal Microsoft accounts"** (for Microsoft accounts)
     - Or **"Single tenant"** if only for your organization
   - **Redirect URI**:
     - Platform: **Web**
     - URI: `http://localhost:5001/api/auth/microsoft-callback`
     - Add production URI when deploying
   - Click **"Register"**

### Step 2: Configure Authentication

1. In your app registration, go to **"Authentication"**
2. Under **"Platform configurations"**, ensure your redirect URI is listed
3. Under **"Implicit grant and hybrid flows"**, enable:
   - ✅ **ID tokens** (used for sign-in flow)
   - ✅ **Access tokens** (optional, if needed)
4. Click **"Save"**

### Step 3: Create Client Secret

1. Go to **"Certificates & secrets"**
2. Click **"+ New client secret"**
3. Enter description: `Teaching Assistant API Secret`
4. Select expiration (recommend 24 months for development)
5. Click **"Add"**
6. **Copy the secret value immediately** - it won't be shown again!

### Step 4: Configure API Permissions (Optional)

1. Go to **"API permissions"**
2. Click **"+ Add a permission"**
3. Select **"Microsoft Graph"** → **"Delegated permissions"**
4. Add permissions:
   - `User.Read` (to read user profile)
   - `email` (to get user email)
   - `profile` (to get user profile)
5. Click **"Add permissions"**
6. Click **"Grant admin consent"** if you have admin rights (for organization accounts)

### Step 5: Configure the API

Add the credentials to your configuration:

**appsettings.Development.json**:
```json
{
  "Authentication": {
    "Microsoft": {
      "ClientId": "YOUR_AZURE_AD_APPLICATION_ID",
      "ClientSecret": "YOUR_AZURE_AD_CLIENT_SECRET"
    }
  }
}
```

**Or via environment variables**:
```bash
Authentication__Microsoft__ClientId=YOUR_AZURE_AD_APPLICATION_ID
Authentication__Microsoft__ClientSecret=YOUR_AZURE_AD_CLIENT_SECRET
```

## Testing OAuth Flow

### Google OAuth Test

1. Start the API: `dotnet run --project TeachingAssistant.API`
2. Navigate to: `http://localhost:5001/api/auth/login/google`
3. You'll be redirected to Google login
4. After authentication, you'll be redirected to `/auth/callback?token=JWT_TOKEN&provider=google`
5. Extract the token from the URL query parameter

### Microsoft OAuth Test

1. Start the API: `dotnet run --project TeachingAssistant.API`
2. Navigate to: `http://localhost:5001/api/auth/login/microsoft`
3. You'll be redirected to Microsoft login
4. After authentication, you'll be redirected to `/auth/callback?token=JWT_TOKEN&provider=microsoft`
5. Extract the token from the URL query parameter

### GitHub OAuth Test

1. Start the API: `dotnet run --project TeachingAssistant.API`
2. Navigate to: `http://localhost:5001/api/auth/login/github`
3. You'll be redirected to GitHub authorization page
4. Authorize the application (grant email access if prompted)
5. After authentication, you'll be redirected to `/auth/callback?token=JWT_TOKEN&provider=github`
6. Extract the token from the URL query parameter

## Using the JWT Token

After OAuth authentication, you'll receive a JWT token. Use it in API requests:

```http
GET /api/students
Authorization: Bearer YOUR_JWT_TOKEN
```

## Frontend Integration

### Blazor WebAssembly Example

```csharp
@inject NavigationManager Navigation

<button @onclick="LoginWithGoogle">Login with Google</button>
<button @onclick="LoginWithMicrosoft">Login with Microsoft</button>
<button @onclick="LoginWithGitHub">Login with GitHub</button>

@code {
    private void LoginWithGoogle()
    {
        Navigation.NavigateTo("http://localhost:5001/api/auth/login/google");
    }
    
    private void LoginWithMicrosoft()
    {
        Navigation.NavigateTo("http://localhost:5001/api/auth/login/microsoft");
    }
    
    private void LoginWithGitHub()
    {
        Navigation.NavigateTo("http://localhost:5001/api/auth/login/github");
    }
}
```

### JavaScript Example

```javascript
function loginWithGoogle() {
    window.location.href = 'http://localhost:5001/api/auth/login/google';
}

function loginWithMicrosoft() {
    window.location.href = 'http://localhost:5001/api/auth/login/microsoft';
}

// Handle callback
const urlParams = new URLSearchParams(window.location.search);
const token = urlParams.get('token');
if (token) {
    localStorage.setItem('authToken', token);
    // Redirect to main app
}
```

## Production Deployment

### Google OAuth

1. Update **Authorized JavaScript origins** in Google Cloud Console:
   - Add your production domain: `https://yourdomain.com`
2. Update **Authorized redirect URIs**:
   - Add: `https://yourdomain.com/api/auth/google-callback`
3. Update configuration with production URLs

### Microsoft OAuth

1. Update **Redirect URIs** in Azure AD app registration:
   - Add: `https://yourdomain.com/api/auth/microsoft-callback`
2. Update configuration with production URLs

### GitHub OAuth

1. Update **Authorization callback URL** in GitHub OAuth App settings:
   - Add: `https://yourdomain.com/api/auth/github-callback`
2. Update configuration with production URLs
3. Regenerate client secret if needed (old secrets expire)

## Security Notes

- **Never commit** OAuth credentials to version control
- Store secrets in:
  - `appsettings.Development.json` (gitignored)
  - Environment variables
  - Azure Key Vault / AWS Secrets Manager (production)
- Use HTTPS in production
- Regularly rotate client secrets
- Monitor OAuth usage in provider dashboards

## Troubleshooting

### Google OAuth Issues

- **"redirect_uri_mismatch"**: Check that redirect URI exactly matches configured URI
- **"invalid_client"**: Verify Client ID and Client Secret are correct
- **"access_denied"**: User denied consent or app not approved

### Microsoft OAuth Issues

- **"AADSTS50011"**: Redirect URI mismatch - check Azure AD configuration
- **"AADSTS700016"**: Application not found - verify Client ID
- **"AADSTS7000215"**: Invalid client secret - verify secret value

### GitHub OAuth Issues

- **"redirect_uri_mismatch"**: Check that callback URL exactly matches configured URL
- **"bad_verification_code"**: Invalid client secret or expired - regenerate secret
- **Email not available**: User's email may be private - API uses `username@github.local` fallback
- **"application_suspended"**: App may be suspended - check GitHub Developer Settings

## Additional Resources

- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [Microsoft Identity Platform Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
- [GitHub OAuth Documentation](https://docs.github.com/en/apps/oauth-apps/building-oauth-apps/authorizing-oauth-apps)
- [ASP.NET Core OAuth Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/)
