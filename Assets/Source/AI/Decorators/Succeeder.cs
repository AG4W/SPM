namespace BehaviourTree.decorators
{
    public class Succeeder : Decorator
    {
        public override Status Tick(Context context)
        {
            base.child.Tick(context);
            return Status.Success;
        }
    }
}
