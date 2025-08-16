using Microsoft.EntityFrameworkCore;
namespace MyForum.Models;
public class ForumContext : DbContext
{
    public DbSet<ThreadConnection> ThreadConnections { get; set; }

    public ForumContext(DbContextOptions<ForumContext> options) : base(options) {}

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ForumThread> Threads { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;

}
