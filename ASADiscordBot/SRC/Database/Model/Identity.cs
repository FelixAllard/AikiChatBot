using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ASADiscordBot.Database.Model;

public class Identity
{
    [Key]
    public ulong Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsWhitelisted { get; set; }
}