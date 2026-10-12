using MediatR;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Profiles.Models;

namespace MyPortfolio.Application.Profiles.Queries.GetProfile;

public record GetProfileQuery(string? UserId = null, string? QueryUser = null) : IRequest<Result<ProfileDto>>;
