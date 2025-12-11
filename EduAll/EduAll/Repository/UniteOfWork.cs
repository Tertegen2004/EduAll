using EduAll.Data;
using EduAll.Domain;
using EduAll.Repository.SpecificRepo;

namespace EduAll.Repository
{
    public class UniteOfWork : IUniteOfWork
    {
        private readonly AppDbContext context;

        public UniteOfWork(AppDbContext context)
        {
            this.context = context;
            Course = new CourseRepo(context);
            Instructor = new Repository<AppUser>(context);
            Section = new Repository<Section>(context);
            Quiz = new QuizRepo(context);
            Question = new Repository<Question>(context);
            Lesson = new Repository<Lesson>(context);
            AnswerOption = new Repository<AnswerOption>(context);
            Category = new Repository<Category>(context);
            Student = new Repository<AppUser>(context);
            Enrollment = new Repository<Enrollment>(context);
            Wishlist = new Repository<Wishlist>(context);
            CourseSuggestion = new Repository<CourseSuggestion>(context);
            Order = new Repository<Order>(context);
            Cart = new Repository<Cart>(context);
            CartItem = new Repository<CartItem>(context);
            LessonProgress = new Repository<LessonProgress>(context);
            Review = new Repository<Review>(context);
            QuizSubmission = new Repository<QuizSubmission>(context);
        }

        public ICourseRepo Course { get; }
        public IRepository<AppUser> Instructor { get; }
        public IRepository<Section> Section { get; }
        public IQuizRepo Quiz { get; }
        public IRepository<Question> Question { get; }
        public IRepository<AnswerOption> AnswerOption { get; }
        public IRepository<Lesson> Lesson { get; }
        public IRepository<AppUser> Student { get; }
        public IRepository<Enrollment> Enrollment { get; }
        public IRepository<Wishlist> Wishlist { get; }
        public IRepository<CourseSuggestion> CourseSuggestion { get; }
        public IRepository<Order> Order { get; }
        public IRepository<Cart> Cart { get; }
        public IRepository<CartItem> CartItem { get; }
        public IRepository<Category> Category { get; }
        public IRepository<LessonProgress> LessonProgress { get; }
        public IRepository<Review> Review { get; }
        public IRepository<QuizSubmission> QuizSubmission { get; }
    }
}
