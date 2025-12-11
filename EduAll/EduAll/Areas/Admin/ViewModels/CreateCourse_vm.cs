using EduAll.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduAll.Areas.Admin.ViewModels
{
    public class CreateCourse_vm
    {
        public int Id { get; set; }
        [DisplayName("Course title")]
        [Required]
        public string Title { get; set; }
        [DisplayName("Course Description")]
        [Required]
        public string Description { get; set; }
        [DisplayName("Course Image")]
        public IFormFile ImgUrl { get; set; }
        [DisplayName("Course Price")]
        [Required]
        public decimal Price { get; set; }
        [DisplayName("Course Level")]
        [Required]
        public string Level { get; set; }
        [DisplayName("Course Language")]
        [Required]

        public string Language { get; set; }
        [DisplayName("Course Duration")]
        [Required]
        public int Duration { get; set; }

        // FK
        [DisplayName("Course Instructor")]
        public string? InstructorId { get; set; }
        [Required]
        [DisplayName("Course Category")]
        public int CategoryId { get; set; }
    }
}
