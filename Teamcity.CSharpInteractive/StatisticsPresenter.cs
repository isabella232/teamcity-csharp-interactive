// ReSharper disable ClassNeverInstantiated.Global
namespace Teamcity.CSharpInteractive
{
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    internal class StatisticsPresenter : IPresenter<IStatistics>
    {
        private readonly ILog<StatisticsPresenter> _log;

        public StatisticsPresenter(ILog<StatisticsPresenter> log) => _log = log;

        public void Show(IStatistics statistics)
        {
            foreach (var error in statistics.Errors)
            {
                _log.Info(new []{ new Text(error, Color.Error)});
            }
            
            foreach (var warning in statistics.Warnings)
            {
                _log.Info(new []{ new Text(warning, Color.Warning)});
            }

            if (statistics.Warnings.Count > 0)
            {
                _log.Info(new []{new Text($"  {statistics.Warnings.Count} Warning(s)")});
            }
            
            if (statistics.Errors.Count > 0)
            {
                _log.Info(new []{new Text($"  {statistics.Errors.Count} Error(s)", Color.Error)});
            }
            
            _log.Info(Text.NewLine, new Text($"Time Elapsed {statistics.TimeElapsed:g}"));
        }
    }
}