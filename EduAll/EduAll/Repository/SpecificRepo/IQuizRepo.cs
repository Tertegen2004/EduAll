using EduAll.Domain;

namespace EduAll.Repository.SpecificRepo
{
    public interface IQuizRepo : IRepository<Quiz>
    {
        Task<Quiz> GetQuizWithInfo(int id);
    }
}
