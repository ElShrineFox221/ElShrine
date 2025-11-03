namespace ElShrine.Async
{
    public interface IDataValidator<in TData>
    {
        public void Initialize(TData data);
        public bool Validate(TData data);
    }
}
