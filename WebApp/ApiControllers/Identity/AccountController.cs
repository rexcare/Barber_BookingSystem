using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using App.DAL.EF;
using App.Domain.Identity;
using Base.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.DTO;
using WebApp.DTO.Identity;

namespace WebApp.ApiControllers.Identity;

[ApiVersion( "1.0" )]
[Route("api/v{version:apiVersion}/identity/[controller]/[action]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;
    private readonly Random _rnd = new();
    private readonly AppDbContext _context;

    public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
        IConfiguration configuration, ILogger<AccountController> logger, AppDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Login into the rest backend - generates JWT to be included in
    /// Authorize: Bearer xyz
    /// </summary>
    /// <param name="loginData">Supply email and password</param>
    /// <returns>JWT and refresh token</returns>
    [HttpPost]
    public async Task<ActionResult<JwtResponse>> LogIn([FromBody] Login loginData)
    {
        // verify username
        var appUser = await _userManager.FindByEmailAsync(loginData.Email);
        if (appUser == null)
        {
            _logger.LogWarning("WebApi login failed, email {} not found", loginData.Email);
            await Task.Delay(_rnd.Next(100, 1000));
            return NotFound("User/Password problem");
        }

        // verify username and password
        var result = await _signInManager.CheckPasswordSignInAsync(appUser, loginData.Password, false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("WebApi login failed, password problem for user {}", loginData.Email);
            await Task.Delay(_rnd.Next(100, 1000));
            return NotFound("User/Password problem");
        }

        // get claims based user
        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        if (claimsPrincipal == null)
        {
            _logger.LogWarning("Could not get ClaimsPrincipal for user {}", loginData.Email);
            await Task.Delay(_rnd.Next(100, 1000));
            return NotFound("User/Password problem");
        }

        appUser.RefreshTokens = await _context
            .Entry(appUser)
            .Collection(a => a.RefreshTokens!)
            .Query()
            .Where(t => t.AppUserId == appUser.Id)
            .ToListAsync();

        foreach (var userRefreshToken in appUser.RefreshTokens)
        {
            if (userRefreshToken.TokenExpirationDateTime < DateTime.UtcNow &&
                userRefreshToken.PreviousTokenExpirationDateTime < DateTime.UtcNow)
            {
                _context.RefreshTokens.Remove(userRefreshToken);
            }
        }
        
        var refreshToken = new RefreshToken();
        refreshToken.AppUserId = appUser.Id;
        _context.RefreshTokens.Add(refreshToken);

        
        
        // generate jwt
        var jwt = IdentityExtensions.GenerateJwt(
            claimsPrincipal.Claims,
            _configuration["JWT:Key"],
            _configuration["JWT:Issuer"],
            _configuration["JWT:Issuer"],
            DateTime.Now.AddMinutes(_configuration.GetValue<int>("JWT:ExpireInMinutes"))
        );

        var res = new JwtResponse()
        {
            Token = jwt,
            RefreshToken = refreshToken.Token,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
        };

        return Ok(res);
    }


    [HttpPost]
    public async Task<ActionResult<JwtResponse>> Register(Register registrationData)
    {
        // verify user
        var appUser = await _userManager.FindByEmailAsync(registrationData.Email);
        if (appUser != null)
        {
            _logger.LogWarning("User with email {} is already registered", registrationData.Email);
            var errorResponse = new RestApiErrorResponse()
            {
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                Title = "App error",
                Status = HttpStatusCode.BadRequest,
                TraceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            };
            errorResponse.Errors["email"] = new List<string>()
            {
                "Email already registered"
            };
            return BadRequest(errorResponse);
        }

        var refreshToken = new RefreshToken();
        appUser = new AppUser()
        {
            FirstName = registrationData.FirstName,
            LastName = registrationData.LastName,
            Email = registrationData.Email,
            UserName = registrationData.Email,
            RefreshTokens = new List<RefreshToken>()
            {
                refreshToken
            }
        };

        // create user (system will do it)
        var result = await _userManager.CreateAsync(appUser, registrationData.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        result = await _userManager.AddClaimAsync(appUser, new Claim("aspnet.firstname", appUser.FirstName));
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        result = await _userManager.AddClaimAsync(appUser, new Claim("aspnet.lastname", appUser.LastName));
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        result = await _userManager.AddClaimAsync(appUser, new Claim("aspnet.phonenumber", appUser.LastName));
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }


        // get full user from system with fixed data
        appUser = await _userManager.FindByEmailAsync(appUser.Email);
        if (appUser == null)
        {
            _logger.LogWarning("User with email {} is not found after registration", registrationData.Email);
            return BadRequest($"User with email {registrationData.Email} is not found after registration");
        }

        // get claims based user
        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        if (claimsPrincipal == null)
        {
            _logger.LogWarning("Could not get ClaimsPrincipal for user {}", registrationData.Email);
            return NotFound("User/Password problem");
        }

        // generate jwt
        var jwt = IdentityExtensions.GenerateJwt(
            claimsPrincipal.Claims,
            _configuration["JWT:Key"],
            _configuration["JWT:Issuer"],
            _configuration["JWT:Issuer"],
            DateTime.Now.AddMinutes(_configuration.GetValue<int>("JWT:ExpireInMinutes"))
        );

        var res = new JwtResponse()
        {
            Token = jwt,
            RefreshToken = refreshToken.Token,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
        };

        return Ok(res);
    }


    [HttpPost]
    public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenModel refreshTokenModel)
    {

        JwtSecurityToken jwtToken;
        // get user info from jwt
        try
        {
            jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(refreshTokenModel.Jwt);
            if (jwtToken == null)
            {
                return BadRequest("No token");
            }
        }
        catch (Exception e)
        {
            return BadRequest($"Cant parse the token, {e.Message}");
        }
        
        // TODO: validate token signature
        // https://stackoverflow.com/questions/49407749/jwt-token-validation-in-asp-net

        var userEmail = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (userEmail == null)
        {
            return BadRequest("No email in jwt");
        }

        // get user and tokens
        var appUser = await _userManager.FindByEmailAsync(userEmail);
        if (appUser == null)
        {
            return NotFound($"User with email {userEmail} not found");
        }

        // load and compare refresh tokens

        await _context.Entry(appUser).Collection(u => u.RefreshTokens!)
            .Query()
            .Where(x => 
                (x.Token == refreshTokenModel.RefreshToken && x.TokenExpirationDateTime > DateTime.UtcNow  ) ||
                (x.PreviousToken == refreshTokenModel.RefreshToken && x.PreviousTokenExpirationDateTime > DateTime.UtcNow)
                )
            .ToListAsync();

        if (appUser.RefreshTokens == null)
        {
            return Problem("RefreshTokens collection is null");
        }
        if (appUser.RefreshTokens.Count == 0)
        {
            return Problem("RefreshTokens collection is empty, no valid refresh tokens found");
        }
        
        if (appUser.RefreshTokens.Count != 1)
        {
            return Problem("More than one valid refresh token found.");
        }
        
        // generate new jwt
        
        // get claims based user
        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        if (claimsPrincipal == null)
        {
            _logger.LogWarning("Could not get ClaimsPrincipal for user {}", userEmail);
            return NotFound("User/Password problem");
        }

        // generate jwt
        var jwt = IdentityExtensions.GenerateJwt(
            claimsPrincipal.Claims,
            _configuration["JWT:Key"],
            _configuration["JWT:Issuer"],
            _configuration["JWT:Issuer"],
            DateTime.Now.AddMinutes(_configuration.GetValue<int>("JWT:ExpireInMinutes"))
        );

        // make new refresh token, obsolete old ones
        var refreshToken = appUser.RefreshTokens.First();
        if (refreshToken.Token == refreshTokenModel.RefreshToken)
        {
            refreshToken.PreviousToken = refreshToken.Token;
            refreshToken.PreviousTokenExpirationDateTime = DateTime.UtcNow.AddMinutes(1);

            refreshToken.Token = Guid.NewGuid().ToString();
            refreshToken.TokenExpirationDateTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();
        }

        var res = new JwtResponse()
        {
            Token = jwt,
            RefreshToken = refreshToken.Token,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
        };

        return Ok(res);
    }

}