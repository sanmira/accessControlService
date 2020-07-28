using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using accessControlService.Models;
using System.Threading.Tasks;
 
namespace accessControlService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly DatabaseContext _context;
        public RequestController(DatabaseContext context)
        {
            _context = context;
        }
 
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }
        [HttpGet("access")]
        public async Task<ActionResult<IEnumerable<AccessRequest>>> GetAllAccessRequests()
        {
            return await _context.AccessRequests.ToListAsync();
        }
 
        // GET api/users/5
        [HttpGet("users/{key}")]
        public async Task<ActionResult<User>> GetUser(string key)
        {
            User user = await _context.Users.FirstOrDefaultAsync(x => x.key == key);
            if (user == null)
            {
                Console.WriteLine("Resource not found");
                return NotFound();
            }
            return new ObjectResult(user);
        }
        [HttpGet("access/{key}")]
        public async Task<ActionResult<IEnumerable<AccessRequest>>> GetAccessRequests(string key)
        {
            Console.WriteLine("Get access requests");
            var accessRequests = await _context.AccessRequests.Where(x => x.user == key).ToListAsync();
            if (accessRequests == null)
            {
                Console.WriteLine("Resource not found");
                return NotFound();
            }
            return accessRequests;
        }
 
        // POST api/users
        [HttpPost("users")]
        public async Task<ActionResult<User>> PostUser([FromForm] User user)
        {
            Console.WriteLine("Post method");
            if (user == null)
            {
                Console.WriteLine("Post method error");
                return BadRequest();
            }
 
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }
 
        // PUT api/users/
        [HttpPut("users")]
        public async Task<ActionResult<User>> PutUser([FromForm] User user)
        {
            Console.WriteLine("Put method");
            if (user == null)
            {
                return BadRequest();
            }
            if (!_context.Users.Any(x => x.key == user.key))
            {
                return NotFound();
            }
 
            _context.Update(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }
 
        // DELETE api/users/5
        [HttpDelete("users/{key}")]
        public async Task<ActionResult<User>> DeleteUser(string key)
        {
            Console.WriteLine("Delete method");
            User user = _context.Users.FirstOrDefault(x => x.key == key);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(user);
        }
        [HttpDelete("access/{id}")]
        public async Task<ActionResult<AccessRequest>> DeleteAccessRequest(int id)
        {
            Console.WriteLine("Delete method");
            AccessRequest accessRequest = _context.AccessRequests.FirstOrDefault(x => x.id == id);
            if (accessRequest == null)
            {
                return NotFound();
            }
            _context.AccessRequests.Remove(accessRequest);
            await _context.SaveChangesAsync();
            return Ok(accessRequest);
        }
    }
}