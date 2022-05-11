namespace Trackor.Features.TaskList
{
    public class TaskListItem
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public int? ProjectId { get; set; }
        public string Narrative { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public DateOnly Due { get; set; }
    }
}
