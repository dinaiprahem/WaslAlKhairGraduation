namespace WaslAlkhair.Api.Helpers
{
    public class JWTmodel
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double ExpirationInDays { get; set; }
    }
}
