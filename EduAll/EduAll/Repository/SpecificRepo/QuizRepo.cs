using EduAll.Data;
using EduAll.Domain;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Repository.SpecificRepo
{
    public class QuizRepo : Repository<Quiz>, IRepository<Quiz>, IQuizRepo
    {
        private readonly AppDbContext context;

        public QuizRepo(AppDbContext context):base(context)
        {
            this.context = context;
        }
        public async Task<Quiz> GetQuizWithInfo(int id)
        {
            var quiz = await context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == id);

            return quiz;
        }
    }
}
