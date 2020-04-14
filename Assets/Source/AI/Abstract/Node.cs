namespace BehaviourTree
{
    public abstract class Node
    {
        public abstract Status Tick(Context context);
    }
}
