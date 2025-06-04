using System.Collections.Generic;

namespace Core.ApplicationServices;

public interface IEventBody
{
    Dictionary<string, object> ToKeyValuePairs();
}