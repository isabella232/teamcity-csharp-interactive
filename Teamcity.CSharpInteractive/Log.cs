// ReSharper disable ClassNeverInstantiated.Global

namespace Teamcity.CSharpInteractive
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [ExcludeFromCodeCoverage]
    internal class Log
    {
        internal static int Tabs;
    }

    [ExcludeFromCodeCoverage]
    internal class Log<T> : ILog<T>
    {
        private readonly IStdErr _stdErr;
        private readonly IStatistics _statistics;
        private readonly ISettings _settings;
        private readonly IStdOut _stdOut;

        public Log(
            ISettings settings,
            IStdOut stdOut,
            IStdErr stdErr,
            IStatistics statistics)
        {
            _settings = settings;
            _stdOut = stdOut;
            _stdErr = stdErr;
            _statistics = statistics;
        }
        
        public void Error(params Text[] error)
        {
            _statistics.RegisterError(string.Join("", error.Select(i => i.Value)));
            _stdErr.Write(GetMessage(error, Color.Error));
        }
        
        public void Warning(params Text[] warning)
        {
            _statistics.RegisterWarning(string.Join("", warning.Select(i => i.Value)));
            _stdErr.Write(GetMessage(warning, Color.Warning));
        }

        public void Info(params Text[] message)
        {
            if (_settings.VerbosityLevel >= VerbosityLevel.Normal)
            {
                _stdOut.Write(GetMessage(message, Color.Default));
            }
        }

        public void Trace(params Text[] traceMessage)
        {
            if (_settings.VerbosityLevel >= VerbosityLevel.Trace)
            {
                _stdOut.Write(GetMessage(traceMessage, Color.Trace));
            }
        }

        public IDisposable Block(Text[] block)
        {
            if (_settings.VerbosityLevel < VerbosityLevel.Normal)
            {
                return Disposable.Empty;
            }

            _stdOut.Write(GetMessage(block, Color.Header));
            Log.Tabs++;
            return Disposable.Create(() => Log.Tabs--);

        }

        private static Text[] GetMessage(Text[] message, Color defaultColor)
        {
            var tabsStr = new string(' ', Log.Tabs * 2);
            message = message.Select(i => new Text(i.Value.Replace("\n", "\n" + tabsStr), i.Color)) .ToArray();
            message += Text.NewLine;
            message = new Text(tabsStr) + message;
            return message.WithDefaultColor(defaultColor);
        }
    }
}