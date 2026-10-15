using System.Reflection;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyPortfolio.API.Controllers;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Portfolio.Models;
using MyPortfolio.Application.Portfolio.Queries.GetPublicPortfolio;

namespace MyPortfolio.IntegrationTests;

public class PublicPortfolioOwnerResolutionTests
{
    [Fact]
    public async Task PublicUrl_AlwaysResolvesTheUsernameOwner_RegardlessOfCurrentSession()
    {
        var ownerA = new IdentityUser("abdallah") { Id = "user-a-id" };
        var ownerB = new IdentityUser("ahmed") { Id = "user-b-id" };
        var (mediator, capture) = CreateMediator();
        var controller = CreateController(mediator, ownerA, ownerB);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, ownerB.Id)], "test"))
            }
        };

        var responseA = await controller.GetByUsername("abdallah", CancellationToken.None);
        Assert.IsType<OkObjectResult>(responseA);
        Assert.Equal(ownerA.Id, Assert.IsType<GetPublicPortfolioQuery>(capture.CapturedRequest).UserId);

        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var anonymousResponseA = await controller.GetByUsername("abdallah", CancellationToken.None);
        Assert.IsType<OkObjectResult>(anonymousResponseA);
        Assert.Equal(ownerA.Id, Assert.IsType<GetPublicPortfolioQuery>(capture.CapturedRequest).UserId);

        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, ownerA.Id)], "test"));
        var responseB = await controller.GetByUsername("AHMED", CancellationToken.None);
        Assert.IsType<OkObjectResult>(responseB);
        Assert.Equal(ownerB.Id, Assert.IsType<GetPublicPortfolioQuery>(capture.CapturedRequest).UserId);
    }

    [Fact]
    public async Task UnknownUsernameOrUserIdDoesNotResolveToAnyPortfolio()
    {
        var (mediator, capture) = CreateMediator();
        var controller = CreateController(mediator, new IdentityUser("abdallah") { Id = "user-a-id" });

        var unknownName = await controller.GetByUsername("unknown-user", CancellationToken.None);
        var userIdAsSlug = await controller.GetByUsername("user-a-id", CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(unknownName);
        Assert.IsType<NotFoundObjectResult>(userIdAsSlug);
        Assert.Null(capture.CapturedRequest);
    }

    [Fact]
    public async Task PublicEndpointWithoutOwnerDoesNotChooseAnotherUsersPortfolio()
    {
        var (mediator, capture) = CreateMediator();
        var controller = CreateController(mediator, new IdentityUser("abdallah") { Id = "user-a-id" });

        var response = await controller.Get(null, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(response);
        Assert.Null(capture.CapturedRequest);
    }

    [Fact]
    public void PrivateProfileAndDashboardPreviewActionsDeclareAuthorization()
    {
        var profileReadAuth = typeof(ProfileController).GetMethod(nameof(ProfileController.GetProfile))!
            .GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
        var previewAuth = typeof(PortfolioController).GetMethod(nameof(PortfolioController.GetMobilePreview))!
            .GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

        Assert.NotNull(profileReadAuth);
        Assert.NotNull(previewAuth);
    }

    private static (IMediator Mediator, CapturingMediator Capture) CreateMediator()
    {
        var mediator = DispatchProxy.Create<IMediator, CapturingMediator>();
        var proxy = (CapturingMediator)(object)mediator;
        proxy.Response = Result<PortfolioDto>.Success(new PortfolioDto(
            [], [], [], [], [], [], [], new PublicUserProfileDto(
                "Public User", "Title", "Role", "Badge", "Location", "Caption", "public@example.com", "",
                "Summary", "Bio", "", "", "", [], true)));
        return (mediator, proxy);
    }

    private static PortfolioController CreateController(IMediator mediator, params IdentityUser[] users)
    {
        var store = new MemoryUserStore(users);
        var userManager = new UserManager<IdentityUser>(
            store,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<IdentityUser>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<UserManager<IdentityUser>>.Instance);

        return new PortfolioController(mediator, userManager, new TestWebHostEnvironment());
    }

    public class CapturingMediator : DispatchProxy
    {
        public object? CapturedRequest { get; private set; }
        public object? Response { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name != nameof(IMediator.Send) || args is null || args.Length == 0)
                throw new NotSupportedException($"Unexpected mediator call: {targetMethod?.Name}");

            CapturedRequest = args[0];
            var responseType = targetMethod.ReturnType.GenericTypeArguments[0];
            return typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(responseType)
                .Invoke(null, [Response]);
        }
    }

    private sealed class MemoryUserStore : IUserStore<IdentityUser>
    {
        private readonly Dictionary<string, IdentityUser> _users;
        private readonly ILookupNormalizer _normalizer = new UpperInvariantLookupNormalizer();

        public MemoryUserStore(IEnumerable<IdentityUser> users)
        {
            _users = users.ToDictionary(user => _normalizer.NormalizeName(user.UserName!), StringComparer.Ordinal);
        }

        public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);
        public Task<string?> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
        public Task SetUserNameAsync(IdentityUser user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }
        public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
        public Task SetNormalizedUserNameAsync(IdentityUser user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }
        public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
            Task.FromResult(_users.Values.FirstOrDefault(user => user.Id == userId));
        public Task<IdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
            Task.FromResult(_users.GetValueOrDefault(normalizedUserName));
        public void Dispose() { }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "MyPortfolio.IntegrationTests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
