namespace Turnero.Controllers;

[Authorize(Roles = RolesConstants.Admin)]
public class AdministrationController(RoleManager<IdentityRole> roleManager,
                                UserManager<IdentityUser> userManager,
                                ApplicationDbContext dbContext,
                                ILogger<AdministrationController> logger,
                                IFirebaseService firebaseService) : TurneroBaseController
{

    public ILogger<AdministrationController> Logger { get; } = logger;

    private IFirebaseService FirebaseService { get; } = firebaseService;

    private async Task<bool> UserHasRoleAsync(IdentityUser user, string roleName)
    {
        string? firebaseRole = null;
        try
        {
            firebaseRole = await FirebaseService.GetRoleAsync(user.Id);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not read Firebase role for user {UserId}; using local role mirror.", user.Id);
        }

        return string.IsNullOrWhiteSpace(firebaseRole)
            ? await userManager.IsInRoleAsync(user, roleName)
            : string.Equals(firebaseRole, roleName, StringComparison.OrdinalIgnoreCase);
    }

    [HttpGet]
    public async Task<IActionResult> ManageUserClaims(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return NotFoundError("User", userId);

        var existingUserClaims = await userManager.GetClaimsAsync(user);
        var model = new UserClaimsViewModel
        {
            UserId = userId
        };

        foreach (Claim claim in ClaimsStore.AllClaims)
        {
            UserClaim userClaim = new()
            {
                ClaimType = claim.Type
            };
            if (existingUserClaims.Any(c => c.Type == claim.Type))
            {
                userClaim.IsSelected = true;
            }
            model.Claims.Add(userClaim);
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFoundError("User", model.UserId);

        var claims = await userManager.GetClaimsAsync(user);
        var result = await userManager.RemoveClaimsAsync(user, claims);

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Cannot remove user existing claims");
            return View(model);
        }

        result = await userManager.AddClaimsAsync(user, model.Claims.Where(c => c.IsSelected).Select(c => new Claim(c.ClaimType, c.ClaimType)));
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Cannot add selected claims to user");
            return View(model);
        }
        return RedirectToAction(nameof(EditUser), new { id = model.UserId });
    }

    [HttpGet]
    public IActionResult ListUsers()
    {
        var users = userManager.Users;
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFoundError("User", id);

        var userClaims = await userManager.GetClaimsAsync(user);
        var userRoles = await userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            Claims = [.. userClaims.Select(c => c.Value)],
            Roles = userRoles
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFoundError("User", model.Id);

        try
        {
            user.Email = model.Email;
            user.UserName = model.UserName;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ListUsers));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError("Error updating user: {Exception}", ex);
            return View("Error");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);

        if (user == null) return NotFoundError("User", id);

        try
        {
            var firebaseUserId = user.Id;
            var result = await userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await FirebaseService.DeleteUserAsync(firebaseUserId);
                return RedirectToAction(nameof(ListUsers));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("ListUsers");
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError("Error deleting user: {Exception}", ex);
            return View("Error");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var role = await roleManager.FindByIdAsync(id);

        if (role == null) return NotFoundError("Role", id);

        try
        {
            var result = await roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                var users = await userManager.Users.ToListAsync();
                foreach (var user in users)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    await FirebaseService.SetRoleAsync(user.Id, roles.FirstOrDefault());
                }
                return RedirectToAction(nameof(ListRoles));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(nameof(ListRoles));
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError("Error deleting role: {Exception}", ex);
            return View("Error", new ErrorViewModel
            {
                ErrorTitle = $"{role.Name} role is in use",
                ErrorMessage = $"{role.Name} role cannot be deleted as there are users " +
                    $"in this role. If you want to delete this role, please remove the users from" +
                    $"the role and then try to delete"
            });
        }
    }

    [HttpGet]
    public IActionResult ListRoles()
    {
        var userRoles = roleManager.Roles;
        return View(userRoles);
    }

    public IActionResult CreateRole()
    {
        return View(new IdentityRole());
    }

    public async Task<IActionResult> EditRole(string id)
    {
        var role = await roleManager.FindByIdAsync(id);

        if (role == null) return NotFoundError("Role", id);

        var model = new EditRoleViewModel
        {
            Id = role.Id,
            RoleName = role.Name
        };

        var usersInRole = await userManager.Users.ToListAsync();
        foreach (var user in usersInRole)
        {
            if (await UserHasRoleAsync(user, role.Name))
            {
                model.Users.Add(user.UserName);
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(EditRoleViewModel model)
    {
        var role = await roleManager.FindByIdAsync(model.Id);

        if (role == null) return NotFoundError("Role", model.Id);

        try
        {
            var users = await userManager.Users.ToListAsync();
            var usersInRole = new List<IdentityUser>();
            foreach (var user in users)
            {
            if (await UserHasRoleAsync(user, role.Name))
                {
                    usersInRole.Add(user);
                }
            }
            role.Name = model.RoleName;
            var result = await roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                foreach (var user in usersInRole)
                {
                    await FirebaseService.SetRoleAsync(user.Id, role.Name);
                }
                return RedirectToAction(nameof(ListRoles));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError("Error editing role: {Exception}", ex);
            return View("Error");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(IdentityRole role)
    {
        await roleManager.CreateAsync(role);
        return RedirectToAction(nameof(ListRoles));
    }

    [HttpGet]
    public async Task<IActionResult> Health()
    {
        var checks = new List<HealthCheckResult>();
        var overallHealthy = true;

        // PostgreSQL
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync();
            checks.Add(new HealthCheckResult
            {
                Name = "PostgreSQL",
                Status = canConnect ? "healthy" : "unhealthy",
                Icon = canConnect ? "bi-database-check" : "bi-database-x",
                Description = canConnect ? "Conexión establecida correctamente" : "No se puede conectar a la base de datos"
            });
            if (!canConnect) overallHealthy = false;
        }
        catch (Exception ex)
        {
            checks.Add(new HealthCheckResult
            {
                Name = "PostgreSQL",
                Status = "unhealthy",
                Icon = "bi-database-x",
                Description = ex.Message
            });
            overallHealthy = false;
        }
        return View(new HealthViewModel
        {
            Checks = checks,
            OverallStatus = overallHealthy ? "healthy" : "unhealthy",
            CheckedAt = DateTime.Now
        });
    }

    [HttpGet]
    public async Task<IActionResult> EditUsersInRole(string roleId)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role == null) return NotFoundError("Role", roleId);

        var model = new List<UserRoleViewModel>();

        var users = await userManager.Users.ToListAsync();
        foreach (var user in users)
        {
            var userRoleViewModel = new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName
            };
                if (await UserHasRoleAsync(user, role.Name))
            {
                userRoleViewModel.IsSelected = true;
            }
            else
            {
                userRoleViewModel.IsSelected = false;
            }

            model.Add(userRoleViewModel);
        }
        return View(new EditUsersInRoleViewModel
        {
            RoleId = roleId,
            Users = model
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUsersInRole(EditUsersInRoleViewModel model)
    {
        var roleId = model.RoleId;
        var role = await roleManager.FindByIdAsync(roleId);
        if (role == null) return NotFoundError("Role", roleId);

        foreach (var userRole in model.Users)
        {
            var user = await userManager.FindByIdAsync(userRole.UserId);
            if (user == null)
            {
                continue;
            }

            var hasRole = await UserHasRoleAsync(user, role.Name);

            if (userRole.IsSelected)
            {
                var currentRoles = await userManager.GetRolesAsync(user);
                if (currentRoles.Count != 1 || !string.Equals(currentRoles[0], role.Name, StringComparison.OrdinalIgnoreCase))
                {
                    await userManager.RemoveFromRolesAsync(user, currentRoles);
                    await userManager.AddToRoleAsync(user, role.Name);
                }
                await FirebaseService.SetRoleAsync(user.Id, role.Name);
            }
            else if (!userRole.IsSelected && hasRole)
            {
                await userManager.RemoveFromRoleAsync(user, role.Name);
                var remainingRole = (await userManager.GetRolesAsync(user)).FirstOrDefault();
                await FirebaseService.SetRoleAsync(user.Id, remainingRole);
            }
        }
        return RedirectToAction(nameof(EditRole), new { Id = roleId });
    }
}
