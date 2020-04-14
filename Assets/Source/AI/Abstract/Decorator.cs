namespace BehaviourTree
{
    public abstract class Decorator : Node
    {
        public Node child { get; private set; }
    }
}
