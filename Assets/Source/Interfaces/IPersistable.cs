public interface IPersistable
{
    string Hash { get; }
    bool IsPersistable { get; }
    bool PersistBetweenScenes { get; }

    void OnEnter(Context context);
    Context GetContext();
}
