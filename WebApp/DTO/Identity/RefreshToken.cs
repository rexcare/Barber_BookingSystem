namespace WebApp.DTO.Identity;

public class RefreshTokenModel
{
    public string Jwt { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}