namespace PubSub.Core.DomainModel.Topics;

public class Topic
{
    public string Name { get; set; }
    public Topic(string name)
    {
        Name = name;
    }
}
