namespace BehaviourTree
{
    public class Sequencer : Node
    {
        public override Status Tick(Context context)
        {
            Status s;

            for (int i = 0; i < base.Children.Length; i++)
            {
                s = base.Children[i].Tick(context);

                if (s != Status.Success)
                    return s;
            }

            return Status.Success;
        }
    }
}
