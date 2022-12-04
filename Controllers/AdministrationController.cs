using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnero.Models;

namespace Turnero.Controllers;

[Authorize(Roles = "Admin")]
public class AdministrationController : Controller
{
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly UserManager<IdentityUser> userManager;

    public ILogger<AdministrationController> Logger { get; }

    public AdministrationController(RoleManager<IdentityRole> roleManager,
                                    UserManager<IdentityUser> userManager,
                                    ILogger<AdministrationController> logger)
    {
        this.roleManager = roleManager;
        this.userManager = userManager;
        this.Logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ManageUserClaims(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if(user == null)
        {
            ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
            return View("NotFound");
        }

        var existingUserClaims = await userManager.GetClaimsAsync(user);
        var model = new UserClaimsViewModel
        {
            UserId = userId
        };

        foreach (Claim claim in ClaimsStore.AllClaims)
        {
            UserClaim userClaim = new UserClaim
            {
                ClaimType = claim.Type
            };
            if(existingUserClaims.Any(c => c.Type == claim.Type))
            {
                userClaim.IsSelected = true;
            }
            model.Claims.Add(userClaim);
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
            return View("NotFound");
        }

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
        if (user == null)
        {
            ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
            return View("NotFound");
        }

        var userClaims = await userManager.GetClaimsAsync(user);
        var userRoles = await userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            Claims = userClaims.Select(c => c.Value).ToList(),
            Roles = userRoles
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
            return View("NotFound");
        }
        else
        {
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
            catch(DbUpdateException ex)
            {
                Logger.LogError($"Error updating user {ex}");
                return View("Error");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);

        if (user == null)
        {
            ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";

            return View("NotFound");
        }
        else
        {
            try
            {
                var result = await userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
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
                Logger.LogError($"Error deleting user {ex}");
                return View("Error");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var role = await roleManager.FindByIdAsync(id);

        if (role == null)
        {
            ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";

            return View("NotFound");
        }
        else
        {
            try
            {
                var result = await roleManager.DeleteAsync(role);

                if (result.Succeeded)
                {
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
                Logger.LogError($"Error deleting role {ex}");
                ViewBag.ErrorTitle = $"{role.Name} role is in use";
                ViewBag.ErrorMessage = $"{role.Name} role cannot be deleted as there are users " +
                    $"in this role. If you want to delete this role, please remove the users from" +
                    $"the role and then try to delete";
                return View("Error");
            }
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

        if (role == null)
        {
            ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
            return View("NotFound");
        }

        var model = new EditRoleViewModel
        {
            Id = role.Id,
            RoleName = role.Name
        };

        foreach (var user in userManager.Users)
        {
            if (await userManager.IsInRoleAsync(user, role.Name))
            {
                model.Users.Add(user.UserName);
            }
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditRole(EditRoleViewModel model)
    {
        var role = await roleManager.FindByIdAsync(model.Id);

        if (role == null)
        {
            ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
            return View("NotFound");
        }
        else
        {
            try
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
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
                Logger.LogError($"Error editing role {ex}");
                return View("Error");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(IdentityRole role)
    {
        await roleManager.CreateAsync(role);
        return RedirectToAction(nameof(ListRoles));
    }

    [HttpGet]
    public async Task<IActionResult> EditUsersInRole(string roleId)
    {
        ViewBag.roleId = roleId;

        var role = await roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
            return View("NotFound");
        }

        var model = new List<UserRoleViewModel>();

        foreach (var user in userManager.Users)
        {
            var userRoleViewModel = new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName
            };
            if (await userManager.IsInRoleAsync(user, role.Name))
            {
                userRoleViewModel.IsSelected = true;
            }
            else
            {
                userRoleViewModel.IsSelected = false;
            }

            model.Add(userRoleViewModel);
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model,string roleId)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
            return View("NotFound");
        }
        foreach (var userRole in model)
        {
            var user = await userManager.FindByIdAsync(userRole.UserId);
            
            if(userRole.IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
            {
                await userManager.AddToRoleAsync(user, role.Name);
            }
            else if (!userRole.IsSelected && await userManager.IsInRoleAsync(user, role.Name))
            {
                await userManager.RemoveFromRoleAsync(user, role.Name);
            }
        }
       return RedirectToAction(nameof(EditRole), new { Id = roleId });
    }
}
