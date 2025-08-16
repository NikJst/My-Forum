namespace MyForum.Controllers;
using MyForum.Hubs;
using MyForum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;



// DTO для создания ветки
public class CreateThreadRequest
{public string Title { get; set; } = string.Empty;
}
    // DTO для создания поста
public class CreatePostRequest
{ public string Content { get; set; } = string.Empty;
}


[ApiController]
[Route("api/[controller]")]
public class ThreadsController : ControllerBase
{
    private readonly ForumContext _context;

    public ThreadsController(ForumContext context)
    {
        _context = context;
    }

    // GET: /api/threads
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var threads = await _context.Threads
            .Include(t => t.User)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.CreatedAt,
                Author = t.User.Username
            })
            .ToListAsync();

        return Ok(threads);
    }

    // POST: /api/threads (только для авторизованных)
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateThreadRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var thread = new ForumThread
        {
            Title = request.Title,
            UserId = userId
        };

        _context.Threads.Add(thread);
        await _context.SaveChangesAsync();

        return Ok(new { thread.Id, thread.Title, thread.CreatedAt });
    }

        // DELETE: /api/threads/{id}
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var thread = await _context.Threads
            .Include(t => t.Posts) // Если хотите также удалить посты
            .FirstOrDefaultAsync(t => t.Id == id);

        if (thread == null)
            return NotFound(new { message = "Ветка не найдена." });

        // Можно добавить проверку, что удаляет автор или админ

        _context.Threads.Remove(thread);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Ветка успешно удалена." });
    }
}


[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly ForumContext _context;
    private readonly IHubContext<ForumHub> _hub;
    //Это интерфейс, через который можно из контроллера или другого сервиса 
    // управлять соединениями, группами, рассылками
    //не находясь внутри самого хаба (не в ForumHub : Hub)

    //====> SIGNAL IR
    public PostsController(ForumContext context, IHubContext<ForumHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    // GET: /api/posts/thread/{threadId}
    [HttpGet("thread/{threadId}")]
    public async Task<IActionResult> GetByThread(int threadId)
    {
        var posts = await _context.Posts
        .Where(p => p.ThreadId == threadId)
        .OrderByDescending(p => p.CreatedAt)
        .Take(50)
        .OrderBy(p => p.CreatedAt) // чтобы в браузере были от старых к новым
        .Select(p => new {
            Username = p.User.Username,
            p.Content
        })
        .ToListAsync();

        return Ok(posts);
    }

    // POST: /api/posts/thread/{threadId} (только для авторизованных)
    [Authorize]
    [HttpPost("thread/{threadId}")]
    public async Task<IActionResult> Create(int threadId, [FromBody] CreatePostRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


        //====
        // Проверим, существует ли такая тема
        var threadExists = await _context.Threads.AnyAsync(t => t.Id == threadId);
        if (!threadExists)
            return NotFound("Тема не найдена.");
        var post = new Post
        {
            Content = request.Content,
            ThreadId = threadId,
            UserId = userId
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        //===> Отправляем сообщение в SignalR
        // Отправляем сообщение всем клиентам, подписанным на эту тему уже после JoinThread
        // Используем _hub для доступа к IHubContext<ForumHub>
        await _hub.Clients
                .Group($"thread_{threadId}")
                .SendAsync("ReceivePost", new
                {
                    post.Id,
                    post.Content,
                    post.CreatedAt,
                    Author = User.Identity?.Name
                });
        return Ok(new { post.Id, post.Content, post.CreatedAt });
    }





}