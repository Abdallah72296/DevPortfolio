namespace MyPortfolio.Application.Common.Interfaces;

public interface IUserDataCleanupService
{
    Task CleanupUserDataAsync(string userId, CancellationToken cancellationToken = default);
}
