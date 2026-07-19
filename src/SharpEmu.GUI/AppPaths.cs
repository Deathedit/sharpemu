// Copyright (C) 2026 SharpEmu Emulator Project
// SPDX-License-Identifier: GPL-2.0-or-later

namespace SharpEmu.GUI;

/// <summary>
/// Resolves the writable locations SharpEmu uses for its own data:
/// GUI settings, per-game configs, savedata (<c>user/</c>) and logs.
///
/// On Windows the historical "portable" convention is kept: data lives next
/// to the executable. On Linux the data follows the XDG Base Directory
/// specification, i.e. <c>$XDG_CONFIG_HOME/SharpEmu</c> (falling back to
/// <c>~/.config/SharpEmu</c>). On macOS it lives in the platform-standard
/// <c>~/Library/Application Support/SharpEmu</c>.
///
/// Set the <c>SHARPEMU_DATA_HOME</c> environment variable to override the
/// root on any platform (useful for testing or a fully portable install).
/// </summary>
public static class AppPaths
{
    private const string AppFolderName = "SharpEmu";

    /// <summary>Root directory that contains all SharpEmu user data.</summary>
    public static string Root { get; } = ResolveRoot();

    /// <summary><c>gui-settings.json</c> location.</summary>
    public static string SettingsFile => Path.Combine(Root, "gui-settings.json");

    /// <summary><c>gui-crash.log</c> location.</summary>
    public static string CrashLogFile => Path.Combine(Root, "gui-crash.log");

    /// <summary>The <c>user/</c> directory (savedata, per-game configs, logs).</summary>
    public static string UserDirectory => Path.Combine(Root, "user");

    /// <summary>The <c>user/logs</c> directory.</summary>
    public static string LogsDirectory => Path.Combine(UserDirectory, "logs");

    /// <summary>The <c>user/custom_configs</c> directory.</summary>
    public static string CustomConfigsDirectory => Path.Combine(UserDirectory, "custom_configs");

    /// <summary>Creates <paramref name="path"/> if needed and returns it.</summary>
    public static string EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }

    private static string ResolveRoot()
    {
        var overridePath = Environment.GetEnvironmentVariable("SHARPEMU_DATA_HOME");
        if (!string.IsNullOrWhiteSpace(overridePath))
        {
            return Path.GetFullPath(overridePath);
        }

        // Keep the portable convention on Windows: data next to the executable.
        if (OperatingSystem.IsWindows())
        {
            return AppContext.BaseDirectory;
        }

        // macOS convention: ~/Library/Application Support/SharpEmu.
        if (OperatingSystem.IsMacOS())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "Application Support", AppFolderName);
        }

        // Linux (XDG): SpecialFolder.ApplicationData resolves to
        // $XDG_CONFIG_HOME, or ~/.config when the variable is unset.
        var configHome = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData,
            Environment.SpecialFolderOption.Create);

        if (string.IsNullOrEmpty(configHome))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            configHome = Path.Combine(home, ".config");
        }

        return Path.Combine(configHome, AppFolderName);
    }
}
