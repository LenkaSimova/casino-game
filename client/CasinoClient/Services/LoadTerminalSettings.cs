using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using CasinoClient.Models;

namespace CasinoClient.Services;

public class ConfigurationService
{
    private const string CONFIG_FILE = "terminal_config.json";

    public static TerminalConfig LoadConfig()
    {
        try
        {
            if (File.Exists(CONFIG_FILE))
            {
                var json = File.ReadAllText(CONFIG_FILE);
                var config = JsonSerializer.Deserialize<TerminalConfig>(json);
                return config ?? GetDefaultConfig();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config: {ex.Message}");
        }

        return GetDefaultConfig();
    }

    private static TerminalConfig GetDefaultConfig()
    {
        return new TerminalConfig
        {
            Id = 0,
            Name = "Terminal-Default",
            AllowedCommands = new List<string> { "help", "clear", "echo", "status", "exit" },
            Prompt = "casino@terminal-default:~$ "
        };
    }
}