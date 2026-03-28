using HouseRules.Data;
using HouseRules.Models;
using HouseRules.Models.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
public class ChoreController : ControllerBase
{

    private HouseRulesDbContext _dbContext;

    public ChoreController(HouseRulesDbContext context)
    {
        _dbContext = context;
    }


    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {

        return Ok(_dbContext.Chores.Select(c => new ChoreDTO
        {
            Id = c.Id,
            Name = c.Name,
            Difficulty = c.Difficulty,
            ChoreFrequencyDays = c.ChoreFrequencyDays
        }).ToList());
    }

    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetSingleChore(int id)
    {
        Chore? chore = _dbContext.Chores
            .Include(c => c.ChoreAssignments)
                .ThenInclude(ca => ca.UserProfile)
            .Include(c => c.ChoreCompletions)
            .SingleOrDefault(c => c.Id == id);

        if (chore == null)
        {
            return NotFound();
        }

        return Ok(new ChoreDTO
        {
            Id = chore.Id,
            Name = chore.Name,
            Difficulty = chore.Difficulty,
            ChoreFrequencyDays = chore.ChoreFrequencyDays,
            ChoreAssignments = chore.ChoreAssignments.Select(c => new ChoreAssignmentDTO
            {
                Id = c.Id,
                UserProfileId = c.UserProfileId,
                ChoreId = c.ChoreId,
                UserProfile = new UserProfileDTO
                {
                    Id = c.UserProfile.Id,
                    FirstName = c.UserProfile.FirstName,
                    LastName = c.UserProfile.LastName,
                    Address = c.UserProfile.Address,
                    UserName = c.UserProfile.UserName,
                    IdentityUserId = c.UserProfile.IdentityUserId,
                    Email = c.UserProfile.Email
                }
            }).ToList(),
            ChoreCompletions = chore.ChoreCompletions.Select(cc => new ChoreCompletionDTO
            {
                Id = cc.Id,
                UserProfileId = cc.UserProfileId,
                ChoreId = cc.ChoreId,
                CompletedOn = cc.CompletedOn
            }).ToList()
        });
    }

    [HttpPost("{id}/complete")]
    [Authorize]
    public IActionResult CompleteChore(int id, int userId)
    {
        ChoreCompletion? chore = _dbContext.ChoreCompletions.FirstOrDefault(c => c.ChoreId == id);

        if (chore == null)
        {
            return NotFound();
        }

        chore.UserProfileId = userId;
        chore.CompletedOn = DateTime.Now;

        _dbContext.SaveChanges();

        return NoContent();
    }
}