using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace EduAll.Areas.Admin.ViewModels
{
    public class AccountSettings_vm
    {
        public ProfileSettings_vm? Profile { get; set; }
        public ChangePassword_vm Password { get; set; }
    }
}
