using System.ComponentModel.DataAnnotations;

namespace Momentum.Areas.Manage.ViewModels.AccountVMs
{
    public class ProfileVM
    {
        [StringLength(255)]
        public string? Name { get; set; }
        [StringLength(255)]
        public string? SurName { get; set; }
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string? ConfirmPassword { get; set; }

    }
}
