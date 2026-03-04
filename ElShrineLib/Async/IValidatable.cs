namespace ElShrine.Async;

public interface IValidatable
{
    public void InitializeValidator();
    public bool IsValidated { get; }
}
