using System.Security.AccessControl;

namespace DokanNet.Tests
{
    internal static class FileSystemSecurityExtensions
    {
        public static string AsString(this FileSystemSecurity security)
            => security.GetSecurityDescriptorSddlForm(AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
    }
}