using EduAll.Domain;
using EduAll.Repository.SpecificRepo;

namespace EduAll.Repository
{
    public interface IUniteOfWork
    {
        ICourseRepo Course { get; }
        IQuizRepo Quiz { get; }
        IRepository<AppUser> Instructor { get; }
        IRepository<Section> Section { get; }
        IRepository<Question> Question { get; }
        IRepository<AnswerOption> AnswerOption { get; }
        IRepository<Lesson> Lesson { get; }
        IRepository<AppUser> Student { get; }
        IRepository<Enrollment> Enrollment { get; }
        IRepository<Wishlist> Wishlist { get; }
        IRepository<CourseSuggestion> CourseSuggestion { get; }
        IRepository<Order> Order { get; }
        IRepository<Cart> Cart { get; }
        IRepository<Category> Category { get; }
        IRepository<CartItem> CartItem { get; }
        IRepository<LessonProgress> LessonProgress { get; }
        IRepository<Review> Review { get; }
        IRepository<QuizSubmission> QuizSubmission { get; }
    }
}
