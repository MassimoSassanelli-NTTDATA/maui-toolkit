using Ndbs.MauiToolkit.Auth.Configuration;
using Ndbs.MauiToolkit.Auth.Profile;
using Xunit;

namespace Ndbs.MauiToolkit.Auth.Tests.Profile
{
    public class UserProfileFactoryTests
    {
        [Fact]
        public void Create_WithDefaultMappings_MapsStandardClaims()
        {
            var claims = new[]
            {
                new KeyValuePair<string, string>("sub", "user-123"),
                new KeyValuePair<string, string>("given_name", "Ada"),
                new KeyValuePair<string, string>("family_name", "Lovelace"),
                new KeyValuePair<string, string>("email", "ada@example.com"),
            };

            var profile = UserProfileFactory.Create(claims, new OidcClaimMappings());

            Assert.Equal("user-123", profile.Subject);
            Assert.Equal("Ada", profile.GivenName);
            Assert.Equal("Lovelace", profile.FamilyName);
            Assert.Equal("ada@example.com", profile.Email);
        }

        [Fact]
        public void Create_WithCustomMappings_UsesConfiguredClaimTypes()
        {
            var claims = new[]
            {
                new KeyValuePair<string, string>("user_id", "abc"),
                new KeyValuePair<string, string>("mail", "x@y.z"),
            };
            var mappings = new OidcClaimMappings
            {
                SubjectClaim = "user_id",
                EmailClaim = "mail",
            };

            var profile = UserProfileFactory.Create(claims, mappings);

            Assert.Equal("abc", profile.Subject);
            Assert.Equal("x@y.z", profile.Email);
        }

        [Fact]
        public void Create_MissingClaims_LeavesOptionalFieldsNull()
        {
            var claims = new[] { new KeyValuePair<string, string>("sub", "only-sub") };

            var profile = UserProfileFactory.Create(claims, new OidcClaimMappings());

            Assert.Equal("only-sub", profile.Subject);
            Assert.Null(profile.GivenName);
            Assert.Null(profile.Email);
        }

        [Fact]
        public void Create_DuplicateClaimTypes_UsesFirstValue()
        {
            var claims = new[]
            {
                new KeyValuePair<string, string>("sub", "first"),
                new KeyValuePair<string, string>("sub", "second"),
            };

            var profile = UserProfileFactory.Create(claims, new OidcClaimMappings());

            Assert.Equal("first", profile.Subject);
        }
    }
}
