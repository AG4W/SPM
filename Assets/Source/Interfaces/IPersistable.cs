public interface IPersistable
{
    string Hash { get; }
    bool IsPersistable { get; }

    void OnEnter(Context context);
    Context GetContext();
}
