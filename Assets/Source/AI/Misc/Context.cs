using System;
using System.Collections.Generic;

namespace BehaviourTree
{
    /// <summary>
    /// Used for storing arbitrary data points necessary for node execution
    /// </summary>
    public class Context
    {
        public Dictionary<string, object> data { get; private set; }

        public Context()
        {
            this.data = new Dictionary<string, object>();
            this.data["random"] = new Random();
        }
    }
}
