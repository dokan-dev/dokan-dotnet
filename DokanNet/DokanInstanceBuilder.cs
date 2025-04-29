using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using DokanNet.Legacy;
using DokanNet.Logging;
using DokanNet.Native;

namespace DokanNet;

/// <summary>
/// Dokan FileSystem build helper. Allows to create one or multiple <see cref="DokanInstance"/> based on a given configuration.
/// </summary>
public class DokanInstanceBuilder
{
    /// <summary>
    /// Delegate used by <see cref="ConfigureOptions"/> to customize th internal <see cref="DOKAN_OPTIONS"/>.
    /// </summary>
    public delegate void OptionsConfigurationDelegate(DOKAN_OPTIONS options);

    /// <summary>
    /// The Dokan version that DokanNet is compatible with.
    /// </summary>
    /// <see cref="DOKAN_OPTIONS.Version"/>
    public const ushort DOKAN_VERSION = 200;

    private readonly DOKAN_OPTIONS _options;

    [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Dokan instance isn't really needed, a reference is kept to enforce a workflow")]
    private readonly Dokan _dokan;

    private ILogger _logger;

    /// <summary>
    /// Constructure an object with a <see cref="NullLogger"/> and default <see cref="DOKAN_OPTIONS"/> that will use the given <paramref name="dokan"/>.
    /// </summary>
    public DokanInstanceBuilder(Dokan dokan)
    {
        _logger = new NullLogger();
        _options = new DOKAN_OPTIONS
        {
            Version = DOKAN_VERSION,
            MountPoint = "",
            UNCName = string.Empty,
            SingleThread = false,
            Options = DokanOptions.FixedDrive,
            TimeOut = TimeSpan.FromSeconds(20),
            AllocationUnitSize = 512,
            SectorSize = 512,
            VolumeSecurityDescriptorLength = 0,

        };
        _dokan = dokan;
    }

    /// <summary>
    /// Allows to set a custom <see cref="ILogger"/> like <see cref="Logger"/>, <see cref="TraceLogger"/> to be used
    /// for the instance created by <see cref="Build(IDokanOperations2)"/>.
    /// </summary>
    public DokanInstanceBuilder ConfigureLogger(Func<ILogger> IlogegrFactory)
    {
        _logger = IlogegrFactory();
        return this;
    }

    /// <summary>
    /// Allows to personalize the <see cref="DOKAN_OPTIONS"/> use for <see cref="Build(IDokanOperations2)"/>.
    /// </summary>
    public DokanInstanceBuilder ConfigureOptions(OptionsConfigurationDelegate optionsConfigurationDelegate)
    {
        optionsConfigurationDelegate(_options);
        return this;
    }

    /// <summary>
    /// Verify that the provided configuration is valid.
    /// Note: Has no effect for now.
    /// </summary>
    public DokanInstanceBuilder Validate()
    {
        // throw on errors
        return this;
    }

    /// <summary>
    /// Create a <see cref="DokanInstance"/> based on the previously provided information
    /// through <see cref="ConfigureOptions"/> and <see cref="ConfigureLogger"/>.
    /// </summary>
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public DokanInstance Build(IDokanOperations2 operations)
    {
        return new DokanInstance(_logger, _options, _dokan, operations);
    }

    /// <summary>
    /// Create a <see cref="DokanInstance"/> based on the previously provided information
    /// through <see cref="ConfigureOptions"/> and <see cref="ConfigureLogger"/>.
    /// </summary>
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public DokanInstance Build(IDokanOperations operations)
    {
        return new DokanInstance(_logger, _options, _dokan, new DokanOperationsAdapter(operations, _logger));
    }
}
