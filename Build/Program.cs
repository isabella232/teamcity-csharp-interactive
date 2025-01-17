﻿using HostApi;
using JetBrains.TeamCity.ServiceMessages.Write.Special;
using NuGet.Versioning;

const string solutionFile = "TeamCity.CSharpInteractive.sln";
const string packageId = "TeamCity.CSharpInteractive";
const string toolPackageId = "TeamCity.csi";
const string templatesPackageId = "TeamCity.CSharpInteractive.Templates";

var currentDir = Environment.CurrentDirectory;
if (!File.Exists(solutionFile))
{
    Error($"Cannot find the solution \"{solutionFile}\". Current directory is \"{currentDir}\".");
    return 1;
}

var underTeamCity = Environment.GetEnvironmentVariable("TEAMCITY_VERSION") != default;
var configuration = Property.Get("configuration", "Release");
var apiKey = Property.Get("apiKey", "");
var integrationTests = bool.Parse(Property.Get("integrationTests", underTeamCity.ToString()));
var defaultVersion = NuGetVersion.Parse(Property.Get("version", "1.0.0-dev", underTeamCity));
var outputDir = Path.Combine(currentDir, "TeamCity.CSharpInteractive", "bin", configuration);
var templateOutputDir = Path.Combine(currentDir, "TeamCity.CSharpInteractive.Templates", "bin", configuration);

var dockerLinuxTests = false;
new DockerCustom("info").WithShortName("Defining a docker container type")
    .Run(output =>
    {
        WriteLine("    " + output.Line, Color.Details);
        if (output.Line.Contains("OSType: linux"))
        {
            dockerLinuxTests = true;
        }
    });

if (!dockerLinuxTests)
{
    Warning("The docker Linux container is not available.");
}

var packageVersion = new[]
{
    Version.GetNext(new NuGetRestoreSettings(toolPackageId).WithPackageType(NuGetPackageType.Tool), defaultVersion),
    Version.GetNext(new NuGetRestoreSettings(packageId), defaultVersion)
}.Max()!;

var templatePackageVersion = Version.GetNext(new NuGetRestoreSettings(templatesPackageId), defaultVersion);

WriteLine($"Tool and package version: {packageVersion}", Color.Highlighted);
WriteLine($"Template version: {templatePackageVersion}", Color.Highlighted);

var packages = new[]
{
    new PackageInfo(
        packageId,
        Path.Combine("TeamCity.CSharpInteractive", "TeamCity.CSharpInteractive.csproj"),
        Path.Combine(outputDir, "TeamCity.CSharpInteractive", $"{packageId}.{packageVersion.ToString()}.nupkg"),
        packageVersion,
        true),
    
    new PackageInfo(
        toolPackageId,
        Path.Combine("TeamCity.CSharpInteractive", "TeamCity.CSharpInteractive.Tool.csproj"),
        Path.Combine(outputDir, "TeamCity.CSharpInteractive.Tool", $"{toolPackageId}.{packageVersion.ToString()}.nupkg"),
        packageVersion,
        true),
    
    new PackageInfo(
        templatesPackageId,
        Path.Combine("TeamCity.CSharpInteractive.Templates", "TeamCity.CSharpInteractive.Templates.csproj"),
        Path.Combine(templateOutputDir, $"{templatesPackageId}.{templatePackageVersion.ToString()}.nupkg"),
        templatePackageVersion,
        false)
};

Assertion.Succeed(
    new DotNetClean()
        .WithProject(solutionFile)
        .WithVerbosity(DotNetVerbosity.Quiet)
        .WithConfiguration(configuration)
        .Build()
);

foreach (var package in packages)
{
    var path = Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".nuget", "packages", package.Id, package.Version.ToString());
    if (Directory.Exists(path))
    {
        Directory.Delete(path, true);
    }
}

var buildProps = new[] {("version", packageVersion.ToString())};
Assertion.Succeed(
    new DotNetBuild()
        .WithProject(solutionFile)
        .WithConfiguration(configuration)
        .WithProps(buildProps)
        .Build());

var test = new DotNetTest()
    .WithProject(solutionFile)
    .WithConfiguration(configuration)
    .WithNoBuild(true)
    .WithProps(buildProps);

if (!integrationTests)
{
    test = test.WithFilter("Integration!=true");
    Warning("Integration tests were skipped.");
}

