using System.Reflection;
using System.Runtime.Loader;
using Jellyfin.Plugin.DismissWatching.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.DismissWatching;

/// <summary>
/// The main plugin.
/// </summary>
public class DismissWatchingPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DismissWatchingPlugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{T}"/> interface.</param>
    /// <param name="configurationManager">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
    public DismissWatchingPlugin(
        IApplicationPaths applicationPaths,
        IXmlSerializer xmlSerializer,
        ILogger<DismissWatchingPlugin> logger,
        IServerConfigurationManager configurationManager)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        _logger = logger;
    }

    /// <summary>
    /// Registers the JavaScript with the JavaScript Injector plugin.
    /// </summary>
    public void RegisterJavascript()
    {
        try
        {
            // Find the JavaScript Injector assembly
            Assembly? jsInjectorAssembly = AssemblyLoadContext.All
                .SelectMany(x => x.Assemblies)
                .FirstOrDefault(x => x.FullName?.Contains("Jellyfin.Plugin.JavaScriptInjector", StringComparison.Ordinal) ?? false);

            if (jsInjectorAssembly != null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var scriptPaths = new[]
                {
                    $"{assembly.GetName().Name}.Web.discontinue-watching.js",
                    $"{assembly.GetName().Name}.Web.nextup-dismiss.js"
                };
                var scriptParts = new List<string>();
                foreach (var customScriptPath in scriptPaths)
                {
                    using var scriptStream = assembly.GetManifestResourceStream(customScriptPath);
                    if (scriptStream == null)
                    {
                        _logger.LogError("Could not find embedded Dismiss Watching script at path: {Path}", customScriptPath);
                        return;
                    }

                    using var reader = new StreamReader(scriptStream);
                    scriptParts.Add(reader.ReadToEnd());
                }
                var scriptContent = string.Join("\n\n", scriptParts);

                // Get the PluginInterface type
                Type? pluginInterfaceType = jsInjectorAssembly.GetType("Jellyfin.Plugin.JavaScriptInjector.PluginInterface");
                if (pluginInterfaceType == null)
                {
                    _logger.LogError("Could not find PluginInterface type in JavaScript Injector assembly.");
                    return;
                }

                // Create the registration payload
                var scriptRegistration = new JObject
                {
                            { "id", $"{Id}-script" },
                            { "name", "Dismiss Watching Client Script" },
                            { "script", scriptContent },
                            { "enabled", true },
                            { "requiresAuthentication", true },
                            { "pluginId", Id.ToString() },
                            { "pluginName", Name },
                            { "pluginVersion", Version.ToString() }
                        };

                // Register the script
                var registerResult = pluginInterfaceType.GetMethod("RegisterScript")?.Invoke(null, new object[] { scriptRegistration });

                // Validate the return value
                if (registerResult is bool success && success)
                {
                    _logger.LogInformation("Successfully registered JavaScript with JavaScript Injector plugin.");
                }
                else
                {
                    _logger.LogWarning("Failed to register JavaScript with JavaScript Injector plugin. RegisterScript returned false.");
                }

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register JavaScript with JavaScript Injector plugin.");
        }
    }

    /// <inheritdoc />
    public override void OnUninstalling()
    {
        try
        {
            // Find the JavaScript Injector assembly
            Assembly? jsInjectorAssembly = AssemblyLoadContext.All
                .SelectMany(x => x.Assemblies)
                .FirstOrDefault(x => x.FullName?.Contains("Jellyfin.Plugin.JavaScriptInjector", StringComparison.Ordinal) ?? false);

            if (jsInjectorAssembly != null)
            {
                Type? pluginInterfaceType = jsInjectorAssembly.GetType("Jellyfin.Plugin.JavaScriptInjector.PluginInterface");

                if (pluginInterfaceType != null)
                {
                    // Unregister all scripts from your plugin
                    var unregisterResult = pluginInterfaceType.GetMethod("UnregisterAllScriptsFromPlugin")?.Invoke(null, new object[] { Id.ToString() });

                    // Validate the return value
                    if (unregisterResult is int removedCount)
                    {
                        _logger?.LogInformation("Successfully unregistered {Count} script(s) from JavaScript Injector plugin.", removedCount);
                    }
                    else
                    {
                        _logger?.LogWarning("Failed to unregister scripts from JavaScript Injector plugin. Method returned unexpected value.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to unregister JavaScript scripts.");
        }

        base.OnUninstalling();
    }

    private readonly ILogger<DismissWatchingPlugin> _logger;

    /// <inheritdoc />
    public override string Name => "Dismiss Watching";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("d4af5c47-69e0-47b7-84cc-1b556b76f98a");

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static DismissWatchingPlugin? Instance { get; private set; }



    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        var prefix = GetType().Namespace;

        return
        [
            // Settings page
            new PluginPageInfo
            {
                Name = "Settings",
                EmbeddedResourcePath = $"{prefix}.Pages.Settings.index.html"
            },
            new PluginPageInfo
            {
                Name = "Settings.js",
                EmbeddedResourcePath = $"{prefix}.Pages.Settings.index.js"
            },

            // UserLists page
            new PluginPageInfo
            {
                Name = "UserLists",
                EmbeddedResourcePath = $"{prefix}.Pages.UserLists.index.html"
            },
            new PluginPageInfo
            {
                Name = "UserLists.js",
                EmbeddedResourcePath = $"{prefix}.Pages.UserLists.index.js"
            },

            // Info page
            new PluginPageInfo
            {
                Name = "Info",
                EmbeddedResourcePath = $"{prefix}.Pages.Info.index.html"
            },
            new PluginPageInfo
            {
                Name = "Info.js",
                EmbeddedResourcePath = $"{prefix}.Pages.Info.index.js"
            },

            // Shared utilities
            new PluginPageInfo
            {
                Name = "shared.js",
                EmbeddedResourcePath = $"{prefix}.Pages.shared.js"
            }
        ];
    }
}
