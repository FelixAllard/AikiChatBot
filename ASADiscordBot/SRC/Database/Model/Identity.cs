using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ASADiscordBot.Database.Model;

public class Identity
{
    [Key]
    public int Id { get; set; }
    public ulong DiscordUserId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsAdmin { get; set; } = false;
    public bool IsWhitelisted { get; set; } = false;
    public bool IsSuperAdmin { get; set; } = false;
    public DateTime DateAdded { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime LastQuerry { get; set; }
    public string? LastChat { get; set; }

}