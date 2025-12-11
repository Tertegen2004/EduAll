using EduAll.Areas.Admin.ViewModels;
using EduAll.Constant;
using EduAll.Domain;
using EduAll.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace EduAll.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class Admin_CourseController : Controller
    {
        private readonly IUniteOfWork unite;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly UserManager<AppUser> usermanager;

        public Admin_CourseController(IUniteOfWork unite, IWebHostEnvironment webHostEnvironment,UserManager<AppUser> usermanager)
        {
            this.unite = unite;
            this.webHostEnvironment = webHostEnvironment;
            this.usermanager = usermanager;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            // 1. التجهيز (شيلنا الـ Includes لأننا هنستخدم Select تحت)
            var coursesQuery = unite.Course.GettAll().AsNoTracking();

            // 2. تطبيق البحث
            if (!string.IsNullOrEmpty(searchString))
            {
                coursesQuery = coursesQuery.Where(c => c.Title.Contains(searchString)
                                                    || c.Instructor.FirstName.Contains(searchString)
                                                    || c.Category.Name.Contains(searchString));
            }

            // 3. التحويل (Projection) - وهنا الحل
            var viewModelQuery = coursesQuery.Select(c => new Course_vm
            {
                Id = c.Id,
                Title = c.Title,
                ImgUrl = c.ImgUrl,
                Price = c.Price,
                CategoryName = c.Category.Name,
                InstructorName = $"{c.Instructor.FirstName} {c.Instructor.LastName}",

                // ✅ التعديل هنا: استخدام SelectMany للعد بشكل صحيح في SQL
                LessonsCount = c.Sections.SelectMany(s => s.Lessons).Count()
            });

            // 4. تنفيذ الباجينيشن
            int pageSize = 10;
            var paginatedModel = await Pagination<Course_vm>.CreateAsync(viewModelQuery, pageNumber ?? 1, pageSize);

            ViewData["CurrentFilter"] = searchString;

            return View(paginatedModel);
        }
        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            var instructors = await usermanager.GetUsersInRoleAsync(Roles.Instructor.ToString());
            var Categories =  unite.Category.GettAll();

            ViewBag.Levels = new SelectList(new List<string> { "Beginner", "Intermdiate", "High" });
            ViewBag.Language = new SelectList(new List<string> { "Arabic", "English" });
            var instructorList = instructors.Select(i => new {
                Id = i.Id,
                FullName = i.FirstName + " " + i.LastName
            });

            ViewBag.Instructors = new SelectList(instructorList, "Id", "FullName");
            ViewBag.Categories = new SelectList(Categories,"Id","Name");

            return View(new CreateCourse_vm());
        }
        [HttpPost]
        public async Task<IActionResult> CreateCourse(CreateCourse_vm model)
        {
            if (model.ImgUrl == null)
            {
                ModelState.AddModelError("ImgUrl", "Course Image is required.");
            }
            if (ModelState.IsValid)
             {
                string? dbpath = null;
                if (model.ImgUrl != null)
                {
                    string uploadDir = Path.Combine(webHostEnvironment.WebRootPath, "images/courses");

                    string fileName = Guid.NewGuid().ToString() + "_" + model.ImgUrl.FileName;

                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImgUrl.CopyToAsync(fileStream);
                    }

                    dbpath = "/images/courses/" + fileName;
                }

                var Course = new Course
                {
                    Title = model.Title,
                    Price = model.Price,
                    Duration = model.Duration,
                    ImgUrl = dbpath,
                    CategoryId=model.CategoryId,
                    InstructorId=model.InstructorId,
                    Level=model.Level,
                    Language=model.Language,
                    Description=model.Description,
                };
                await unite.Course.Create(Course);
                return RedirectToAction(nameof(CourseContent), new {CourseId=Course.Id});
            }
            var instructors = await usermanager.GetUsersInRoleAsync(Roles.Instructor.ToString());

            var Categories = unite.Category.GettAll();

            ViewBag.Levels = new SelectList(new List<string> { "Beginner", "Intermdiate", "High" });
            ViewBag.Language = new SelectList(new List<string> { "Arabic", "English" });
            var instructorList = instructors.Select(i => new {
                Id = i.Id,
                FullName = i.FirstName + " " + i.LastName
            });

            ViewBag.Instructors = new SelectList(instructorList, "Id", "FullName");
            ViewBag.Categories = new SelectList(Categories, "Id", "Name");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await unite.Course.GettAll()
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons) 
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Quizzes)
                        .ThenInclude(q => q.Questions)
                            .ThenInclude(o => o.Options) 
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var filesToDelete = new List<string>();

                if (!course.Sections.Any())
                {
                    foreach (var section in course.Sections)
                    {

                        if (section.Lessons != null && section.Lessons.Any())
                        {
                            foreach (var lesson in section.Lessons)
                            {
                                if (!string.IsNullOrEmpty(lesson.VideoUrl))
                                {
                                    var videoPath = Path.Combine(webHostEnvironment.WebRootPath, lesson.VideoUrl.TrimStart('/'));
                                    if (System.IO.File.Exists(videoPath)) System.IO.File.Delete(videoPath);
                                }
                            }
                            await unite.Lesson.DeleteAll(section.Lessons);
                        }

                        if (section.Quizzes != null && section.Quizzes.Any())
                        {
                            foreach (var quiz in section.Quizzes)
                            {
                                if (quiz.Questions != null)
                                {
                                    foreach (var question in quiz.Questions)
                                    {

                                        if (question.Options != null)
                                            await unite.AnswerOption.DeleteAll(question.Options);
                                    }

                                    await unite.Question.DeleteAll(quiz.Questions);
                                }
                            }
                            await unite.Quiz.DeleteAll(section.Quizzes);
                        }
                    }


                    await unite.Section.DeleteAll(course.Sections);
                }

                if (!string.IsNullOrEmpty(course.ImgUrl))
                {
                    filesToDelete.Add(Path.Combine(webHostEnvironment.WebRootPath, course.ImgUrl.TrimStart('/')));
                }

                await unite.Course.Delete(course.Id);
                foreach (var filePath in filesToDelete)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                TempData["SuccessMessage"] = "Course deleted successfully along with all its contents.";

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting course. Make sure there are no enrolled students yet. Error: " + ex.Message;


            }
            return RedirectToAction(nameof(Index));

        }
        [HttpGet]
        public async Task<IActionResult> CourseContent(int courseid)
        {
            var Course = await unite.Course.GettAll()
                .Include(c => c.Sections)     
                .ThenInclude(s => s.Lessons)     
                .Include(c => c.Sections)
                .ThenInclude(s => s.Quizzes)      
                .FirstOrDefaultAsync(c => c.Id == courseid);

            return View(Course);
        }
        [HttpPost]
        public async Task<IActionResult> AddSection(CreateSection_vm model)
        {
            if (ModelState.IsValid)
            {
                var section = new Section
                {
                    Title = model.Title,
                    CourseId=model.CourseId,
                    Order=unite.Section.GettAll().Count(c=>c.CourseId==model.CourseId)+1
                };

                await unite.Section.Create(section);
                TempData["SuccessMessage"] = "Section Added Successfully.";

                return RedirectToAction(nameof(CourseContent), new {courseId = model.CourseId });
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditSection(int sectionId, int courseId, string title)
        {
            var section = await unite.Section.FindById(sectionId);

            if (section != null)
            {
                section.Title = title;
                await unite.Section.Update(section.Id);

                TempData["SuccessMessage"] = "Section updated successfully.";
            }

            return RedirectToAction(nameof(CourseContent), new { courseId = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSection(int sectionId, int courseId)
        {
            // 1. هات السكشن وهات معاه الدروس بتاعته (مهم جداً Include)
            var section = await unite.Section.GettAll()
                .Include(s => s.Lessons)
                .Include(s=>s.Quizzes)
                .ThenInclude(q=>q.Questions)
                .ThenInclude(q=>q.Options)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section != null)
            {

                // Delete quiz Section
                if (section.Quizzes != null && section.Quizzes.Any())
                {
                    foreach (var quiz in section.Quizzes)
                    {
                        // لو الكويز فيه أسئلة، لازم ننظفها الأول
                        if (quiz.Questions != null && quiz.Questions.Any())
                        {
                            foreach (var question in quiz.Questions)
                            {
                                // مسح الاختيارات
                                if (question.Options != null)
                                {
                                    await unite.AnswerOption.DeleteAll(question.Options);
                                }
                            }
                            // مسح الأسئلة
                            await unite.Question.DeleteAll(quiz.Questions);
                        }
                    }
                    // مسح الكويزات نفسها
                    await unite.Quiz.DeleteAll(section.Quizzes);
                }


                // 2. Delete Video Lesson
                if (section.Lessons != null && section.Lessons.Any())
                {
                    foreach (var lesson in section.Lessons)
                    {
                        if (!string.IsNullOrEmpty(lesson.VideoUrl))
                        {
                            // تحويل المسار النسبي (web path) لمسار حقيقي (server path)
                            var videoPath = Path.Combine(webHostEnvironment.WebRootPath, lesson.VideoUrl.TrimStart('/'));

                            // امسح الملف لو موجود
                            if (System.IO.File.Exists(videoPath))
                            {
                                System.IO.File.Delete(videoPath);
                            }
                        }
                    }
                    // Delete All Lesson in Section

                    await unite.Lesson.DeleteAll(section.Lessons);
                }

                await unite.Section.Delete(section.Id);


                TempData["SuccessMessage"] = "Section and all its lessons deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Section not found!";
            }

            return RedirectToAction(nameof(CourseContent), new { courseId = courseId });
        }
        [HttpPost]
        public async Task<IActionResult> AddLesson(CreateLesson_vm model)
        {
            if (model.VideoUrl == null || model.VideoUrl.Length == 0)
            {
                ModelState.AddModelError("VideoFile", "Video file is required.");
            }
            if (ModelState.IsValid)
            {

                string videoPath = null;

                // 1. رفع الفيديو للسيرفر
                if (model.VideoUrl != null)
                {
                    string uploadDir = Path.Combine(webHostEnvironment.WebRootPath, "videos","courses");

                    // تأكد إن الفولدر موجود
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    // اسم فريد للملف
                    string fileName = Guid.NewGuid().ToString() + "_" + model.VideoUrl.FileName;
                    string filePath = Path.Combine(uploadDir, fileName);

                    // عملية النسخ (ممكن تاخد وقت حسب حجم الفيديو)
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.VideoUrl.CopyToAsync(fileStream);
                    }

                    videoPath = "/videos/courses/" + fileName;
                }
                var lesson = new Lesson
                {
                    Title = model.Title,
                    VideoUrl=videoPath,
                    SectionId = model.SectionId,
                    Order = unite.Lesson.GettAll().Count(l => l.SectionId == model.SectionId) + 1,
                    Duration = "12"
                    
                };

                await unite.Lesson.Create(lesson);
                TempData["SuccessMessage"] = "Lesson Added Successfully.";

            }
            return RedirectToAction(nameof(CourseContent), new { courseId = model.CourseId });
        }

        [HttpPost]
        public async Task<IActionResult> EditLesson(int lessonId, int courseId, string title,IFormFile? VideoUrl)
        {
            var lesson = await unite.Lesson.FindById(lessonId);


            if (lesson != null)
            {
                lesson.Title = title;

                if (VideoUrl != null && VideoUrl.Length > 0)
                {
                    // أ. حذف الفيديو القديم من السيرفر (لتوفير المساحة)
                    if (!string.IsNullOrEmpty(lesson.VideoUrl))
                    {
                        var oldPath = Path.Combine(webHostEnvironment.WebRootPath, lesson.VideoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // ب. رفع الفيديو الجديد
                    string uploadDir = Path.Combine(webHostEnvironment.WebRootPath, "videos","courses");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(VideoUrl.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await VideoUrl.CopyToAsync(stream);
                    }

                    // ج. تحديث المسار في الداتا بيز
                    lesson.VideoUrl = "/videos/courses/" + fileName;
                }

                await unite.Lesson.Update(lesson.Id);
                TempData["SuccessMessage"] = "Lesson updated successfully.";
            }

            return RedirectToAction(nameof(CourseContent), new { courseId = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLesson(int lessonId, int courseId)
        {
            var lesson = await unite.Lesson.FindById(lessonId);

            if (lesson != null)
            {
                // أ. حذف ملف الفيديو من السيرفر (اختياري بس مستحسن)
                if (!string.IsNullOrEmpty(lesson.VideoUrl))
                {
                    var videoPath = Path.Combine(webHostEnvironment.WebRootPath, lesson.VideoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(videoPath))
                    {
                        System.IO.File.Delete(videoPath);
                    }
                }

                // ب. حذف السجل من الداتا بيز
                await unite.Lesson.Delete(lesson.Id);

                TempData["SuccessMessage"] = "Lesson deleted successfully.";
            }

            return RedirectToAction(nameof(CourseContent), new { courseId = courseId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id) 
        {
            var course = await unite.Course.FindById(id);

            if (course == null) return NotFound();

            var model = new CreateCourse_vm
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                Level = course.Level,
                Language = course.Language,
                Duration = course.Duration,
                CategoryId = course.CategoryId,
                InstructorId = course.InstructorId,
            };

            var Instructors = unite.Instructor.GettAll(u => u.JobTitle == null);
            var Categories = unite.Category.GettAll();

            ViewBag.Levels = new SelectList(new List<string> { "Beginner", "Intermdiate", "High" },model.Level);
            ViewBag.Language = new SelectList(new List<string> { "Arabic", "English" },model.Language);
            var instructorList = Instructors.Select(i => new {
                Id = i.Id,
                FullName = i.FirstName + " " + i.LastName
            });

            ViewBag.Instructors = new SelectList(instructorList, "Id", "FullName",model.InstructorId);
            ViewBag.Categories = new SelectList(Categories, "Id", "Name",model.CategoryId);

            ViewBag.CurrentImgUrl = course.ImgUrl;

            return View(nameof(CreateCourse), model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateCourse_vm model)
        {
            if (model.ImgUrl == null)
            {
                ModelState.Remove("ImgUrl");
            }
            if (ModelState.IsValid)
            {
                var course = await unite.Course.FindById(model.Id);

                if (course == null) return NotFound();

                course.Title = model.Title;
                course.Description = model.Description;
                course.Price = model.Price;
                course.Level = model.Level;
                course.Language = model.Language;
                course.Duration = model.Duration;
                course.CategoryId = model.CategoryId;
                course.InstructorId = model.InstructorId;

                if (model.ImgUrl != null)
                {
                    if (!string.IsNullOrEmpty(course.ImgUrl))
                    {
                        var oldPath = Path.Combine(webHostEnvironment.WebRootPath, course.ImgUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    string uploadDir = Path.Combine(webHostEnvironment.WebRootPath, "images/courses");

                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    string fileName = Guid.NewGuid().ToString() + "_" + model.ImgUrl.FileName;
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImgUrl.CopyToAsync(fileStream);
                    }

                    course.ImgUrl = "/images/courses/" + fileName;
                }

                await unite.Course.Update(course.Id); 

                return RedirectToAction(nameof(CourseContent), new { courseId = course.Id });
            }



            var Instructors = unite.Instructor.GettAll(u => u.JobTitle == null);
            var Categories = unite.Category.GettAll();

            var existingCourse = await unite.Course.SelectBy(c => c.Id == model.Id);
            if (existingCourse != null) ViewBag.CurrentImgUrl = existingCourse.ImgUrl;

            ViewBag.Levels = new SelectList(new List<string> { "Beginner", "Intermediate", "High" }, model.Level);
            ViewBag.Language = new SelectList(new List<string> { "Arabic", "English" }, model.Language);

            var instructorList = Instructors.Select(i => new {
                Id = i.Id,
                FullName = i.FirstName + " " + i.LastName
            });

            ViewBag.Instructors = new SelectList(instructorList, "Id", "FullName", model.InstructorId);
            ViewBag.Categories = new SelectList(Categories, "Id", "Name", model.CategoryId);

            return View(nameof(CreateCourse), model);
        }

        public async Task<IActionResult> CreateQuiz(int courseid)
        {
            var coursesections = await unite.Course.GettAll(null, c => c.Sections)
                .FirstOrDefaultAsync(c=>c.Id==courseid);

            if (!coursesections.Sections.Any())
            {
                TempData["ErrorMessage"] = "You must add at least one section before creating quizzes!";
                return RedirectToAction(nameof(CourseContent), new { courseId = courseid });
            }


            var course =  await unite.Course.GetCourseWithQuizzes(courseid);

            return View(course);
        }

        public async Task<IActionResult> AddQuiz(CreateQuiz_vm model)
        {
            if (ModelState.IsValid)
            {
                var Quiz = new Quiz
                {
                    Title = model.Title,
                    SectionId = model.SectionId,
                    PassingScore = model.PassingScore
                };
                TempData["SuccessMessage"] = "Quiz Added Successfully.";
                await unite.Quiz.Create(Quiz);
            }
            return RedirectToAction(nameof(CreateQuiz), new { courseid = model.CourseId });

        }
        [HttpPost]
        public async Task<IActionResult> EditQuiz(int quizid,string title,int courseid,int passingscore)
        {

            var quiz = await unite.Quiz.FindById(quizid);
            quiz.Title = title;
            quiz.PassingScore = passingscore;

            await unite.Quiz.Update(quiz.Id);
            TempData["SuccessMessage"] = "Quiz Edited Successfully.";
            return RedirectToAction(nameof(CreateQuiz), new { courseid = courseid });

        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuiz(int quizId, int courseId)
        {
            // 1. هات الكويز وهات معاه الأسئلة والاختيارات (مهم جداً Include)
            var quiz = await unite.Quiz.GetQuizWithInfo(quizId);

            if (quiz != null)
            {
                // 2. حذف الاختيارات والأسئلة يدوياً
                if (quiz.Questions != null && quiz.Questions.Any())
                {
                    foreach (var question in quiz.Questions)
                    {
                        // أ. حذف اختيارات السؤال الحالي
                        if (question.Options != null && question.Options.Any())
                        {
                            await unite.AnswerOption.DeleteAll(question.Options);
                        }
                    }

                    // ب. بعد تنظيف الاختيارات، نحذف الأسئلة نفسها
                    await unite.Question.DeleteAll(quiz.Questions);
                }

                // 3. أخيراً.. نحذف الكويز (الرأس الكبيرة)
                await unite.Quiz.Delete(quiz.Id);

                TempData["SuccessMessage"] = "Quiz and all its questions deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Quiz not found!";
            }

            // العودة لنفس الصفحة
            return RedirectToAction(nameof(CreateQuiz), new { courseId = courseId });
        }

        public async Task<IActionResult> AddQuestion(CreateQuestion_vm model)
        {
            if (ModelState.IsValid)
            {
                var question = new Question
                {
                    Text = model.Text,
                    Score = model.Score,
                    QuizId=model.QuizId
                };

                await unite.Question.Create(question);

                // 2. حفظ الاختيارات
                if (model.Options != null && model.Options.Count > 1)
                {
                    var choicesToAdd = new List<AnswerOption>();
                    for (int i = 0; i < model.Options.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(model.Options[i])) continue;

                        choicesToAdd.Add(new AnswerOption
                        {
                            Option = model.Options[i], // تأكد من اسم الخاصية في جدولك (OptionText أو Option)
                            QuestionId = question.Id, // ربط بالسؤال الجديد
                            IsCorrect = (i == model.CorrectOption)
                        });
                    }

                    await unite.AnswerOption.CreateRange(choicesToAdd);
                }

                TempData["SuccessMessage"] = "Question & choices added successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add question.";
            }
            return RedirectToAction(nameof(CreateQuiz), new { courseid = model.CourseId });

        }

        [HttpPost]
        public async Task<IActionResult> EditQuestion(EditQuestion_vmcs model)
        {
            var question = await unite.Question.GettAll(null ,q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == model.QuestionId);

            if (question != null)
            {
                question.Text = model.Text;
                question.Score = model.Score;

                if (model.Options != null && model.Options.Count > 1)
                {

                    await unite.AnswerOption.DeleteAll(question.Options);


                    var newOptions = new List<AnswerOption>();
                    for (int i = 0; i < model.Options.Count; i++)
                    {
                        if (string.IsNullOrWhiteSpace(model.Options[i])) continue;

                        newOptions.Add(new AnswerOption
                        {
                            Option = model.Options[i], 
                            QuestionId = question.Id,
                            IsCorrect = (i == model.CorrectIndex)
                        });
                    }
                    await unite.AnswerOption.CreateRange(newOptions);
                }
                TempData["SuccessMessage"] = "Question updated successfully.";
            }

            return RedirectToAction(nameof(CreateQuiz), new { courseId = model.CourseId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int questionid,int courseid)
        {
            var question = await unite.Question.GettAll(null,q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == questionid);

            if (question != null)
            {
                if (question.Options != null && question.Options.Any())
                {
                    await unite.AnswerOption.DeleteAll(question.Options);
                }

                await unite.Question.Delete(question.Id);


                TempData["SuccessMessage"] = "Question deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Question not found!";
            }
            return RedirectToAction(nameof(CreateQuiz), new { courseId = courseid });
        }

        public async Task<IActionResult> PublishCourse(int courseid)
        {
            var course = await unite.Course.GetAllCourseInfo(courseid);
            return View(course);
        }
        [HttpPost]
        public IActionResult PublishCourse()
        {
            return RedirectToAction("Index","Home");
        }

    }
}
