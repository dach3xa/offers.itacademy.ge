namespace offers.API.Infrastructure.Auth.JWT
{
    public class JWTConfiguration
    {
        public string Secret { get; set; }
        public int ExpirationInMInutes { get; set; }
    }
}
