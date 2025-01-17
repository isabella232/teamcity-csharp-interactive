// ReSharper disable StringLiteralTypo
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
namespace TeamCity.CSharpInteractive.Tests.UsageScenarios;

using System;
using HostApi;

[CollectionDefinition("Integration", DisableParallelization = true)]
[Trait("Integration", "true")]
public class CommandLineBuildScenario : BaseScenario
{
    [SkippableFact]
    public void Run()
    {
        Skip.IfNot(Environment.OSVersion.Platform == PlatformID.Win32NT);

        // $visible=true
        // $tag=10 Command Line API
        // $priority=00
        // $description=Build command lines
        // {
        // Adds the namespace "Script.Cmd" to use Command Line API
        // ## using HostApi;

        // Creates a simple command line from just the name of the executable 
        new CommandLine("whoami");

        // Creates a command line with multiple command line arguments 
        new CommandLine("cmd", "/c", "echo", "Hello");

        // Same as previous statement
        new CommandLine("cmd", "/c")
            .AddArgs("echo", "Hello");

        // Builds a command line with multiple environment variables
        new CommandLine("cmd", "/c", "echo", "Hello")
            .AddVars(("Var1", "val1"), ("var2", "Val2"));

        // Builds a command line to run from a specific working directory 
        new CommandLine("cmd", "/c", "echo", "Hello")
            .WithWorkingDirectory("MyDyrectory");

        // Builds a command line and replaces all command line arguments
        new CommandLine("cmd", "/c", "echo", "Hello")
            .WithArgs("/c", "echo", "Hello !!!");
        // }
    }
}