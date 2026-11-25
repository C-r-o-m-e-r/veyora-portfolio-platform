using System.Security.Claims;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Portfolio.Data;
using Portfolio.Models;

namespace Portfolio.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
    Task<PasswordChangeResult> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<CredentialCreateOptions?> MakeCredentialOptionsAsync(CancellationToken cancellationToken = default);
    Task<PasskeyRegistrationResult> MakeCredentialResultAsync(AuthenticatorAttestationRawResponse attestationResponse, CancellationToken cancellationToken = default);
    Task<AssertionOptions?> AssertionOptionsAsync(string? username = null, CancellationToken cancellationToken = default);
    Task<PasskeyLoginResult> MakeAssertionAsync(AuthenticatorAssertionRawResponse assertionResponse, CancellationToken cancellationToken = default);
}

public sealed record PasskeyRegistrationResult(bool Success, string Message);
public sealed record PasswordChangeResult(bool Success, string Message);
public sealed record PasskeyLoginResult(bool Success, string Message);

public sealed class AuthService(
    IHttpContextAccessor httpContextAccessor,
    AppDbContext dbContext,
    IPasswordHasher<string> passwordHasher,
    IFido2 fido2,
    IMemoryCache memoryCache) : IAuthService
{
    private static string RegistrationCacheKey(int userId) => $"fido2-reg:{userId}";
    private static string AssertionCacheKey(string username) => $"fido2-assert:{username}";

    public async Task<bool> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var normalizedUsername = username.Trim();
        var adminUser = await dbContext.AdminUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == normalizedUsername);

        if (adminUser is null)
        {
            return false;
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(
            adminUser.Username,
            adminUser.PasswordHash,
            password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return false;
        }

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return false;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, adminUser.Username),
            new(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
            });

        return true;
    }

    public Task LogoutAsync()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext is null
            ? Task.CompletedTask
            : httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<PasswordChangeResult> ChangePasswordAsync(
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var username = httpContext?.User?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            return new PasswordChangeResult(false, "Not authenticated.");
        }

        var adminUser = await dbContext.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (adminUser is null)
        {
            return new PasswordChangeResult(false, "User not found.");
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(
            adminUser.Username,
            adminUser.PasswordHash,
            currentPassword);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return new PasswordChangeResult(false, "Current password is incorrect.");
        }

        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser.Username, newPassword);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new PasswordChangeResult(true, "Password updated successfully.");
    }

    public async Task<CredentialCreateOptions?> MakeCredentialOptionsAsync(CancellationToken cancellationToken = default)
    {
        var adminUser = await GetCurrentAdminUserAsync(cancellationToken);
        if (adminUser is null)
        {
            return null;
        }

        var existingKeys = await dbContext.UserCredentials
            .AsNoTracking()
            .Where(c => c.UserId == adminUser.Id)
            .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
            .ToListAsync(cancellationToken);

        var fidoUser = new Fido2User
        {
            DisplayName = adminUser.Username,
            Name = adminUser.Username,
            Id = GetUserHandle(adminUser.Id)
        };

        var options = fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = fidoUser,
            ExcludeCredentials = existingKeys,
            AuthenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = true,
                ResidentKey = ResidentKeyRequirement.Required,
                UserVerification = UserVerificationRequirement.Preferred
            },
            AttestationPreference = AttestationConveyancePreference.None
        });

        memoryCache.Set(RegistrationCacheKey(adminUser.Id), options.ToJson(), TimeSpan.FromMinutes(5));
        return options;
    }

    public async Task<PasskeyRegistrationResult> MakeCredentialResultAsync(
        AuthenticatorAttestationRawResponse attestationResponse,
        CancellationToken cancellationToken = default)
    {
        var adminUser = await GetCurrentAdminUserAsync(cancellationToken);
        if (adminUser is null)
        {
            return new PasskeyRegistrationResult(false, "Not authenticated.");
        }

        if (!memoryCache.TryGetValue(RegistrationCacheKey(adminUser.Id), out string? jsonOptions)
            || string.IsNullOrWhiteSpace(jsonOptions))
        {
            return new PasskeyRegistrationResult(false, "Registration session expired.");
        }

        memoryCache.Remove(RegistrationCacheKey(adminUser.Id));
        var options = CredentialCreateOptions.FromJson(jsonOptions);

        RegisteredPublicKeyCredential credential;
        try
        {
            credential = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
            {
                AttestationResponse = attestationResponse,
                OriginalOptions = options,
                IsCredentialIdUniqueToUserCallback = IsCredentialIdUniqueAsync
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            return new PasskeyRegistrationResult(false, ex.Message);
        }

        dbContext.UserCredentials.Add(new UserCredential
        {
            UserId = adminUser.Id,
            CredentialId = credential.Id,
            PublicKey = credential.PublicKey,
            UserHandle = credential.User.Id,
            SignatureCounter = credential.SignCount,
            CredType = "public-key",
            RegDate = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return new PasskeyRegistrationResult(true, "Passkey registered.");
    }

public async Task<AssertionOptions?> AssertionOptionsAsync(string? username = null, CancellationToken cancellationToken = default)
    {
        var existingCredentials = new List<PublicKeyCredentialDescriptor>();
        var cacheKey = "fido2-assert-anon";

        if (!string.IsNullOrWhiteSpace(username))
        {
            var adminUser = await dbContext.AdminUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username.Trim(), cancellationToken);
            if (adminUser != null)
            {
                existingCredentials = await dbContext.UserCredentials.AsNoTracking()
                    .Where(c => c.UserId == adminUser.Id)
                    .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
                    .ToListAsync(cancellationToken);
                cacheKey = AssertionCacheKey(adminUser.Username);
            }
        }

        var args = new GetAssertionOptionsParams
        {
            AllowedCredentials = existingCredentials, // Порожній список запускає магію Apple!
            UserVerification = UserVerificationRequirement.Preferred
        };

        var options = fido2.GetAssertionOptions(args);
        memoryCache.Set(cacheKey, options.ToJson(), TimeSpan.FromMinutes(5));
        return options;
    }

public async Task<PasskeyLoginResult> MakeAssertionAsync(
        AuthenticatorAssertionRawResponse assertionResponse,
        CancellationToken cancellationToken = default)
    {
        var cred = await dbContext.UserCredentials
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CredentialId == assertionResponse.RawId, cancellationToken);

        if (cred is null)
            return new PasskeyLoginResult(false, "Unknown credential.");

        var username = cred.User.Username;
        var cacheKey = AssertionCacheKey(username);

        if (!memoryCache.TryGetValue(cacheKey, out string? jsonOptions))
        {
            if (!memoryCache.TryGetValue("fido2-assert-anon", out jsonOptions))
            {
                return new PasskeyLoginResult(false, "Login session expired.");
            }
            memoryCache.Remove("fido2-assert-anon");
        }
        else
        {
            memoryCache.Remove(cacheKey);
        }
        var options = AssertionOptions.FromJson(jsonOptions!);

        try
        {
            IsUserHandleOwnerOfCredentialIdAsync callback = (args, ct) => Task.FromResult(true);

            // АБСОЛЮТНА РЕФЛЕКСІЯ: Сама конструює об'єкти для будь-якої версії бібліотеки
            var method = fido2.GetType().GetMethods().First(m => m.Name == "MakeAssertionAsync");
            var paramInfos = method.GetParameters();
            var invokeArgs = new object?[paramInfos.Length];

            for (int i = 0; i < paramInfos.Length; i++)
            {
                var pType = paramInfos[i].ParameterType;

                // Якщо це секретний об'єкт з параметрами від fido2 (v4.0.0+)
                if (pType.IsClass && pType.Namespace != null && pType.Namespace.Contains("Fido2"))
                {
                    var paramsObj = Activator.CreateInstance(pType);
                    var props = pType.GetProperties();
                    foreach (var prop in props)
                    {
                        if (prop.PropertyType == typeof(AuthenticatorAssertionRawResponse)) prop.SetValue(paramsObj, assertionResponse);
                        else if (prop.PropertyType == typeof(AssertionOptions)) prop.SetValue(paramsObj, options);
                        else if (prop.PropertyType == typeof(byte[])) prop.SetValue(paramsObj, cred.PublicKey);
                        else if (prop.PropertyType == typeof(uint)) prop.SetValue(paramsObj, cred.SignatureCounter);
                        else if (prop.PropertyType == typeof(IsUserHandleOwnerOfCredentialIdAsync)) prop.SetValue(paramsObj, callback);
                    }
                    invokeArgs[i] = paramsObj;
                }
                // Для старих версій (плоскі параметри)
                else if (pType == typeof(AuthenticatorAssertionRawResponse)) invokeArgs[i] = assertionResponse;
                else if (pType == typeof(AssertionOptions)) invokeArgs[i] = options;
                else if (pType == typeof(byte[])) invokeArgs[i] = cred.PublicKey;
                else if (pType == typeof(uint)) invokeArgs[i] = cred.SignatureCounter;
                else if (pType == typeof(IsUserHandleOwnerOfCredentialIdAsync)) invokeArgs[i] = callback;
                else if (pType == typeof(CancellationToken)) invokeArgs[i] = cancellationToken;
                else if (paramInfos[i].HasDefaultValue) invokeArgs[i] = paramInfos[i].DefaultValue;
                else invokeArgs[i] = null;
            }

            // Викликаємо метод
            var task = (Task)method.Invoke(fido2, invokeArgs)!;
            await task.ConfigureAwait(false);

            // Динамічно дістаємо результат без ризику NRE
            dynamic dynTask = task;
            dynamic result = dynTask.Result;

            try { cred.SignatureCounter = (uint)result.SignCount; }
            catch { cred.SignatureCounter = (uint)result.Counter; }

            await dbContext.SaveChangesAsync(cancellationToken);

            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, cred.User.Username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
                    });
            }

            return new PasskeyLoginResult(true, "Passkey login successful.");
        }
        catch (Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return new PasskeyLoginResult(false, msg);
        }
    }

    private async Task<AdminUser?> GetCurrentAdminUserAsync(CancellationToken cancellationToken)
    {
        var username = httpContextAccessor.HttpContext?.User?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return await dbContext.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    private async Task<bool> IsCredentialIdUniqueAsync(IsCredentialIdUniqueToUserParams args, CancellationToken cancellationToken)
    {
        var credentialId = args.CredentialId;
        return !await dbContext.UserCredentials
            .AsNoTracking()
            .AnyAsync(c => c.CredentialId == credentialId, cancellationToken);
    }

    private static byte[] GetUserHandle(int userId) => BitConverter.GetBytes(userId);
}