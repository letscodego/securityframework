using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Security.Identity
{
    public class RoleViewModel
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "RoleName")]
        public string Name { get; set; }
    }
    
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            this.RolesList = new List<SelectListItem>();
            this.GroupsList = new List<SelectListItem>();
            this.ClaimsList = new List<SelectListItem>();
        }
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Is Active Directory User?")]
        public bool IsActiveDirectoryUser { get; set; }
        public IEnumerable<SelectListItem> RolesList { get; set; }
        public ICollection<SelectListItem> GroupsList { get; set; }
        public ICollection<SelectListItem> ClaimsList { get; set; }
    }

    public class GroupViewModel
    {
        public GroupViewModel()
        {
            this.UsersList = new List<SelectListItem>();
            this.RolesList = new List<SelectListItem>();
            this.ClaimsList = new List<SelectListItem>();
        }
        [Required(AllowEmptyStrings = false)]
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<SelectListItem> UsersList { get; set; }
        public ICollection<SelectListItem> RolesList { get; set; }
        public ICollection<SelectListItem> ClaimsList { get; set; }
    }

    public class ClaimViewModel
    {
        public ClaimViewModel()
        {
        }
        [Required(AllowEmptyStrings = false)]
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Key { get; set; }
        public string Value { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExpireDate { get; set; }
    }

    public class PrincipalRowFilterViewModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string PrincipalType { get; set; }

        [DisplayName("PrincipalId")]
        [Required]
        public int SelectedPrincipalId { get; set; }
        public string PrincipalName { get; set; }
        public IEnumerable<SelectListItem> PrincipalList { get; set; }

        [DisplayName("RowFilterType")]
        [Required]
        public int SelectedRowFilterTypeId { get; set; }
        public string RowFilterTypeName { get; set; }
        public IEnumerable<SelectListItem> RowFilterTypeList { get; set; }

        [Required(AllowEmptyStrings = false)]
        public int RowFilterValue { get; set; }
    }
}