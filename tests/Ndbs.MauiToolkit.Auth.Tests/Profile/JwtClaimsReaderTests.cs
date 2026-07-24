using System.Text;
using System.Text.Json;
using Ndbs.MauiToolkit.Auth.Profile;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Tests.Profile
{
    public class JwtClaimsReaderTests
    {
        private static string CreateJwt(string payloadJson)
        {
            static string Encode(string value)
                => Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
                    .TrimEnd('=').Replace('+', '-').Replace('/', '_');

            return $"{Encode("{\"alg\":\"none\"}")}.{Encode(payloadJson)}.";
        }

        [Fact]
        public void Read_ValidToken_ReturnsStringClaims()
        {
            var jwt = CreateJwt("{\"sub\":\"123\",\"email\":\"a@b.c\"}");

            var claims = JwtClaimsReader.Read(jwt);

            Assert.Contains(new KeyValuePair<string, string>("sub", "123"), claims);
            Assert.Contains(new KeyValuePair<string, string>("email", "a@b.c"), claims);
        }

        [Fact]
        public void Read_ArrayClaim_ReturnsOnePairPerElement()
        {
            var jwt = CreateJwt("{\"roles\":[\"admin\",\"user\"]}");

            var claims = JwtClaimsReader.Read(jwt);

            Assert.Contains(new KeyValuePair<string, string>("roles", "admin"), claims);
            Assert.Contains(new KeyValuePair<string, string>("roles", "user"), claims);
        }

        [Fact]
        public void Read_NumericClaim_ReturnsRawValue()
        {
            var jwt = CreateJwt("{\"exp\":1700000000}");

            var claims = JwtClaimsReader.Read(jwt);

            Assert.Contains(new KeyValuePair<string, string>("exp", "1700000000"), claims);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not-a-jwt")]
        [InlineData("header.only")]
        public void Read_InvalidToken_ReturnsEmpty(string? jwt)
        {
            Assert.Empty(JwtClaimsReader.Read(jwt));
        }

        [Fact]
        public void Read_MalformedPayloadJson_ReturnsEmpty()
        {
            static string Encode(string value)
                => Convert.ToBase64String(Encoding.UTF8.GetBytes(value)).TrimEnd('=');

            var jwt = $"{Encode("{}")}.{Encode("not json")}.";

            Assert.Empty(JwtClaimsReader.Read(jwt));
        }
    }
}
