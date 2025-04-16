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
    public bool IsAdmin { get; set; }
    public bool IsWhitelisted { get; set; }
    public bool IsSuperAdmin { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime LastLogin { get; set; }
    public DateTime LastQuerry { get; set; }
    public string? LastChat { get; set; }

}