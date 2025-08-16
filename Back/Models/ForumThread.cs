public class ForumThread
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public List<Post> Posts { get; set; } = new();
}
