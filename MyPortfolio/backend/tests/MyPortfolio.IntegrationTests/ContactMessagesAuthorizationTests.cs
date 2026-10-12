using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace MyPortfolio.IntegrationTests;

public class ContactMessagesAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private const string JwtSecret = "MyPortfolio_SuperSecret_Jwt_SigningKey_For_Production_Ready_Auth_2026_Key!";
    private const string JwtIssuer = "MyPortfolioAPI";
    private const string JwtAudience = "MyPortfolioClient";

    public ContactMessagesAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private string GenerateToken(string role, string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task Anonymous_Can_Submit_Contact_Message()
    {
        // Arrange
        var client = _factory.CreateClient();
        var payload = new
        {
            name = "Sarah Recruit",
            email = "sarah@techcompany.com",
            subject = "Staff Engineer Opportunity",
            message = "We would love to discuss a systems architecture role with you."
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/contact", payload);

        // Assert: Anonymous users must be permitted to submit inquiry messages
        Assert.True(
            response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK,
            $"Expected 201 Created or 200 OK, but received {response.StatusCode}");
    }

    [Fact]
    public async Task Anonymous_Cannot_List_Contact_Messages()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/admin/contact-messages");

        // Assert: Anonymous callers must be rejected with 401 Unauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Guest_Cannot_List_Contact_Messages()
    {
        // Arrange
        var client = _factory.CreateClient();
        var guestToken = GenerateToken("Guest", "guest@myportfolio.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", guestToken);

        // Act
        var response = await client.GetAsync("/api/admin/contact-messages");

        // Assert: Guest role must receive 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OrdinaryUser_Can_List_Their_Contact_Messages()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userToken = GenerateToken("User", "standarduser@myportfolio.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        // Act
        var response = await client.GetAsync("/api/admin/contact-messages");

        // Assert: Ordinary portfolio owners (User role) are authorized to view messages sent to their portfolio
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Can_List_Contact_Messages()
    {
        // Arrange
        var client = _factory.CreateClient();
        var adminToken = GenerateToken("Admin", "admin@myportfolio.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Act
        var response = await client.GetAsync("/api/admin/contact-messages");

        // Assert: Admin must be authorized (200 OK)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OrdinaryUser_Passes_Authorization_On_Delete_Contact_Message()
    {
        // Arrange
        var client = _factory.CreateClient();
        var userToken = GenerateToken("User", "standarduser@myportfolio.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        var targetId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/admin/contact-messages/{targetId}");

        // Assert: User must NOT be forbidden (403) or unauthorized (401); returns 404 because random ID doesn't exist for this user
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.NoContent,
            $"Expected 404 or 204, but received {response.StatusCode}");
    }

    [Fact]
    public async Task Guest_Cannot_Delete_Contact_Message()
    {
        // Arrange
        var client = _factory.CreateClient();
        var guestToken = GenerateToken("Guest", "guest@myportfolio.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", guestToken);
        var targetId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/admin/contact-messages/{targetId}");

        // Assert: Guest role must receive 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Passes_Authorization_On_Delete_Contact_Message()
    {
        // Arrange
        var client = _factory.CreateClient();
        var adminToken = GenerateToken("Admin", "admin@myportfolio.local");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/admin/contact-messages/{nonExistentId}");

        // Assert: Admin must NOT be forbidden (403) or unauthorized (401).
        // Since ID is a random Guid, handler returns 404 Not Found (or 204 if found), proving authorization passed!
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.NoContent,
            $"Expected 404 or 204, but received {response.StatusCode}");
    }
}
