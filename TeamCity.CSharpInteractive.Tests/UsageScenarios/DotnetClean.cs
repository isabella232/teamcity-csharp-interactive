// ReSharper disable StringLiteralTypo
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable CommentTypo
namespace TeamCity.CSharpInteractive.Tests.UsageScenarios;

using Script.DotNet;

[CollectionDefinition("Integration", DisableParallelization = true)]
public class DotNetClean: ScenarioHostService
{
    [Fact]
    public void Run()
    {
        // $visible=true
        // $tag=11 .NET build API
        // $priority=00
        // $description=Clean a project
        // {
        // Adds the namespace "Script.DotNet" to use .NET build API
        // ## using DotNet;

        // Resolves a build service
        var build = GetService<IBuildRunner>();
            
        // Creates a new library project, running a command like: "dotnet new classlib -n MyLib --force"
        var result = build.Run(new Custom("new", "classlib", "-n", "MyLib", "--force"));
        result.ExitCode.ShouldBe(0);

        // Builds the library project, running a command like: "dotnet build" from the directory "MyLib"
        result = build.Run(new Build().WithWorkingDirectory("MyLib"));
        result.ExitCode.ShouldBe(0);
            
        // Clean the project, running a command like: "dotnet clean" from the directory "MyLib"
        result = build.Run(new Clean().WithWorkingDirectory("MyLib"));
            
        // The "result" variable provides details about a build
        result.ExitCode.ShouldBe(0);
        // }
    }
}