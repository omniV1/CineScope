# CineScope

## Configuration Setup

### Development Environment Setup

1. Copy the template configuration files:
   ```bash
   cp Server/appsettings.template.json Server/appsettings.json
   cp Client/wwwroot/appsettings.template.json Client/wwwroot/appsettings.json
   ```

2. Update the configuration files with your actual values:
   - Server/appsettings.json:
     - `AnthropicSettings:ApiKey`: Your Anthropic API key
     - `JwtSettings:Secret`: A secure random string for JWT signing
     - `MongoDbSettings:ConnectionString`: Your MongoDB connection string
     - `RecaptchaSettings:SecretKey`: Your reCAPTCHA secret key

   - Client/wwwroot/appsettings.json:
     - `AnthropicSettings:ApiKey`: Your Anthropic API key

### Production Environment

For production deployment, use environment variables or a secure configuration management system to provide the following values:

- `ANTHROPIC_API_KEY`
- `JWT_SECRET`
- `MONGODB_CONNECTION_STRING`
- `RECAPTCHA_SECRET_KEY`

### Security Notes

- Never commit sensitive configuration values to version control
- Use different API keys and secrets for development and production
- Regularly rotate secrets and API keys
- Monitor for any accidental commits of sensitive data 