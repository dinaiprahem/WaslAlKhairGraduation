# Twilio SMS Integration Guide

## Overview
The SMS service has been implemented using [Twilio](https://www.twilio.com) - a reliable and feature-rich communication platform that supports SMS, MMS, and voice messaging worldwide.

## Why Twilio?
- **Reliable**: Industry-leading uptime and delivery rates
- **Global Coverage**: Works in 180+ countries
- **Feature-Rich**: Supports SMS, MMS, two-way messaging, and delivery receipts
- **Scalable**: From small projects to enterprise-level applications
- **Free Trial**: $15 in free credits to get started

## 🚀 Quick Setup Guide

### Step 1: Create Twilio Account
1. Go to [twilio.com](https://www.twilio.com)
2. Sign up for a free account
3. Verify your phone number
4. Get $15 in free trial credits

### Step 2: Get Your Credentials
1. Go to the [Twilio Console](https://console.twilio.com)
2. Find your **Account SID** and **Auth Token** on the dashboard
3. Note these down - you'll need them for configuration

### Step 3: Get a Phone Number
1. In the Twilio Console, go to **Phone Numbers** > **Manage** > **Buy a number**
2. Choose a phone number that supports SMS
3. Complete the purchase (uses your trial credits)
4. Note down the phone number in E.164 format (e.g., +1234567890)

### Step 4: Configure the Application
Update your `appsettings.json` file:

```json
{
  "Twilio": {
    "AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AuthToken": "your_auth_token_here",
    "FromPhoneNumber": "+1234567890",
    "WebhookUrl": "https://yourapp.com/webhooks/sms",
    "TimeoutSeconds": 30
  }
}
```

**Required Settings:**
- `AccountSid`: Your Twilio Account SID
- `AuthToken`: Your Twilio Auth Token
- `FromPhoneNumber`: Your Twilio phone number in E.164 format

**Optional Settings:**
- `WebhookUrl`: URL to receive delivery receipts and status updates
- `TimeoutSeconds`: API request timeout (default: 30 seconds)

### Step 5: Set Environment Variables (Production)
For production, set these as environment variables instead of in appsettings.json:

```bash
export Twilio__AccountSid="ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
export Twilio__AuthToken="your_auth_token_here"
export Twilio__FromPhoneNumber="+1234567890"
```

## 📱 Available Features

### 1. Send Single SMS
```bash
POST /api/sms/send
```
```json
{
  "to": ["+1234567890"],
  "text": "Hello from Twilio!",
  "messageType": "SMS"
}
```

### 2. Send Bulk SMS
```bash
POST /api/sms/send
```
```json
{
  "to": ["+1234567890", "+1987654321"],
  "text": "Bulk message to multiple recipients",
  "messageType": "SMS"
}
```

### 3. Send MMS with Media
```bash
POST /api/sms/send
```
```json
{
  "to": ["+1234567890"],
  "text": "Check out this image!",
  "mediaUrls": ["https://example.com/image.jpg"],
  "messageType": "MMS"
}
```

### 4. Send Dedication SMS (Legacy Endpoint)
```bash
POST /api/sms/send-dedication
```
```json
{
  "mobileNumber": "+1234567890",
  "body": "Your dedication message here"
}
```

### 5. Send Group MMS
```bash
POST /api/sms/send-group-mms
```
```json
{
  "recipients": ["+1234567890", "+1987654321"],
  "text": "Group message with optional media",
  "mediaUrls": ["https://example.com/image.jpg"],
  "subject": "Optional MMS Subject"
}
```

### 6. Get Received Messages
```bash
GET /api/sms/received?since=2024-01-01T00:00:00Z
```

## 🔧 Configuration Options

### Webhook Configuration
To receive delivery receipts and status updates, configure a webhook:

1. Set up an endpoint in your application to receive webhooks
2. Add the webhook URL to your Twilio settings
3. Twilio will send POST requests to this URL with message status updates

### Error Handling
The service includes comprehensive error handling:
- Invalid phone numbers
- Insufficient account balance
- Network timeouts
- Invalid media URLs
- Rate limiting

### Response Format
All SMS operations return a standardized response:

```json
{
  "isSuccess": true,
  "messageId": "SMxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "status": "sent",
  "sentAt": "2024-01-15T10:30:00Z",
  "to": "+1234567890",
  "from": "+1987654321",
  "messageType": "SMS",
  "providerData": {
    "provider": "Twilio",
    "accountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "numSegments": "1",
    "price": "0.0075",
    "priceUnit": "USD"
  }
}
```

## 💰 Pricing Information

### SMS Pricing (US)
- **Outbound SMS**: $0.0075 per message
- **Inbound SMS**: $0.0075 per message
- **Long Code**: $1.00/month per number

### MMS Pricing (US)
- **Outbound MMS**: $0.02 per message
- **Inbound MMS**: $0.01 per message

### International Rates
- Vary by country
- Check [Twilio's pricing page](https://www.twilio.com/pricing) for details

## 🔒 Security Best Practices

1. **Never commit credentials to version control**
2. **Use environment variables in production**
3. **Implement webhook signature validation**
4. **Use HTTPS for all webhook URLs**
5. **Regularly rotate your Auth Token**
6. **Monitor usage and set up alerts**

## 🚨 Troubleshooting

### Common Issues

1. **"The number is not a valid phone number"**
   - Ensure phone numbers are in E.164 format (+1234567890)

2. **"Insufficient account balance"**
   - Add funds to your Twilio account

3. **"Permission denied"**
   - Check that your AccountSid and AuthToken are correct

4. **"Message delivery failed"**
   - Verify the recipient phone number is valid and can receive SMS

### Testing
Use Twilio's verified caller IDs for testing without charges during trial period.

## 📞 Support

- **Twilio Documentation**: [https://www.twilio.com/docs](https://www.twilio.com/docs)
- **Console**: [https://console.twilio.com](https://console.twilio.com)
- **Support**: Available through Twilio Console

---

**Note**: Remember to comply with SMS regulations in your region, including opt-in requirements and message content guidelines.
