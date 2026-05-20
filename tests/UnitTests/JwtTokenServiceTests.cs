using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace UnitTests;

public sealed class JwtTokenServiceTests
{
    private static readonly JwtOptions TestJwt = new()
    {
        Key = "unit-test-jwt-signing-key-must-be-at-least-32-bytes",
        Issuer = "UnitTests",
        Audience = "UnitTests.Web",
        AccessTokenExpirationMinutes = 60,
    };

    [Fact]
    public void CreateAccessToken_includes_sub_email_name_and_roles()
    {
        var userId = Guid.NewGuid();
        var svc = new JwtTokenService(Options.Create(TestJwt));
        var (jwt, expiresAt) = svc.CreateAccessToken(
            userId,
            "jane@example.com",
            "Jane",
            ["Teacher", "Admin"]);

        expiresAt.Should().BeAfter(DateTimeOffset.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        token.Issuer.Should().Be(TestJwt.Issuer);
        token.Audiences.Should().Contain(TestJwt.Audience);

        token.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub).Select(c => c.Value).Should().ContainSingle(userId.ToString());
        token.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Email).Select(c => c.Value).Should().ContainSingle("jane@example.com");
        token.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Name).Select(c => c.Value).Should().ContainSingle("Jane");
        token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).Should().BeEquivalentTo(new[] { "Teacher", "Admin" });
    }
}
