// ReSharper disable ClassNeverInstantiated.Global
namespace Teamcity.CSharpInteractive
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using Microsoft.DotNet.PlatformAbstractions;

    [ExcludeFromCodeCoverage]
    internal class Environment : IEnvironment, ITraceSource, IWorkingDirectoryContext
    {
        private readonly LinkedList<string> _workingDirectories = new();
        
        public Platform OperatingSystemPlatform => RuntimeEnvironment.OperatingSystemPlatform;

        public string ProcessArchitecture => RuntimeEnvironment.RuntimeArchitecture;

        public IEnumerable<string> GetCommandLineArgs() => System.Environment.GetCommandLineArgs();

        public string GetPath(SpecialFolder specialFolder)
        {
            switch (OperatingSystemPlatform)
            {
                case Platform.Windows:
                    return specialFolder switch
                    {
                        SpecialFolder.Temp => System.Environment.GetEnvironmentVariable("TMP") ?? ".",
                        SpecialFolder.ProgramFiles => System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles),
                        SpecialFolder.WorkingDirectory => GetWorkingDirectory(),
                        _ => throw new ArgumentOutOfRangeException(nameof(specialFolder), specialFolder, null)
                    };

                case Platform.Unknown:
                case Platform.Linux:
                case Platform.Darwin:
                case Platform.FreeBSD:
                    return specialFolder switch
                    {
                        SpecialFolder.Temp => System.Environment.GetEnvironmentVariable("TMP") ?? ".",
                        SpecialFolder.ProgramFiles => "usr/local/share",
                        SpecialFolder.WorkingDirectory => GetWorkingDirectory(),
                        _ => throw new ArgumentOutOfRangeException(nameof(specialFolder), specialFolder, null)
                    };
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Exit(ExitCode exitCode) => System.Environment.Exit((int)exitCode);

        public IEnumerable<Text> GetTrace()
        {
            yield return new Text($"OperatingSystemPlatform: {OperatingSystemPlatform}");
            yield return new Text($"ProcessArchitecture: {ProcessArchitecture}");
            foreach (var specialFolder in Enum.GetValues(typeof(SpecialFolder)).OfType<SpecialFolder>())
            {
                yield return new Text($"Path({specialFolder}): {GetPath(specialFolder)}");
            }

            yield return new Text($"Command line arguments:");
            foreach (var arg in System.Environment.GetCommandLineArgs())
            {
                yield return new Text($"  {arg}");
            }
            
            yield return new Text($"Environment variables:");
            foreach (var key in System.Environment.GetEnvironmentVariables().Keys.OfType<string>().OrderBy(i => i).Where(string.IsNullOrWhiteSpace))
            {
                yield return new Text($"  {key}={System.Environment.GetEnvironmentVariable(key)}");
            }
        }

        public IDisposable OverrideWorkingDirectory(string? workingDirectory)
        {
            if (workingDirectory == null)
            {
                return Disposable.Empty;
            }

            _workingDirectories.AddLast(workingDirectory);
            return Disposable.Create(() => _workingDirectories.Remove(workingDirectory));
        }

        private string GetWorkingDirectory() => 
            _workingDirectories.Count > 0 ? _workingDirectories.Last!.Value : Directory.GetCurrentDirectory();
    }
}