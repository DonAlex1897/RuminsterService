# Email Configuration Guide

## Overview
This guide explains how to set up email functionality for Ruminster's account activation and password reset features.

## Configuration

### 1. Update appsettings.json

Add your email provider settings to `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Ruminster",
    "BaseUrl": "https://ruminster.com"
  }
}
```

### 2. Gmail Setup (Recommended for Development)

1. **Enable 2-Factor Authentication** on your Gmail account
2. **Generate an App Password**:
   - Go to Google Account settings
   - Security → 2-Step Verification
   - App passwords → Select app → Mail
   - Copy the generated 16-character password
3. **Use the App Password** in the configuration (not your regular Gmail password)

### 3. Alternative: Brevo (SendinBlue)

For production or if you prefer Brevo:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp-relay.brevo.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "Username": "your-brevo-email@domain.com",
    "Password": "your-brevo-smtp-key",
    "FromEmail": "your-brevo-email@domain.com",
    "FromName": "Ruminster",
    "BaseUrl": "https://ruminster.com"
  }
}
```

### 4. Environment Variables (Production)

For production, use environment variables instead of storing credentials in appsettings.json:

```bash
EmailSettings__Username=your-email@gmail.com
EmailSettings__Password=your-app-password
EmailSettings__FromEmail=your-email@gmail.com
EmailSettings__BaseUrl=https://your-production-domain.com
```

## API Endpoints

The following endpoints are now available:

### 1. Sign Up (with Email Verification)
```
POST /api/auth/signup
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

Response:
```json
{
  "message": "Registration successful! Please check your email to activate your account."
}
```

### 2. Activate Account
```
GET /api/auth/activate?token=your-activation-token
```

Response:
```json
{
  "message": "Account activated successfully! You can now log in."
}
```

### 3. Forgot Password
```
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "john@example.com"
}
```

Response:
```json
{
  "message": "If an account with that email exists, a password reset link has been sent."
}
```

### 4. Reset Password
```
POST /api/auth/reset-password
Content-Type: application/json

{
  "token": "your-reset-token",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

Response:
```json
{
  "message": "Password reset successful! You can now log in with your new password."
}
```

## Security Features

- **Token Expiration**: 
  - Email verification tokens expire in 30 minutes
  - Password reset tokens expire in 15 minutes
- **One-time Use**: Tokens can only be used once
- **Secure Generation**: Tokens use cryptographically secure random generation
- **Email Confirmation Required**: Users must verify their email before they can log in
- **Refresh Token Revocation**: Password reset revokes all existing refresh tokens

## Email Templates

The system includes responsive HTML email templates with:
- Clear call-to-action buttons
- Fallback links for email clients that don't support buttons
- Professional styling
- Security warnings and expiration notices

## Testing

To test the email functionality:

1. Configure your email settings
2. Use a tool like Postman or curl to test the API endpoints
3. Check your email inbox for activation/reset emails
4. Verify that the email links work correctly

## Troubleshooting

### Common Issues:

1. **Gmail "Less Secure Apps"**: Use App Passwords instead
2. **SMTP Connection Issues**: Check firewall and port settings
3. **Email Not Received**: Check spam folder, verify email settings
4. **Token Expired**: Tokens have short lifespans for security

### Debugging:

- Check application logs for email sending errors
- Verify SMTP settings are correct
- Test with a simple email client first
- Ensure BaseUrl is set correctly for link generation
