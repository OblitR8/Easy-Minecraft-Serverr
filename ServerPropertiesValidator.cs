using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Easy_Minecraft_Serverr
{
    public static class ServerPropertiesValidator
    {
        // Regex patterns for validation
        private static readonly Regex _keyPattern = new Regex(@"^[a-zA-Z0-9._-]+$");
        private static readonly Regex _propertyLinePattern = new Regex(@"^([^=]+)=(.*)$");

        public static (bool IsValid, List<string> Errors) ValidatePropertiesFile(string filePath)
        {
            var errors = new List<string>();

            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    return (true, errors); // New file is OK
                }

                var lines = System.IO.File.ReadAllLines(filePath);
                int lineNumber = 0;

                foreach (var line in lines)
                {
                    lineNumber++;

                    // Skip empty lines and comments
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    // Validate property format
                    var match = _propertyLinePattern.Match(line);
                    if (!match.Success)
                    {
                        errors.Add($"Line {lineNumber}: Invalid property format. Expected 'key=value'");
                        continue;
                    }

                    string key = match.Groups[1].Value.Trim();
                    string value = match.Groups[2].Value;

                    // Validate key format
                    if (!_keyPattern.IsMatch(key))
                    {
                        errors.Add($"Line {lineNumber}: Invalid property key '{key}'. Keys must contain only alphanumeric characters, dots, hyphens, and underscores.");
                    }

                    // Validate specific known properties
                    ValidatePropertyValue(key, value, lineNumber, errors);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error reading properties file: {ex.Message}");
            }

            return (errors.Count == 0, errors);
        }

        public static string SanitizePropertyValue(string value)
        {
            // Remove dangerous characters but keep the value usable
            return value?.Trim() ?? "";
        }

        private static void ValidatePropertyValue(string key, string value, int lineNumber, List<string> errors)
        {
            key = key.ToLower();

            // Validate specific properties
            switch (key)
            {
                case "server-port":
                    if (!int.TryParse(value, out int port) || port < 1 || port > 65535)
                        errors.Add($"Line {lineNumber}: Invalid port '{value}'. Must be between 1 and 65535.");
                    break;

                case "max-players":
                    if (!int.TryParse(value, out int maxPlayers) || maxPlayers < 0)
                        errors.Add($"Line {lineNumber}: Invalid max-players '{value}'. Must be a non-negative integer.");
                    break;

                case "view-distance":
                    if (!int.TryParse(value, out int viewDist) || viewDist < 3 || viewDist > 32)
                        errors.Add($"Line {lineNumber}: Invalid view-distance '{value}'. Must be between 3 and 32.");
                    break;

                case "difficulty":
                    if (!new[] { "peaceful", "easy", "normal", "hard" }.Contains(value.ToLower()))
                        errors.Add($"Line {lineNumber}: Invalid difficulty '{value}'. Must be peaceful, easy, normal, or hard.");
                    break;

                case "gamemode":
                    if (!new[] { "survival", "creative", "adventure", "spectator" }.Contains(value.ToLower()))
                        errors.Add($"Line {lineNumber}: Invalid gamemode '{value}'. Must be survival, creative, adventure, or spectator.");
                    break;
            }
        }
    }
}
