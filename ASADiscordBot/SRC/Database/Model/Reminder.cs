using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;

namespace ASADiscordBot.Database.Model;

public class Reminder
{
    [Key]
    public int Id { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime ReminderTime { get; set; }
    public int TimePushedBack { get; set; }
    public ulong RequesterDiscordId { get; set; }
}