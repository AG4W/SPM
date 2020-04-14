namespace BehaviourTree
{
    public class Sequencer : Composite
    {
        public override Status Tick(Context context)
        {
            Status s;

            for (int i = 0; i < base.children.Length; i++)
            {
                s = base.children[i].Tick(context);

                if (s != Status.Success)
                    return s;
            }

            return Status.Success;
        }
    }
}
