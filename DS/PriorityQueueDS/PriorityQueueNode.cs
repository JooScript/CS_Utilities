namespace Utils.DS.PriorityQueueDS
{
    public class PriorityQueueNode
    {
        public string Name { get; set; }
        public int Priority { get; set; }

        public PriorityQueueNode(string name, int priority)
        {
            Name = name;
            Priority = priority;
        }
    }
}