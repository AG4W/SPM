namespace BehaviourTree
{
    public abstract class Node
    {
        protected Node[] Children { get; private set; }

        public Node(params Node[] children)
        {
            this.Children = children;
        }
        public abstract Status Tick(Context context);
    }
}
