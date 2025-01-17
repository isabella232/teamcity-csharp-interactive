// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable StringLiteralTypo
// ReSharper disable InvertIf
// ReSharper disable UseDeconstructionOnParameter
namespace TeamCity.CSharpInteractive;

using HostApi;

internal class BuildRunner : IBuildRunner
{
    private readonly IProcessRunner _processRunner;
    private readonly IHost _host;
    private readonly ITeamCityContext _teamCityContext;
    private readonly Func<IBuildContext> _resultFactory;
    private readonly IBuildOutputProcessor _buildOutputProcessor;
    private readonly Func<IProcessMonitor> _monitorFactory;
    private readonly IBuildMessagesProcessor _defaultBuildMessagesProcessor;
    private readonly IBuildMessagesProcessor _customBuildMessagesProcessor;
    private readonly IProcessResultHandler _processResultHandler;

    public BuildRunner(
        IProcessRunner processRunner,
        IHost host,
        ITeamCityContext teamCityContext,
        Func<IBuildContext> resultFactory,
        IBuildOutputProcessor buildOutputProcessor,
        Func<IProcessMonitor> monitorFactory,
        [Tag("default")] IBuildMessagesProcessor defaultBuildMessagesProcessor,
        [Tag("custom")] IBuildMessagesProcessor customBuildMessagesProcessor,
        IProcessResultHandler processResultHandler)
    {
        _processRunner = processRunner;
        _host = host;
        _teamCityContext = teamCityContext;
        _resultFactory = resultFactory;
        _buildOutputProcessor = buildOutputProcessor;
        _monitorFactory = monitorFactory;
        _defaultBuildMessagesProcessor = defaultBuildMessagesProcessor;
        _customBuildMessagesProcessor = customBuildMessagesProcessor;
        _processResultHandler = processResultHandler;
    }

    public IBuildResult Run(ICommandLine commandLine, Action<BuildMessage>? handler = default, TimeSpan timeout = default)
    {
        var buildResult = _resultFactory();
        var startInfo = CreateStartInfo(commandLine);
        var processInfo = new ProcessInfo(startInfo, _monitorFactory(), output => Handle(handler, output, buildResult));
        var result = _processRunner.Run(processInfo, timeout);
        _processResultHandler.Handle(result, handler);
        return buildResult.Create(startInfo, result.ExitCode);
    }

    public async Task<IBuildResult> RunAsync(ICommandLine commandLine, Action<BuildMessage>? handler = default, CancellationToken cancellationToken = default)
    {
        var buildResult = _resultFactory();
        var startInfo = CreateStartInfo(commandLine);
        var processInfo = new ProcessInfo(startInfo, _monitorFactory(), output => Handle(handler, output, buildResult));
        var result = await _processRunner.RunAsync(processInfo, cancellationToken);
        _processResultHandler.Handle(result, handler);
        return buildResult.Create(startInfo, result.ExitCode);
    }

    private IStartInfo CreateStartInfo(ICommandLine commandLine)
    {
        try
        {
            _teamCityContext.TeamCityIntegration = true;
            return commandLine.GetStartInfo(_host);
        }
        finally
        {
            _teamCityContext.TeamCityIntegration = false;
        }
    }

    private void Handle(Action<BuildMessage>? handler, in Output output, IBuildContext context)
    {
        var messages = _buildOutputProcessor.Convert(output, context);
        if (handler != default)
        {
            _customBuildMessagesProcessor.ProcessMessages(output, messages, handler);
        }
        else
        {
            _defaultBuildMessagesProcessor.ProcessMessages(output, messages, EmptyHandler);
        }
    }

    private static void EmptyHandler(BuildMessage obj) { }
}