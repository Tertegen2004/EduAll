using EduAll.Domain;
using EduAll.Repository;
using EduAll.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Controllers
{
    public class QuizController : Controller
    {
        private readonly IUniteOfWork unite;
        private readonly UserManager<AppUser> usermanager;

        public QuizController(IUniteOfWork unite,UserManager<AppUser>usermanager)
        {
            this.unite = unite;
            this.usermanager = usermanager;
        }

        [HttpGet]
        public async Task<IActionResult> LoadQuiz(int quizId, int courseId)
        {
            var quiz = await unite.Quiz.GettAll()
                .Include(q => q.Questions).ThenInclude(qu => qu.Options)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return Content("<p class='text-danger'>Quiz not found!</p>");

            // تحويل للدومين للموديل
            var model = new QuizAttemp_vm
            {
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,
                CourseId = courseId,
                Questions = quiz.Questions.Select(q => new Question_vm
                {
                    QuestionId = q.Id,
                    Text = q.Text,
                    Score = q.Score,
                    Options = q.Options.Select(o => new Option_vm
                    {
                        OptionId = o.Id,
                        Text = o.Option
                    }).ToList()
                }).ToList()
            };

            // هنرجع Partial View عشان يتعرض جوه المودال
            return PartialView("_QuizModalContent", model);
        }

        // 2. أكشن لاستلام الإجابات وتصحيحها (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(QuizSubmission_vm model)
        {
            var userId = usermanager.GetUserId(User);

            // 1. جلب الكويز الأصلي بالإجابات الصحيحة للمقارنة
            var quizOriginal = await unite.Quiz.GettAll()
                .Include(q => q.Questions).ThenInclude(qu => qu.Options)
                .FirstOrDefaultAsync(q => q.Id == model.QuizId);

            if (quizOriginal == null) return Json(new { success = false, message = "Quiz not found" });

            // 2. التصحيح
            int totalScore = 0;
            int studentScore = 0;
            var studentAnswersList = new List<StudentAnswer>();

            // إنشاء سجل المحاولة (Submission)
            var submission = new QuizSubmission
            {
                QuizId = model.QuizId,
                StudentId = userId,
                SubmittedAt = DateTime.Now
            };

            foreach (var question in quizOriginal.Questions)
            {
                totalScore += question.Score;

                // هل الطالب جاوب السؤال ده؟
                if (model.Answers.ContainsKey(question.Id))
                {
                    int selectedOptionId = model.Answers[question.Id];

                    // حفظ إجابة الطالب
                    studentAnswersList.Add(new StudentAnswer
                    {
                        QuestionId = question.Id,
                        SelectedOptionId = selectedOptionId,
                        SubmissionId = submission.Id // سيتم ربطها بعد الحفظ
                    });

                    // التحقق من صحة الإجابة
                    var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                    if (correctOption != null && correctOption.Id == selectedOptionId)
                    {
                        studentScore += question.Score;
                    }
                }
            }

 
            submission.Score = studentScore;
            submission.IsPassed = studentScore >= quizOriginal.PassingScore;
            submission.StudentAnswers = studentAnswersList;

            // 4. الحفظ في الداتا بيز
            await unite.QuizSubmission.Create(submission);

            // 5. إرجاع النتيجة
            return Json(new
            {
                success = true,
                score = studentScore,
                total = totalScore,
                passed = submission.IsPassed,
                message = submission.IsPassed ? "Congratulations! You passed." : "You failed. Try again."
            });
        }
    }
}
