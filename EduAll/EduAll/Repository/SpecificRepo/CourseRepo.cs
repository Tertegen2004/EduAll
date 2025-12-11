using EduAll.Data;
using EduAll.Domain;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Repository.SpecificRepo
{
    public class CourseRepo : Repository<Course>, ICourseRepo
    {
        private readonly AppDbContext context;

        public CourseRepo(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<Course> GetAllCourseInfo(int id)
        {
            var course = await context.Courses
                    .Include(c => c.Category)      
                    .Include(c => c.Instructor)   
                    .Include(c => c.Sections)      
                        .ThenInclude(s => s.Lessons)   
                    .Include(c => c.Sections)
                        .ThenInclude(s => s.Quizzes)  
                    .FirstOrDefaultAsync(c => c.Id == id);
            return course;
        }

        public async Task<Course> GetCourseWithInfo(int id)
        {
            var Course = await context.Courses
                .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);
            return Course;
        }

        public async Task<Course> GetCourseWithQuizzes(int id)
        {
            var Course = await context.Courses
                .Include(c => c.Sections)
                .ThenInclude(q => q.Quizzes)
                .ThenInclude(q => q.Questions)
                .ThenInclude(o => o.Options)
                .FirstOrDefaultAsync(c => c.Id == id);

            return Course;
        }
    }
}
