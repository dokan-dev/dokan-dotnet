using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokanNet.Logging;
using DokanNet.Native;

namespace DokanNet
{
    public class DokanBuilder
    {
        public delegate void OptionsConfigurationDelegate(DOKAN_OPTIONS options);
        /// <summary>
        /// The Dokan version that DokanNet is compatible with.
        /// </summary>
        /// <see cref="DOKAN_OPTIONS.Version"/>
        public const ushort DOKAN_VERSION = 200;
        private readonly DOKAN_OPTIONS _options;
        private ILogger _logger;

        public DokanBuilder()
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
        }
        public DokanBuilder ConfigureLogger(Func<ILogger> IlogegrFactory)
        {
            _logger = IlogegrFactory();
            return this;
        }
        public DokanBuilder ConfigureOptions(OptionsConfigurationDelegate optionsConfigurationDelegate)
        {
            optionsConfigurationDelegate(_options);
            return this;
        }
        public DokanBuilder Validate()
        {
            // throw on errors
            return this;
        }
        public Dokan Build(IDokanOperations operations)
        {
            return new Dokan(_logger, _options, operations);
        }

    }
}
