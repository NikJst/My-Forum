namespace MyForum.Models;

public class ThreadConnection
{
    public int Id { get; set; }
    public int ThreadId { get; set; }
    public string UserId { get; set; } = string.Empty;
}