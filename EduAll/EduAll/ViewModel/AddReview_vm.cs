using System.ComponentModel.DataAnnotations;

namespace EduAll.ViewModel
{
    public class AddReview_vm
    {
        public int CourseId { get; set; }

        [Range(1, 5, ErrorMessage = "Please select a rating")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please write a comment")]
        public string Comment { get; set; }
    }
}
