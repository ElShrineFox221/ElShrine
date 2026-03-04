namespace ElShrine.Modules.Log;

public class LogScopeTreeOverflowException(int max) : Exception($"Maximum scope depth reached: {max}");
