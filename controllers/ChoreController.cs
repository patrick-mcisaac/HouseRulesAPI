using HouseRules.Data;
using HouseRules.Models;
using HouseRules.Models.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
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
    // [Authorize]
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
    // [Authorize]
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

    // [Authorize]
    [HttpPost("{id}/complete")]
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

    // [Authorize(Roles = "Admin")]
    [HttpPost()]
    public IActionResult NewChore(Chore chore)
    {

        _dbContext.Chores.Add(chore);
        _dbContext.SaveChanges();

        return Created($"/chore/{chore.Id}", chore);
    }

    [HttpPut("{id}")]
    // [Authorize(Roles = "Admin")]
    public IActionResult UpdateChore(int id, ChoreUpdateDTO chore)
    {
        // Could make a ChoreUpdateDTO to ensure only parts i want to update are updated
        Chore? choreToUpdate = _dbContext.Chores.FirstOrDefault(c => c.Id == id);

        if (choreToUpdate == null)
        {
            return NotFound();
        }

        _dbContext.Entry(choreToUpdate).CurrentValues.SetValues(chore);

        _dbContext.SaveChanges();

        return Ok(choreToUpdate);
    }

    [HttpDelete("{id}")]
    // [Authorize(Roles = "Admin")]
    public IActionResult DeleteChore(int id)
    {
        Chore? chore = _dbContext.Chores.FirstOrDefault(c => c.Id == id);

        if (chore == null)
        {
            return NotFound();
        }

        _dbContext.Chores.Remove(chore);
        _dbContext.SaveChanges();

        return NoContent();
    }

    [HttpPost("{id}/assign")]
    // [Authorize(Roles = "Admin")]
    public IActionResult AssignChore(int id, int userId)
    {
        /* Assign a user to a chore */
        ChoreAssignment choreAssignment = new ChoreAssignment
        {
            ChoreId = id,
            UserProfileId = userId
        };

        _dbContext.ChoreAssignments.Add(choreAssignment);
        _dbContext.SaveChanges();
        return NoContent();

    }

    [HttpPost("{id}/unassign")]
    // [Authorize(Roles = "Admin")]
    public IActionResult UnassignChore(int id, int userId)
    {
        ChoreAssignment? choreAssignment = _dbContext.ChoreAssignments.FirstOrDefault(ca => ca.ChoreId == id && ca.UserProfileId == userId);
        if (choreAssignment == null)
        {
            return NotFound();
        }
        _dbContext.ChoreAssignments.Remove(choreAssignment);
        _dbContext.SaveChanges();

        return NoContent();
    }

}