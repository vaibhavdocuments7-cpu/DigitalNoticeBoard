using DigitalNoticeBoard.Application.Abstractions;

namespace DigitalNoticeBoard.Application.Authentication;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordService passwordService,
    ITokenService tokenService)
{
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await userRepository.GetByUsernameAsync(
            request.Username.Trim(),
            cancellationToken);

        if (user is null || !user.IsActive || !passwordService.Verify(user, request.Password))
        {
            return null;
        }

        var token = tokenService.Create(user);
        return new LoginResponse(
            token.Token,
            token.ExpiresAtUtc,
            new UserDto(user.Id, user.Username, user.DisplayName, user.Role));
    }
}
