using System.Diagnostics;

namespace ElShrine.ECommand
{
    public interface ICommandResult
    {
        public bool Parsed { get; }
        public bool Excuted { get; }

        public bool HasWarning { get; }
        public bool HasError { get; }
        public string[] Warnings { get; }
        public Exception? TerminateException { get; }
    }
}
