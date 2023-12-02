using Microsoft.AspNetCore.Mvc.Rendering;
using Momentum.Models;
using System.ComponentModel.DataAnnotations;

namespace Momentum.Areas.Manage.ViewModels.UserVMs
{
    public class ChangeRoleVM
    {
        [Required]
        public AppUser User { get; set; }
        public SelectList? RolesSelectList { get; set; }
        public string? SelectedRole { get; set; }
    }
}
