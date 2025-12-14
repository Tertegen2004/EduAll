using EduAll.Constant;
using EduAll.Domain;
using EduAll.Repository;
using EduAll.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;

namespace EduAll.Controllers
{
    public class CourseController : Controller
    {
        private readonly IUniteOfWork unite;
        private readonly UserManager<AppUser> usermanager;

        public CourseController(IUniteOfWork unite,UserManager<AppUser>usermanager)
        {
            this.unite = unite;
            this.usermanager = usermanager;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string search, string sortBy = "Newest", int page = 1)
        {


            
            var userId = usermanager.GetUserId(User);


            var wishlistCourseIds = new HashSet<int>();
            if (userId != null)
            {
                
                var wishlists = await unite.Wishlist.GettAll()
                                        .Where(w => w.UserId == userId)
                                        .Select(w => w.CourseId)
                                        .ToListAsync();
                wishlistCourseIds = wishlists.ToHashSet();
            }

            int pageSize = 9; // عدد الكورسات في الصفحة

           
            var query = unite.Course.GettAll()
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .Include(c => c.Sections).ThenInclude(s => s.Lessons)
                .AsQueryable();

            
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search) || c.Category.Name.Contains(search));
            }

            
            query = sortBy switch
            {
                "Popular" => query.OrderByDescending(c => c.Enrollments.Count),
                "Trending" => query.OrderByDescending(c => c.Price), 
                _ => query.OrderByDescending(c => c.Id) 
            };

            
            int totalCourses = await query.CountAsync();
            var coursesData = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var enrolledCourseIds = new HashSet<int>();
            if (userId != null)
            {
                enrolledCourseIds = (await unite.Enrollment.GettAll()
                    .Where(e => e.UserId == userId)
                    .Select(e => e.CourseId)
                    .ToListAsync()).ToHashSet();
            }

            var coursesVM = coursesData.Select(c => new HomeCourse_vm
            {
                Id = c.Id,
                Title = c.Title,
                ImgUrl = c.ImgUrl,
                CategoryName = c.Category?.Name ?? "General",
                InstructorName = c.Instructor != null ? $"{c.Instructor.FirstName} {c.Instructor.LastName}" : "Unknown",
                InstructorImg = c.Instructor?.ImgUrl,
                Price = c.Price,
                LessonsCount = c.Sections.SelectMany(s => s.Lessons).Count(),
                DurationHours = c.Duration, // حساب تقريبي
                StudentsCount = c.Enrollments.Count,
                IsEnrolled = enrolledCourseIds.Contains(c.Id),
                IsWishlisted = wishlistCourseIds.Contains(c.Id),
                Level = c.Level.ToString()
            }).ToList();

            var model = new CoursesPage_vm
            {
                Courses = coursesVM,
                SearchQuery = search,
                SortBy = sortBy,
                TotalCourses = totalCourses,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCourses / (double)pageSize)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = usermanager.GetUserId(User);

            var course = await unite.Course.GettAll()
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .Include(c => c.Reviews).ThenInclude(r => r.User) 
                .Include(c => c.Sections).ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            bool isEnrolled = userId != null && course.Enrollments.Any(e => e.UserId == userId);
            bool isWishlisted = userId != null && await unite.Wishlist.GettAll().AnyAsync(w => w.UserId == userId && w.CourseId == id);

            var instructorCourses = await unite.Course.GettAll(c => c.InstructorId == course.InstructorId)
                                        .Include(c => c.Enrollments).Include(c => c.Reviews).ToListAsync();

            var model = new CourseDetails_vm
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                ImgUrl = string.IsNullOrEmpty(course.ImgUrl) ? "/assets/images/thumbs/course-details-img.png" : course.ImgUrl,
                Price = course.Price,
                Level = course.Level.ToString(),
                Duration = $"{course.Duration} Hours", 

                IsEnrolled = isEnrolled,
                IsWishlisted = isWishlisted,

                LessonsCount = course.Sections.SelectMany(s => s.Lessons).Count(),
                StudentsCount = course.Enrollments.Count,
                QuizzesCount = 5, 
                Rating = (course.Reviews.Any()) ? course.Reviews.Average(r => r.Rating) : 0,
                ReviewsCount = course.Reviews.Count,


                InstructorId = course.InstructorId,
                InstructorName = $"{course.Instructor.FirstName} {course.Instructor.LastName}",
                InstructorImg = string.IsNullOrEmpty(course.Instructor.ImgUrl) ? "/assets/images/thumbs/details-instructor.png" : course.Instructor.ImgUrl,
                InstructorJob = course.Instructor.JobTitle ?? "Instructor",
                InstructorBio = course.Instructor.Bio ?? "Expert Instructor",
                InstructorCoursesCount = instructorCourses.Count,
                InstructorStudentsCount = instructorCourses.Sum(c => c.Enrollments.Count),
                InstructorRating = (instructorCourses.SelectMany(c => c.Reviews).Any()) ? instructorCourses.SelectMany(c => c.Reviews).Average(r => r.Rating) : 0,

                Sections = course.Sections.Select(s => new Section_vm
                {
                    Title = s.Title,
                    Lessons = s.Lessons.Select(l => new Lesson_vm
                    {
                        Title = l.Title,
                        Duration = "10:00" 
                    }).ToList()
                }).ToList(),

                Reviews = course.Reviews.OrderByDescending(r => r.CreatedAt).Take(5).Select(r => new Review_vm
                {
                    ReviewerName = (r.User != null) ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown User",
                    ReviewerImg = r.User.ImgUrl ?? "/assets/images/thumbs/reviewer-img1.png",
                    Comment = r.Comment,
                    Rating = r.Rating,
                    Date = r.CreatedAt
                }).ToList()
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Watch(int id, int? lessonId)
        {
            var userId = usermanager.GetUserId(User);


            var isEnrolled = await unite.Enrollment.GettAll()
                .AnyAsync(e => e.CourseId == id && e.UserId == userId);

            if (!isEnrolled)
            {
                TempData["ErrorMessage"] = "You need to enroll in this course first.";
                return RedirectToAction("Details", new { id = id });
            }

     
            var course = await unite.Course.GettAll()
                .Include(c => c.Instructor)
                .Include(c => c.Sections).ThenInclude(s => s.Lessons)
                .Include(c => c.Sections).ThenInclude(s => s.Quizzes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

 
            var courseQuizIds = course.Sections
                                      .SelectMany(s => s.Quizzes)
                                      .Select(q => q.Id)
                                      .ToList();

            // بنسأل الداتا بيز: هاتلي الـ QuizId اللي الطالب نجح فيه ويكون موجود في القائمة دي
            var passedQuizIds = await unite.QuizSubmission.GettAll()
                .Where(qs => qs.StudentId == userId &&
                             courseQuizIds.Contains(qs.QuizId) &&
                             qs.IsPassed) // شرط النجاح
                .Select(qs => qs.QuizId)
                .ToListAsync();

            // جلب الدروس المكتملة
            var completedLessonIds = await unite.LessonProgress.GettAll()
                .Where(lp => lp.UserId == userId && lp.CourseId == id)
                .Select(lp => lp.LessonId)
                .ToListAsync();

            var enrollment = await unite.Enrollment.GettAll()
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == id);

            var orderedSections = course.Sections.OrderBy(s => s.Order).ToList();
            Lesson currentLesson = null;

            if (lessonId.HasValue)
            {
                currentLesson = course.Sections.SelectMany(s => s.Lessons).FirstOrDefault(l => l.Id == lessonId);
            }
            else
            {
                currentLesson = orderedSections.FirstOrDefault()?.Lessons.OrderBy(l => l.Order).FirstOrDefault();
            }

            if (currentLesson == null)
            {
                TempData["ErrorMessage"] = "This course has no content yet.";
                return RedirectToAction("Index", "MyCourses");
            }

            
            var sectionsVm = new List<Section_vm>();
            bool unlockNextSection = true; // أول سكشن دايماً مفتوح

            foreach (var section in orderedSections)
            {
                // السكشن ده مفتوح لو المتغير true
                // (إلا لو هو أول سكشن فهو دائماً مفتوح)
                bool isLocked = !unlockNextSection;

                // تجهيز الفيو مودل
                var secVm = new Section_vm
                {
                    Id = section.Id,
                    Title = section.Title,
                    IsLocked = isLocked,

                    Lessons = section.Lessons.OrderBy(l => l.Order).Select(l => new Lesson_vm
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Duration = l.Duration,
                        IsActive = l.Id == currentLesson.Id,
                        IsCompleted = completedLessonIds.Contains(l.Id)
                    }).ToList(),

                    Quizzes = section.Quizzes.Select(q => new Quiz_vm
                    {
                        Id = q.Id,
                        Title = q.Title,
                        IsCompleted = passedQuizIds.Contains(q.Id)
                    }).ToList()
                };

                sectionsVm.Add(secVm);

                // --- تحديث حالة السكشن القادم ---
                if (section.Quizzes != null && section.Quizzes.Any())
                {
                    // هل نجح في كل كويزات السكشن الحالي؟
                    bool passedAll = section.Quizzes.All(q => passedQuizIds.Contains(q.Id));

                    // لو نجح في كله -> افتح اللي بعده
                    // لو رسب في واحد -> اقفل اللي بعده
                    unlockNextSection = passedAll;
                }
                // لو السكشن الحالي مفيهوش كويزات، unlockNextSection هتفضل زي ما هي (مفتوحة)
            }

            var targetSectionVm = sectionsVm.FirstOrDefault(s => s.Lessons.Any(l => l.Id == currentLesson.Id));
            if (targetSectionVm != null && targetSectionVm.IsLocked)
            {
                TempData["ErrorMessage"] = "Please complete previous quizzes to unlock this section.";

                // ارجع لآخر درس متاح
                var lastUnlocked = sectionsVm.LastOrDefault(s => !s.IsLocked);
                var safeLesson = lastUnlocked?.Lessons.LastOrDefault(); // أو FirstOrDefault
                if (safeLesson != null)
                    return RedirectToAction("Watch", new { id = id, lessonId = safeLesson.Id });
            }

 
            var allLessons = orderedSections.SelectMany(s => s.Lessons.OrderBy(l => l.Order)).ToList();
            var currentIndex = allLessons.FindIndex(l => l.Id == currentLesson.Id);
            int? prevId = (currentIndex > 0) ? allLessons[currentIndex - 1].Id : null;
            int? nextId = (currentIndex < allLessons.Count - 1) ? allLessons[currentIndex + 1].Id : null;

            var model = new WatchCourse_vm
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                CompletedLessonIds = completedLessonIds,
                StudentProgress = enrollment?.Progress ?? 0,
                CurrentLessonId = currentLesson.Id,
                CurrentLessonTitle = currentLesson.Title,
                CurrentVideoUrl = currentLesson.VideoUrl,
                LessonDescription = currentLesson.Content,
                InstructorName = $"{course.Instructor.FirstName} {course.Instructor.LastName}",
                InstructorImg = course.Instructor.ImgUrl ?? "/assets/images/thumbs/user-img1.png",
                InstructorJob = course.Instructor.JobTitle ?? "Instructor",
                Sections = sectionsVm,
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> MarkLessonComplete(int lessonId, int courseId)
        {
            var userId = usermanager.GetUserId(User);
            if (userId == null) return Json(new { success = false });

            var existingProgress = await unite.LessonProgress.GettAll()
                .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);

            if (existingProgress == null)
            {
                var progressEntry = new LessonProgress
                {
                    UserId = userId,
                    LessonId = lessonId,
                    CourseId = courseId,
                    IsCompleted = true
                };
                await unite.LessonProgress.Create(progressEntry);
            }

            var totalLessons = await unite.Lesson.GettAll()
                .Include(l => l.Section)
                .Where(l => l.Section.CourseId == courseId)
                .CountAsync();

            var completedLessonsCount = await unite.LessonProgress.GettAll()
                .Where(lp => lp.UserId == userId && lp.CourseId == courseId && lp.IsCompleted)
                .CountAsync();

            int newPercentage = (totalLessons == 0) ? 0 : (int)((double)completedLessonsCount / totalLessons * 100);

            var enrollment = await unite.Enrollment.GettAll()
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment != null)
            {
                enrollment.Progress = newPercentage;
                if (newPercentage == 100)
                {
                    enrollment.EnrolledAt = DateTime.Now; 
                }
                await unite.Enrollment.Update(enrollment.Id); 
            }

            return Json(new { success = true, progress = newPercentage });
        }


        [HttpPost]
        [Authorize] 
        public async Task<IActionResult> AddReview(CourseDetails_vm mainModel)
        {
            var model = mainModel.NewReview;
            if (model == null)
            {
                return RedirectToAction("Details", new { id = model.CourseId });
            }

            var userId = usermanager.GetUserId(User);

            var review = new Review
            {
                CourseId = model.CourseId,
                UserId = userId,
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedAt = DateTime.Now
            };

            await unite.Review.Create(review);

            return RedirectToAction("Details", new { id = model.CourseId });
        }

        [HttpPost]
        [Authorize]

        public async Task<IActionResult> SendReport(int courseId, string message)
        {
            var userId = usermanager.GetUserId(User);

            if (userId == null)
                return Json(new { success = false, message = "Please login first." });

            if (string.IsNullOrWhiteSpace(message))
                return Json(new { success = false, message = "Message cannot be empty." });


            var suggestion = new CourseSuggestion
            {
                UserId = userId,
                CourseId = courseId,
                Content = message,
                Status = SuggestionState.Pending, 
                CreatedAt = DateTime.Now
            };

            try
            {

                await unite.CourseSuggestion.Create(suggestion);


                return Json(new { success = true, message = "Suggestion sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error sending suggestion." });
            }
        }
    }
}
