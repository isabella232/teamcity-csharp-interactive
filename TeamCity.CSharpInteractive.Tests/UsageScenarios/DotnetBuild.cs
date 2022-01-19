// ReSharper disable StringLiteralTypo
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable CommentTypo
namespace TeamCity.CSharpInteractive.Tests.UsageScenarios
{
    using System.Linq;
    using Dotnet;
    using Shouldly;
    using Xunit;

    [CollectionDefinition("Integration", DisableParallelization = true)]
    public class DotnetBuild: Scenario
    {
        [Fact]
        public void Run()
        {
            // $visible=true
            // $tag=11 .NET build API
            // $priority=00
            // $description=Build a project
            // {
            // Adds the namespace "Dotnet" to use .NET build API
            // ## using Dotnet;

            // Resolves a build service
            var build = GetService<IBuild>();
            
            // Creates a new library project, running a command like: "dotnet new classlib -n MyLib --force"
            var result = build.Run(new Custom("new", "classlib", "-n", "MyLib", "--force"));
            result.ExitCode.ShouldBe(0);

            // Builds the library project, running a command like: "dotnet build" from the directory "MyLib"
            result = build.Run(new Build().WithWorkingDirectory("MyLib"));
            
            // The "result" variable provides details about a build
            result.Errors.Any(message => message.State == BuildMessageState.Error).ShouldBeFalse();
            result.ExitCode.ShouldBe(0);
            // }
        }
    }
}