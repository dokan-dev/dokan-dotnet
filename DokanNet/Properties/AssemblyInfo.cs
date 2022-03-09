using System.Runtime.CompilerServices;

#if DEBUG
// Make internals visible to tests.
[assembly:InternalsVisibleTo("DokanNet.Tests.net461")]
#endif