using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using CoursePlatform.API.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CoursePlatform.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("ThisIsASecretKeyForTestingPurposesOnly123!");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            _controller = new AuthController(_mockUserManager.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task Login_WithAdminRole_ShouldIncludeRoleAndIdClaims()
        {
            // Arrange
            var email = "admin@test.com";
            var password = "Password123!";
            var userId = Guid.NewGuid().ToString();
            var user = new IdentityUser { Id = userId, UserName = email, Email = email };

            _mockUserManager.Setup(u => u.FindByEmailAsync(email)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.CheckPasswordAsync(user, password)).ReturnsAsync(true);
            _mockUserManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await _controller.Login(new LoginModel { Email = email, Password = password });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Use reflection or JObject to parse anonymous type, or strong typed model if possible. 
            // Since it returns anonymous object, we can serialize to JSON and back to a helper class.
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(json, options);
            
            Assert.NotNull(response);
            Assert.NotNull(response.Token);
            
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(response.Token);

            // Verify Claims
            Assert.Contains(token.Claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId);
            Assert.Contains(token.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        public class TokenResponse
        {
            public required string Token { get; set; }
            public DateTime Expiration { get; set; }
        }
    }
}
