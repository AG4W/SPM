namespace BehaviourTree
{
    /// <summary>
    /// Actual implementation nodes inherit from Leaf and implement Execute.
    /// </summary>
    public abstract class Leaf : Node
    {
        public sealed override Status Tick(Context context) => Execute(context);

        public abstract Status Execute(Context context);
    }
}
