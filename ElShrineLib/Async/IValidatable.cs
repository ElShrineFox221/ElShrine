namespace ElShrine.Async;

public interface IValidatable
{
    public void UpdateValidator();
    public bool IsValidated { get; }
}
