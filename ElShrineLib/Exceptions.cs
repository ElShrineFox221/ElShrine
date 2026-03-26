namespace ElShrine;

public class ValidationFailedException(string message) : Exception(message);
public class PluginException(string message) : Exception(message);

public class LocalizationException(string message) : Exception(message);
public class ProcessException(string message) : Exception(message);