using System.Collections.Generic;

public class Context
{
    public Dictionary<string, object> data { get; private set; }

    public Context()
    {
        data = new Dictionary<string, object>();
    }
}
