using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.DTOs.OpportunityParticipation
{
    public class CreateOpportunityParticipation
    {
        
        [Required]
        public int OpportunityId { get; set; }
        [Required]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required]
        public string Specialization { get; set; }

        [Required]
        [RegularExpression(@"^(?:\+20|0)?(10|11|12|15)\d{8}$",
            ErrorMessage = "Invalid Egyptian phone number format.")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [MaxLength(14), MinLength(14)]
        [RegularExpression(@"^[2-3][0-9]{2}(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])[0-9]{7}$",
                 ErrorMessage = "رقم قومي غير صالح. تأكد من أن الرقم مكون من 14 رقمًا بالشكل الصحيح.")]
        public string NationalId { get; set; }
    }
}
