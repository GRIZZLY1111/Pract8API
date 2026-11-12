using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pract8.Models;
using Microsoft.EntityFrameworkCore;

namespace Pract8.Controllers
{

    [ApiController]
    [Route("api/v1/tasks")]
    [Authorize]
    public class TasksController : Controller
    {
        private readonly TokenDBContext _context;

        public TasksController(TokenDBContext context)
        {
            _context = context;
        }

        public record CreateTaskRequest(string Title, string Description);
        public record UpdateTaskRequest(string Title, string Description, bool IsCompleted);

        [HttpGet]
        public async Task<IActionResult> GetMyTasks()
        {
            var currentLogin = User.Identity?.Name;
            var tasks = await _context.Tasks
                .Where(t => t.OwnerLogin == currentLogin)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            var currentLogin = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");

            if (task.OwnerLogin != currentLogin && !isAdmin)
                return Forbid();

            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
        {
            var currentLogin = User.Identity?.Name!;

            var newTask = new Models.Task
            {
                Title = request.Title,
                Description = request.Description,
                OwnerLogin = currentLogin,
                IsCompleted = false
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, newTask);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            var currentLogin = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");

            if (task.OwnerLogin != currentLogin && !isAdmin)
                return Forbid();

            task.Title = request.Title;
            task.Description = request.Description;
            task.IsCompleted = request.IsCompleted;

            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            var currentLogin = User.Identity?.Name;
            var isAdmin = User.IsInRole("Admin");

            if (task.OwnerLogin != currentLogin && !isAdmin)
                return Forbid();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTasks()
        {
            var allTasks = await _context.Tasks.ToListAsync();
            return Ok(allTasks);
        }
    }
}

