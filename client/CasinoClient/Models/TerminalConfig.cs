using System.Collections.Generic;

namespace CasinoClient.Models;

public class TerminalConfig
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<string> AllowedCommands { get; set; } = new();
    public string Prompt { get; set; } = "casino@terminal:~$ ";
    public string ServerBaseUrl { get; set; } = "http://localhost:5122";
}