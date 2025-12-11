using EduAll.Areas.Admin.ViewModels;
using EduAll.Constant;
using EduAll.Domain;
using EduAll.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduAll.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class Admin_CategoryController : Controller
    {
        private readonly IUniteOfWork unite;
        private readonly IWebHostEnvironment webHostEnvironment;

        public Admin_CategoryController(IUniteOfWork unite,IWebHostEnvironment webHostEnvironment)
        {
            this.unite = unite;
            this.webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var categories = unite.Category.GettAll()
                .Include(c => c.Courses)
                .ThenInclude(c => c.Enrollments)
                .ToList();

            var model = categories.Select(c => new Category_vm
            {
                Id = c.Id,
                Name = c.Name,
                Img = c.Img,
                Description = c.Description,
                CoursesCount = c.Courses.Count,
                StudentCategory = c.Courses.Sum(c => c.Enrollments.Count())
            }).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategory_vm model)
        {
            if (ModelState.IsValid)
            {
                string iconPath = null;

                // 1. رفع الأيقونة/الصورة
                if (model.Img != null)
                {
                    string uploadDir = Path.Combine(webHostEnvironment.WebRootPath, "images/categories");

                    // التأكد من وجود الفولدر
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    // اسم فريد للملف
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Img.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Img.CopyToAsync(fileStream);
                    }

                    iconPath = "/images/categories/" + fileName;
                }

                // 2. الحفظ في الداتا بيز
                var category = new Category
                {
                    Name = model.Name,
                    Img = iconPath,
                    Description = model.Description

                };

                await unite.Category.Create(category);

                TempData["SuccessMessage"] = "Category added successfully!";
            }
            else
            {
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "Failed to add category: " + errors;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {

            var hasCourses = await unite.Course.GettAll(c => c.CategoryId == id).AnyAsync();

            if (hasCourses)
            {
                TempData["ErrorMessage"] = "Cannot delete this category because it contains active courses. Please delete or move the courses first.";
                return RedirectToAction(nameof(Index));
            }
            var category = await unite.Category.FindById(id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found!";
                return RedirectToAction(nameof(Index));
            }

            // 2. 🧹 تنظيف السيرفر: حذف ملف الصورة (Icon) لو موجود
            if (!string.IsNullOrEmpty(category.Img))
            {
                // تحويل المسار النسبي لمسار حقيقي
                var imagePath = Path.Combine(webHostEnvironment.WebRootPath, category.Img.TrimStart('/'));

                // الحذف الفعلي للملف
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await unite.Category.Delete(category.Id);

            TempData["SuccessMessage"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditCategory_vm model)
        {
            var category = await unite.Category.FindById(model.Id);

            if (category != null)
            {
                // 1. تحديث النصوص
                category.Name = model.Name;
                category.Description = model.Description;

                // 2. تحديث الصورة (فقط لو رفع صورة جديدة)
                if (model.Img != null)
                {
                    // أ. حذف القديمة
                    if (!string.IsNullOrEmpty(category.Img))
                    {
                        var oldPath = Path.Combine(webHostEnvironment.WebRootPath, category.Img.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    // ب. رفع الجديدة
                    string uploadDir = Path.Combine(webHostEnvironment.WebRootPath, "images/categories");
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Img.FileName);
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Img.CopyToAsync(fileStream);
                    }

                    category.Img = "/images/categories/" + fileName;
                }

                await unite.Category.Update(category.Id);

                TempData["SuccessMessage"] = "Category updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Category not found.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

