using FluentValidation;
using MediatR;
using MyPortfolio.Application.AdminUsers.Models;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.AdminUsers;

public sealed record GetAdminUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    string? Role = null,
    string? Status = null) : IRequest<Result<AdminUserDirectoryPage>>;

public sealed class GetAdminUsersQueryValidator : AbstractValidator<GetAdminUsersQuery>
{
    public GetAdminUsersQueryValidator()
    {
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Search).MaximumLength(200);
        RuleFor(query => query.Role).Must(value => string.IsNullOrWhiteSpace(value) || value is "All" or "Admin" or "User");
        RuleFor(query => query.Status).Must(value => string.IsNullOrWhiteSpace(value) || value is "All" or "Active" or "Locked");
    }
}

public sealed class GetAdminUsersQueryHandler(IAdminUserManagementService service)
    : IRequestHandler<GetAdminUsersQuery, Result<AdminUserDirectoryPage>>
{
    public Task<Result<AdminUserDirectoryPage>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken) =>
        service.GetDirectoryAsync(request.PageNumber, request.PageSize, request.Search, request.Role, request.Status, cancellationToken);
}

public sealed record CreateAdminUserCommand(CreateAdminUserRequest Request) : IRequest<Result<AdminUserDto>>;
public sealed class CreateAdminUserCommandHandler(IAdminUserManagementService service)
    : IRequestHandler<CreateAdminUserCommand, Result<AdminUserDto>>
{
    public Task<Result<AdminUserDto>> Handle(CreateAdminUserCommand request, CancellationToken cancellationToken) =>
        service.CreateAsync(request.Request, cancellationToken);
}

public sealed record SetAdminUserRoleCommand(string Id, string Role) : IRequest<Result<AdminUserDto>>;
public sealed class SetAdminUserRoleCommandHandler(IAdminUserManagementService service)
    : IRequestHandler<SetAdminUserRoleCommand, Result<AdminUserDto>>
{
    public Task<Result<AdminUserDto>> Handle(SetAdminUserRoleCommand request, CancellationToken cancellationToken) =>
        service.SetRoleAsync(request.Id, request.Role, cancellationToken);
}

public sealed record SetAdminUserLockCommand(string Id, bool Locked, string? CurrentAdminId) : IRequest<Result<AdminUserDto>>;
public sealed class SetAdminUserLockCommandHandler(IAdminUserManagementService service)
    : IRequestHandler<SetAdminUserLockCommand, Result<AdminUserDto>>
{
    public Task<Result<AdminUserDto>> Handle(SetAdminUserLockCommand request, CancellationToken cancellationToken) =>
        service.SetLockAsync(request.Id, request.Locked, request.CurrentAdminId, cancellationToken);
}

public sealed record DeleteAdminUserCommand(string Id, string? CurrentAdminId) : IRequest<Result>;
public sealed class DeleteAdminUserCommandHandler(IAdminUserManagementService service)
    : IRequestHandler<DeleteAdminUserCommand, Result>
{
    public Task<Result> Handle(DeleteAdminUserCommand request, CancellationToken cancellationToken) =>
        service.DeleteAsync(request.Id, request.CurrentAdminId, cancellationToken);
}
