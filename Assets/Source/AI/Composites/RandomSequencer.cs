﻿using System;
using System.Linq;

namespace BehaviourTree.composites
{
    public class RandomSequencer : Composite
    {
        public override Status Tick(Context context)
        {
            Status s;

            foreach(int i in Enumerable.Range(0, base.children.Length).OrderBy(x => ((Random)context.data["random"]).Next()))
            {
                s = base.children[i].Tick(context);

                if (s != Status.Success)
                    return s;
            }

            return Status.Success;
        }
    }
}
