using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Enums
{
    public enum BTRoles
    {
        Admin,
        [Display(Name = "Project Manager")]
        ProjectManager,
        Developer,
        Submitter,
        DemoUser
    }
}
