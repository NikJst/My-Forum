using MyForum.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace MyForum.Hubs;

public class ForumHub : Hub
{
    private readonly ForumContext _context;
    public ForumHub(ForumContext context)
    {
        _context = context;
    }

    // Клиент может присоединиться к конкретной теме (группе)
    public async Task JoinThread(string threadId)
    {
        // Добавляем пользователя в группу по ID темы
        await Groups.AddToGroupAsync(Context.ConnectionId, $"thread_{threadId}");
// Сохраняем подключение пользователя к теме в базе данных
        var user = Context.UserIdentifier ?? Context.ConnectionId;
        int threadIdInt = int.Parse(threadId);

        // Удаляем старые подключения
        var existing = _context.ThreadConnections
            .Where(tc => tc.UserId == user);
        _context.ThreadConnections.RemoveRange(existing);

        // Добавляем новое подключение пользователя к теме
        _context.ThreadConnections.Add(new ThreadConnection
        {
            ThreadId = threadIdInt,
            UserId = user
        });

        await _context.SaveChangesAsync();
        await BroadcastUserCounts();
    }

    // Клиент может покинуть тему
    public async Task LeaveThread(string threadId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"thread_{threadId}");
// Удаляем подключение пользователя к теме из базы данных
        var user = Context.UserIdentifier ?? Context.ConnectionId;
        int threadIdInt = int.Parse(threadId);
// Удаляем подключение пользователя к теме
        var toRemove = _context.ThreadConnections
            .FirstOrDefault(tc => tc.UserId == user && tc.ThreadId == threadIdInt);
// Если подключение найдено, удаляем его
        if (toRemove != null)
        {
            _context.ThreadConnections.Remove(toRemove);
            await _context.SaveChangesAsync();
            await BroadcastUserCounts();
        }
    }
// Метод для получения количества пользователей в каждой теме
    private async Task BroadcastUserCounts()
    {
        var threadUsers = await _context.ThreadConnections
            .GroupBy(tc => tc.ThreadId)
            .Select(g => new { ThreadId = g.Key, Count = g.Count() })
            .ToListAsync();
// Отправляем обновленное количество пользователей в каждую тему
        foreach (var item in threadUsers)
        {
            await Clients.All.SendAsync("UpdateUserCount", item.ThreadId, item.Count);
        }
    }
    // Метод вызывается при отключении клиента
    // Удаляем все подключения пользователя к темам
    // и обновляем количество пользователей в темах
    public override async Task OnDisconnectedAsync(Exception? exception)
    {// Удаляем все подключения пользователя к темам
     // Получаем идентификатор пользователя или соединения
        var user = Context.UserIdentifier ?? Context.ConnectionId;
        // Ищем все подключения пользователя к темам
        // и удаляем их из базы данных чтобы не было фантомных подключений
        var connections = _context.ThreadConnections
            .Where(tc => tc.UserId == user);

        _context.ThreadConnections.RemoveRange(connections);
        await _context.SaveChangesAsync();

        await BroadcastUserCounts();

        await base.OnDisconnectedAsync(exception);
    }
}