namespace BehaviourTree.decorators
{
    public class Succeeder : Node
    {
        public override Status Tick(Context context)
        {
            base.Children[0].Tick(context);
            return Status.Success;
        }
    }
}
