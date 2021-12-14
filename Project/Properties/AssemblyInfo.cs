using System.Resources;
using System.Reflection;
using System.Runtime.InteropServices;
using VRCBhapticsIntegration.Properties;

[assembly: AssemblyTitle(BuildInfo.Name)]
[assembly: AssemblyCompany(BuildInfo.Company)]
[assembly: AssemblyProduct(BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + BuildInfo.Author)]
[assembly: AssemblyTrademark(BuildInfo.Company)]
[assembly: Guid("F306E898-C72D-403E-950E-39E13911070B")]
[assembly: AssemblyVersion(BuildInfo.Version)]
[assembly: AssemblyFileVersion(BuildInfo.Version)]
[assembly: NeutralResourcesLanguage("en")]
[assembly: MelonLoader.MelonInfo(typeof(VRCBhapticsIntegration.VRCBhapticsIntegration), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author, BuildInfo.DownloadLink)]
[assembly: MelonLoader.MelonGame("VRChat", "VRChat")]
[assembly: MelonLoader.MelonPlatformDomain(MelonLoader.MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
[assembly: MelonLoader.VerifyLoaderVersion("0.5.2", true)]