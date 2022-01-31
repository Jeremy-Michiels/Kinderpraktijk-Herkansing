using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace src.Areas.Profile.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; }
        public string ParentId { get; set; }

        [Display(Name = "Voornaam")]
        public string FirstName { get; set; }

        [Display(Name = "Achternaam")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Emailadres")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Telefoonnummer")]
        public string PhoneNumber { get; set; }
        public string Specialism { get; set; }
        public string Description { get; set; }
    }
}
