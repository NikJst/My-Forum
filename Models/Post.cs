public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public int ThreadId { get; set; }
    public ForumThread Thread { get; set; } = null!;

    public int UserId { get; set; } 
    public User User { get; set; } = null!; // подключать через Include
}
