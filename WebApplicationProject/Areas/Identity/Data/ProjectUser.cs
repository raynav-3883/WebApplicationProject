using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebApplicationProject.Areas.Identity.Data;

public class ProjectUser : IdentityUser
{
    internal byte[] ProfilePictureData;

    [PersonalData]
    [Column(TypeName ="nvarchar(100)")]
    public string FirstName { get; set; }

    [PersonalData]
    [Column(TypeName = "nvarchar(100)")]
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; internal set; }
    public string MobileNumber { get; internal set; }
}

