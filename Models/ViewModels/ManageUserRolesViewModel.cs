using Microsoft.AspNetCore.Mvc.Rendering;

namespace CrucibleBugTracker.Models.ViewModels
{
    public class ManageUserRolesViewModel
    {
        public SelectList? Roles { get; set; }
        
        public BTUser? User { get; set; }

        public string? SelectedRole { get; set; }
    }
}
