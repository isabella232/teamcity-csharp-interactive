// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
namespace HostApi;

using DotNet;

[Immutype.Target]
public record DotNetPack(
    IEnumerable<(string name, string value)> Props,
    IEnumerable<string> Args,
    IEnumerable<(string name, string value)> Vars,
    string ExecutablePath = "",
    string WorkingDirectory = "",
    string Project = "",
    string Output = "",
    bool NoBuild = false,
    bool IncludeSymbols = false,
    bool IncludeSource = false,
    bool Serviceable = false,
    bool NoLogo = false,
    bool NoRestore = false,
    string VersionSuffix = "",
    string Configuration = "",
    bool UseCurrentRuntime = false,
    DotNetVerbosity? Verbosity = default,
    string ShortName = "")
    : ICommandLine
{
    public DotNetPack(params string[] args)
        : this(Enumerable.Empty<(string, string)>(), args, Enumerable.Empty<(string, string)>())
    { }
        
    public IStartInfo GetStartInfo(IHost host) =>
        new CommandLine(string.IsNullOrWhiteSpace(ExecutablePath) ? host.GetService<IDotNetSettings>().DotNetExecutablePath : ExecutablePath)
            .WithShortName("dotnet pack".GetShortName(ShortName, Project))
            .WithArgs("pack")
            .AddArgs(new []{ Project }.Where(i => !string.IsNullOrWhiteSpace(i)).ToArray())
            .WithWorkingDirectory(WorkingDirectory)
            .WithVars(Vars.ToArray())
            .AddMSBuildIntegration(host, Verbosity)
            .AddArgs(
                ("--output", Output),
                ("--version-suffix", VersionSuffix),
                ("--configuration", Configuration)
            )
            .AddBooleanArgs(
                ("--no-build", NoBuild),
                ("--include-symbols", IncludeSymbols),
                ("--include-source", IncludeSource),
                ("--serviceable", Serviceable),
                ("--nologo", NoLogo),
                ("--no-restore", NoRestore),
                ("--use-current-runtime", UseCurrentRuntime)
            )
            .AddProps("/p", Props.ToArray())
            .AddArgs(Args.ToArray());
}