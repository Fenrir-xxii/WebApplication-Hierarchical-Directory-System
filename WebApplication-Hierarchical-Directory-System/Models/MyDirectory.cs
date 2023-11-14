namespace WebApplication_Hierarchical_Directory_System.Models
{
    public class MyDirectory
    {
        public MyDirectory()
        {
            Childrens = new List<MyDirectory>();
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public MyDirectory? Parent { get; set; }
        public int? ParentId { get; set; }
        public ICollection<MyDirectory> Childrens { get; set; }
    }
}
