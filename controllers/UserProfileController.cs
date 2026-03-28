
using HouseRules.Data;
using HouseRules.Models;
using HouseRules.Models.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

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

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetUserProfile(int id)
    {
        UserProfile? user = _dbContext.UserProfiles
        .Include(u => u.ChoreAssignments)
            .ThenInclude(ca => ca.Chore)
        .Include(u => u.ChoreCompletions)
            .ThenInclude(cc => cc.Chore)
        .FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserProfileDTO
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Address = user.Address,
            UserName = user.UserName,
            Email = user.Email,
            IdentityUserId = user.IdentityUserId,
            ChoreAssignments = user.ChoreAssignments.Select(ca => new ChoreAssignmentDTO
            {
                Id = ca.Id,
                UserProfileId = ca.UserProfileId,
                ChoreId = ca.ChoreId,
                Chore = new ChoreDTO
                {
                    Id = ca.Chore.Id,
                    Name = ca.Chore.Name,
                    Difficulty = ca.Chore.Difficulty,
                    ChoreFrequencyDays = ca.Chore.ChoreFrequencyDays
                }
            }).ToList(),
            ChoreCompletions = user.ChoreCompletions.Select(c => new ChoreCompletionDTO
            {
                Id = c.Id,
                ChoreId = c.ChoreId,
                Chore = new ChoreDTO
                {
                    Id = c.Chore.Id,
                    Name = c.Chore.Name,
                    Difficulty = c.Chore.Difficulty,
                    ChoreFrequencyDays = c.Chore.ChoreFrequencyDays
                }
            }).ToList()
        });


    }

    [HttpGet("withroles")]
    [Authorize(Roles = "Admin")]
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
        return NotFound();
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