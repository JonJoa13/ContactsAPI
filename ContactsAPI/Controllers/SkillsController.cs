using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ContactsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkillsController : Controller
    {
        private readonly ContactsApiDbContext _dbContext;

        public SkillsController(ContactsApiDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        #region Public methods
        /// <summary>
        /// Get all skills
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Standard")]
        public IActionResult GetSkills()
        {
            try
            {
                return Ok(_dbContext.Skills.ToList());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        ///  Get a specific skill
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Standard")]
        public IActionResult GetSkill([FromRoute] Guid id)
        {
            try
            {
                var skill = _dbContext.Skills.Include(cs => cs.ContactSkills).SingleOrDefault(c => c.Id == id);
                if (skill == null)
                    return NotFound();

                return Ok(skill);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        ///  Add a new skill
        /// </summary>
        /// <param name="addSkillRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult AddSkill(AddSkillRequest addSkillRequest)
        {
            try
            {
                var existingSkill = _dbContext.Skills.SingleOrDefault(s => s.Name == addSkillRequest.Name && s.Level == addSkillRequest.Level);
                // If the user try to add an existing skill with same level, we can just return the existing one.
                if (existingSkill != null)
                    return Ok(existingSkill);

                var skill = new Skill()
                {
                    Name = addSkillRequest.Name,
                    Level = addSkillRequest.Level
                };

                _dbContext.Skills.Add(skill);
                _dbContext.SaveChanges();
                return Ok(skill);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Update a skill
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateSkillRequest"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult UpdateSkill([FromRoute] Guid id, UpdateSkillRequest updateSkillRequest)
        {
            try
            {
                var skill = _dbContext.Skills.Find(id);
                if (skill != null)
                {
                    var contactSkills = _dbContext.ContactSkills.Include(c => c.Contact).Where(cs => cs.SkillId == id);
                    var user = GetLoggedUser();
                    if (contactSkills.Any(cs => cs.Contact.UserId != user.Id))
                        return Unauthorized("You cannot update a skill used by other contacts.");

                    skill.Name = updateSkillRequest.Name;
                    skill.Level = updateSkillRequest.Level;

                    _dbContext.Skills.Update(skill);
                    _dbContext.SaveChanges();

                    return Ok(skill);
                }

                return NotFound();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Delete a skill
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult DeleteSkill([FromRoute] Guid id)
        {
            try
            {
                var skill = _dbContext.Skills.Find(id);
                if (skill != null)
                {
                    var contactSkills = _dbContext.ContactSkills.Include(c => c.Contact).Where(cs => cs.SkillId == id);
                    var user = GetLoggedUser();
                    if (contactSkills.Any(cs => cs.Contact.UserId != user.Id))
                        return Unauthorized("You cannot delete a skill used by other contacts.");

                    _dbContext.Skills.Remove(skill);
                    _dbContext.SaveChanges();
                    return Ok(skill);
                }

                return NotFound();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Get User logged
        /// </summary>
        /// <returns></returns>
        private User GetLoggedUser()
        {
            try
            {
                var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = _dbContext.Users.SingleOrDefault(u => u.Username == userName);
                return user;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion
    }
}
