using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet.Logging;
using DokanNet.Native;

namespace DokanNet
{
    public class DokanInstanceBuilder
    {
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

        public DokanInstanceBuilder(Dokan dokan)
        {
            _logger = new NullLogger();
            _options = new DOKAN_OPTIONS
            {
                Version = DOKAN_VERSION,
                MountPoint = "",
                UNCName = String.Empty,
                SingleThread = false,
                Options = DokanOptions.FixedDrive,
                TimeOut = TimeSpan.FromSeconds(20),
                AllocationUnitSize = 512u,
                SectorSize = 512u,
                VolumeSecurityDescriptorLength = 0,

            };
            _dokan = dokan;
        }
        public DokanInstanceBuilder ConfigureLogger(Func<ILogger> IlogegrFactory)
        {
            _logger = IlogegrFactory();
            return this;
        }
        public DokanInstanceBuilder ConfigureOptions(OptionsConfigurationDelegate optionsConfigurationDelegate)
        {
            optionsConfigurationDelegate(_options);
            return this;
        }
        public DokanInstanceBuilder Validate()
        {
            // throw on errors
            return this;
        }
        public DokanInstance Build(IDokanOperations operations)
        {
            return new DokanInstance(_logger, _options, operations);
        }

    }
}
