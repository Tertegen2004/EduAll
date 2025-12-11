using EduAll.Areas.Admin.ViewModels;

namespace EduAll.ViewModel
{
    public class AccountSettings_vm
    {
        public ProfileSettings_vm? Profile { get; set; }
        public ChangePassword_vm Password { get; set; }
    }
}
