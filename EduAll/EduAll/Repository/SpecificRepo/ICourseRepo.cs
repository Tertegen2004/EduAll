using EduAll.Domain;

namespace EduAll.Repository.SpecificRepo
{
    public interface ICourseRepo:IRepository<Course>
    {
        Task<Course> GetCourseWithInfo(int id);
        Task<Course> GetCourseWithQuizzes(int id);
        Task<Course> GetAllCourseInfo(int id);
    }
}
