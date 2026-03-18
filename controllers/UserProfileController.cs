
using HouseRules.Data;
using HouseRules.Models.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HouseRules.Controllers;

[ApiController]
[Route("[controller]")]
public class UserProfileController : ControllerBase
{
    private HouseRulesDbContext _dbContext;

    public UserProfileController(HouseRulesDbContext context)
    {
        _dbContext = context;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_dbContext
        .UserProfiles
        .Include(up => up.IdentityUser)
        .Select(up => new UserProfileDTO
        {
            Id = up.Id,
            FirstName = up.FirstName,
            LastName = up.LastName,
            Address = up.Address,
            IdentityUserId = up.IdentityUserId,
            Email = up.Email,
            UserName = up.UserName
        })
        .ToList());
    }

    [HttpGet("withroles")]
    // [Authorize(Roles = "Admin")]
    public IActionResult GetWithRoles()
    {
        return Ok(_dbContext.UserProfiles
        .Include(up => up.IdentityUser)
        .Select(up => new UserProfileDTO
        {
            Id = up.Id,
            FirstName = up.FirstName,
            LastName = up.LastName,
            Address = up.Address,
            IdentityUserId = up.IdentityUserId,
            Email = up.Email,
            UserName = up.UserName,
            Roles = _dbContext.UserRoles
            .Where(ur => ur.UserId == up.IdentityUserId)
            .Join(
                _dbContext.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name)
                .Where(name => name != null)
                .Select(name => name!).ToList()
        }).ToList()
        );
    }

    [HttpPost("promote/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Promote(string id)
    {
        IdentityRole? role = _dbContext.Roles.SingleOrDefault(r => r.Name == "Admin");
        if (role != null)
        {
            _dbContext.UserRoles.Add(new IdentityUserRole<string>
            {
                RoleId = role.Id,
                UserId = id
            });
            _dbContext.SaveChanges();
            return NoContent();
        }
        return NotFound()
    }

    [HttpPost("demote/{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Demote(string id)
    {
        IdentityRole? role = _dbContext.Roles
            .SingleOrDefault(r => r.Name == "Admin");
        if (role != null)
        {

            IdentityUserRole<string>? userRole = _dbContext
                .UserRoles
                .SingleOrDefault(ur =>
                ur.RoleId == role.Id &&
                ur.UserId == id);

            if (userRole != null)
            {
                _dbContext.UserRoles.Remove(userRole);
                _dbContext.SaveChanges();

                return NoContent();
            }
        }
        return NotFound();
    }

}