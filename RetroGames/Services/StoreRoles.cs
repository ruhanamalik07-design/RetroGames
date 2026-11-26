
using Microsoft.AspNetCore.Identity;

namespace RetroGames.Services;

public static class StoreRoles
{
    public const string Customer = "Customer";
    public const string Administrator = "Administrator";

    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        await using var scope = services.CreateAsyncScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var name in new[] { Customer, Administrator })
        {
            if (!await roles.RoleExistsAsync(name))
            {
                Check(await roles.CreateAsync(new IdentityRole(name)));
            }
        }

        var userId = configuration["StoreRoles:AdministratorUserId"];
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await users.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("The configured administrator user ID was not found.");
        if (!await users.IsInRoleAsync(user, Administrator))
        {
            Check(await users.AddToRoleAsync(user, Administrator));
        }
    }

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }
    }
}
