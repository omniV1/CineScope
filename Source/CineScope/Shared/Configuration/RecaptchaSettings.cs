namespace CineScope.Shared.Configuration;

public class RecaptchaSettings
{
    public string SiteKey { get; set; }
    public string SecretKey { get; set; }
    public double MinimumScore { get; set; } = 0.5;
} 