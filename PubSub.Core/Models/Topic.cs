namespace PubSub.Core.Models;

public struct Topic
{
    public string Name { get; set; }
    public Topic(string name)
    {
        Name = name;
    }
}
