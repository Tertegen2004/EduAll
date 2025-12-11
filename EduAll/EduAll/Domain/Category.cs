namespace EduAll.Domain
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? Img { get; set; }

        // Nav Prop
        public List<Course> Courses { get; set; }
    }
}
