using ContactsAPI.Data;
using ContactsAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ContactsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly ContactsApiDbContext _dbContext;

        public ContactsController(ContactsApiDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        #region Public methods
        /// <summary>
        /// Get all contacts
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Standard")]
        public IActionResult GetContacts()
        {
            try
            {
                return Ok(_dbContext.Contacts.Include(c => c.ContactSkills).ToList());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Get a specific contact
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin, Standard")]
        public IActionResult GetContact([FromRoute] Guid id)
        {
            try
            {
                var contact = _dbContext.Contacts.Include(cs => cs.ContactSkills).SingleOrDefault(c => c.Id == id);
                if (contact == null)
                    return NotFound();

                return Ok(contact);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Add a new contact
        /// </summary>
        /// <param name="addContactRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult AddContact(AddContactRequest addContactRequest)
        {
            try
            {
                if (!IsEmailValid(addContactRequest.Email))
                    return BadRequest("Email format not correct");
                if (!IsPhoneNumberValid(addContactRequest.MobilePhoneNumber))
                    return BadRequest("Mobile phone format not correct. International format starting with \"+\" required");

                var exitsingContact = _dbContext.Contacts.SingleOrDefault(c => c.Firstname == addContactRequest.Firstname
                                                                            && c.Lastname == addContactRequest.Lastname
                                                                            && c.MobilePhoneNumber == addContactRequest.MobilePhoneNumber);
                // If the user try to add an existing contact with same firsname, lastname and mobile phone number, we can just return the existing one.
                if (exitsingContact != null)
                    return Ok(exitsingContact);
                var user = GetLoggedUser();
                if (user == null)
                    return Unauthorized("User logged cannot be found.");

                var contact = new Contact()
                {
                    Firstname = addContactRequest.Firstname,
                    Lastname = addContactRequest.Lastname,
                    Fullname = addContactRequest.Fullname,
                    Address = addContactRequest.Address,
                    Email = addContactRequest.Email,
                    MobilePhoneNumber = addContactRequest.MobilePhoneNumber,
                    UserId = user.Id
                };

                _dbContext.Contacts.Add(contact);
                _dbContext.SaveChanges();
                return Ok(contact);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Update a contact
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateContactRequest"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult UpdateContact([FromRoute] Guid id, UpdateContactRequest updateContactRequest)
        {
            try
            {
                var contact = _dbContext.Contacts.Find(id);
                if (contact != null)
                {
                    var user = GetLoggedUser();
                    if (contact.UserId != user.Id)
                        return Unauthorized("You cannot update another user's contact data");

                    if (!IsEmailValid(updateContactRequest.Email))
                        return BadRequest("Email format not correct");
                    if (!IsPhoneNumberValid(updateContactRequest.MobilePhoneNumber))
                        return BadRequest("Mobile phone format not correct. International format starting with \"+\" required");

                    contact.Firstname = updateContactRequest.Firstname;
                    contact.Lastname = updateContactRequest.Lastname;
                    contact.Fullname = updateContactRequest.Fullname;
                    contact.Address = updateContactRequest.Address;
                    contact.Email = updateContactRequest.Email;
                    contact.MobilePhoneNumber = updateContactRequest.MobilePhoneNumber;

                    _dbContext.Contacts.Update(contact);
                    _dbContext.SaveChanges();

                    return Ok(contact);
                }
                return NotFound();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Delete a contact
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult DeleteContact([FromRoute] Guid id)
        {
            try
            {
                var contact = _dbContext.Contacts.Find(id);
                if (contact != null)
                {
                    var user = GetLoggedUser();
                    if (contact.UserId != user.Id)
                        return Unauthorized("You cannot delete a contact from another user");

                    _dbContext.Contacts.Remove(contact);
                    _dbContext.SaveChanges();
                    return Ok(contact);
                }

                return NotFound();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Add a skill to a contact
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="skillId"></param>
        /// <returns></returns>
        [Route("{contactId:guid}/{skillId:guid}")]
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult AddSkillToContact([FromRoute] Guid contactId, [FromRoute] Guid skillId)
        {
            try
            {
                var contact = _dbContext.Contacts.Find(contactId);
                if (contact == null)
                    return NotFound("Contact has not been found");

                var skill = _dbContext.Skills.Find(skillId);
                if (skill == null)
                    return NotFound("Skill has not been found");

                var contactSkill = new ContactSkill()
                {
                    ContactId = contactId,
                    SkillId = skillId
                };

                _dbContext.ContactSkills.Add(contactSkill);
                _dbContext.SaveChanges();
                return Ok(contactSkill);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Delete a skill to a contact
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="skillId"></param>
        /// <returns></returns>
        [Route("{contactId:guid}/{skillId:guid}")]
        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult DeleteSkillToContact([FromRoute] Guid contactId, [FromRoute] Guid skillId)
        {
            try
            {
                var contact = _dbContext.Contacts.Find(contactId);
                if (contact == null)
                    return NotFound("Contact has not been found");

                var skill = _dbContext.Skills.Find(skillId);
                if (skill == null)
                    return NotFound("Skill has not been found");

                var contactSkill = _dbContext.ContactSkills.SingleOrDefault(cs => cs.ContactId == contactId && cs.SkillId == skillId);
                if (contactSkill == null)
                    return NotFound("The contact don't have this skill");

                _dbContext.ContactSkills.Remove(contactSkill);
                _dbContext.SaveChanges();
                return Ok(contactSkill);
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
                if (user == null)
                    throw new Exception("User logged cannot be found.");
                return user;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Check if email format is valid
        /// </summary>
        /// <param name="emailString"></param>
        /// <returns></returns>
        private static bool IsEmailValid(string emailString)
        {
            return Regex.IsMatch(emailString, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Check if phone number format is valid
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        private static bool IsPhoneNumberValid(string phoneNumber)
        {
            return Regex.IsMatch(phoneNumber, "^\\+?[1-9][0-9]{7,14}$");
        }
        #endregion
    }
}