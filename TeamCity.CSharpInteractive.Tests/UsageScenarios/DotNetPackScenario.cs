// ReSharper disable StringLiteralTypo
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable CommentTypo
namespace TeamCity.CSharpInteractive.Tests.UsageScenarios;

using HostApi;

[CollectionDefinition("Integration", DisableParallelization = true)]
[Trait("Integration", "true")]
public class DotNetPackScenario : BaseScenario
{
    [Fact]
    public void Run()
    {
        // $visible=true
        // $tag=11 .NET build API
        // $priority=01
        // $description=Pack a project
        // {
        // Adds the namespace "HostApi" to use .NET build API
        // ## using HostApi;

        // Creates a new library project, running a command like: "dotnet new classlib -n MyLib --force"
        var result = new DotNetCustom("new", "classlib", "-n", "MyLib", "--force").Build();
        result.ExitCode.ShouldBe(0);

        // Creates a NuGet package of version 1.2.3 for the project, running a command like: "dotnet pack /p:version=1.2.3" from the directory "MyLib"
        result = new DotNetPack()
                .WithWorkingDirectory("MyLib")
                .AddProps(("version", "1.2.3"))
                .Build();

        result.ExitCode.ShouldBe(0);
        // }
    }
}