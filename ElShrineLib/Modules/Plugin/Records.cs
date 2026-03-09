namespace ElShrine.Modules.Plugin;

public record PluginInfo(
    string Id, // Unique name
    string PluginFullName, // Entry point type full name
    string Folder, // Plugin folder to locate container
    string FileHash, // File hash of its assembly
    string Name, // Display name, from attr
    string Author, // From attr
    string VersionInfo, // From attr
    string Description // From attr 
    );