if (!dockerLinuxTests)
{
    test = test.WithFilter(string.Join('&', test.Filter, "Integration!=true"));
    Warning("Docker tests were skipped.");
}

Assertion.Succeed(test.Build());

foreach (var package in packages)
{
    var packageOutput = Path.GetDirectoryName(package.Package);
    if (Directory.Exists(packageOutput))
    {
        Directory.Delete(packageOutput, true);
    }

    Assertion.Succeed(new DotNetPack()
        .WithConfiguration(configuration)
        .WithProps(("version", package.Version.ToString()))
        .WithProject(package.Project)
        .Build());
}

var uninstallTool = new DotNetCustom("tool", "uninstall", toolPackageId, "-g")
    .WithShortName("Uninstalling tool");

if (uninstallTool.Run(output => WriteLine(output.Line)) != 0)
{
    Warning($"{uninstallTool} failed.");
}

var installTool = new DotNetCustom("tool", "install", toolPackageId, "-g", "--version", packageVersion.ToString(), "--add-source", Path.Combine(outputDir, "TeamCity.CSharpInteractive.Tool"))
    .WithShortName("Installing tool");

if (installTool.Run(output => WriteLine(output.Line)) != 0)
{
    Warning($"{installTool} failed.");
}

Assertion.Succeed(new DotNetCustom("csi", "/?").Run(), "Checking tool");

var uninstallTemplates = new DotNetCustom("new", "-u", templatesPackageId)
    .WithShortName("Uninstalling template");

if (uninstallTemplates.Run(output => WriteLine(output.Line)) != 0)
{
    Warning($"{uninstallTemplates} failed.");
}

var installTemplates = new DotNetCustom("new", "-i", $"{templatesPackageId}::{templatePackageVersion.ToString()}", "--nuget-source", templateOutputDir)
    .WithShortName("Installing template");

Assertion.Succeed(installTemplates.Run(), installTemplates.ShortName);

var buildProjectDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()[..8]);
Directory.CreateDirectory(buildProjectDir);
try
{
    var sampleProjectDir = Path.Combine("Samples", "DemoProject", "MySampleLib", "MySampleLib.Tests");
    Assertion.Succeed(new DotNetCustom("new", "build", $"--package-version={packageVersion}").WithWorkingDirectory(buildProjectDir).Run(), "Creating new build project");
    Assertion.Succeed(new DotNetBuild().WithProject(buildProjectDir).AddSources(Path.Combine(outputDir, "TeamCity.CSharpInteractive")).WithShortName("Building a build project").Build());
    Assertion.Succeed(new DotNetRun().WithProject(buildProjectDir).WithNoBuild(true).WithWorkingDirectory(sampleProjectDir).Run(), "Running a build as a console application");
    Assertion.Succeed(new CommandLine("dotnet", "csi", Path.Combine(buildProjectDir, "Program.csx")).WithWorkingDirectory(sampleProjectDir).Run(), "Running a build as a C# script");

    Info("Publishing artifacts.");
    var teamCityWriter = GetService<ITeamCityWriter>();

    foreach (var package in packages)
    {
        if (!File.Exists(package.Package))
        {
            Error($"NuGet package {package.Package} does not exist.");
            return 1;
        }

        teamCityWriter.PublishArtifact($"{package.Package} => .");
    }

    if (!string.IsNullOrWhiteSpace(apiKey) && packageVersion.Release != "dev" && templatePackageVersion.Release != "dev")
    {
        var push = new DotNetNuGetPush().WithApiKey(apiKey).WithSources("https://api.nuget.org/v3/index.json");
        foreach (var package in packages.Where(i => i.Publish))
        {
            Assertion.Succeed(push.WithPackage(package.Package).Run(), $"Pushing {Path.GetFileName(package.Package)}");
        }
    }
    else
    {
        Info("Pushing NuGet packages were skipped.");
    }
}
finally
{
    Directory.Delete(buildProjectDir, true);
}

WriteLine("To use the csi tool:", Color.Highlighted);
WriteLine("    dotnet csi /?", Color.Highlighted);
WriteLine("To create a build project from the template:", Color.Highlighted);
WriteLine($"    dotnet new build --package-version={packageVersion}", Color.Highlighted);

return 0;

record PackageInfo(string Id, string Project, string Package, NuGetVersion Version, bool Publish);