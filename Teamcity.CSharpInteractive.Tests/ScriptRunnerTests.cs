namespace Teamcity.CSharpInteractive.Tests
{
    using System.Collections.Generic;
    using Moq;
    using Shouldly;
    using Xunit;

    public class ScriptRunnerTests
    {
        private readonly Mock<ICommandSource> _commandSource;
        private readonly Mock<ICommandsRunner> _commandsRunner;
        private readonly Mock<IStatistics> _statistics;
        private readonly Mock<IPresenter<IStatistics>> _statisticsPresenter;
        private readonly Mock<ILog<ScriptRunner>> _log;
        private readonly IEnumerable<ICommand> _commands;

        public ScriptRunnerTests()
        {
            _log = new Mock<ILog<ScriptRunner>>();
            _commands = Mock.Of<IEnumerable<ICommand>>();
            _commandSource = new Mock<ICommandSource>();
            _commandSource.Setup(i => i.GetCommands()).Returns(_commands);
            _commandsRunner = new Mock<ICommandsRunner>();
            _statistics = new Mock<IStatistics>();
            _statisticsPresenter = new Mock<IPresenter<IStatistics>>();
        }
        
        [Fact]
        public void ShouldProvideMode()
        {
            // Given
            var runner = CreateInstance();

            // When
            var mode = runner.InteractionMode;
            
            // Then
            mode.ShouldBe(InteractionMode.Script);
        }

        [Theory]
        [MemberData(nameof(Data))]
        internal void ShouldRun(CommandResult[] results, string[] errors, string[] warnings, ExitCode expectedExitCode)
        {
            // Given
            var runner = CreateInstance();
            _commandsRunner.Setup(i => i.Run(_commands)).Returns(results);
            _statistics.SetupGet(i => i.Errors).Returns(errors);
            _statistics.SetupGet(i => i.Warnings).Returns(warnings);
            
            // When
            var actualExitCode = runner.Run();

            // Then
            actualExitCode.ShouldBe(expectedExitCode);
        }
        
        public static IEnumerable<object?[]> Data => new List<object?[]> 
        {
            // Success
            new object[]
            {
                new CommandResult[] { new(new CodeCommand(), null), new(new CodeCommand(), null), new(new ScriptCommand(string.Empty, string.Empty), null)},
                new string[] {},
                new string[] {},
                ExitCode.Success
            },
        };

        private ScriptRunner CreateInstance() =>
            new(
                _log.Object,
                _commandSource.Object,
                _commandsRunner.Object,
                _statistics.Object,
                _statisticsPresenter.Object);
    }
}