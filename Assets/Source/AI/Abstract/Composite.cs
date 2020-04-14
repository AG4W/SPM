namespace BehaviourTree
{
    public abstract class Composite : Node
    {
        public Node[] children { get; private set; }
    }
}
