using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Reviews
{
    public class CreateUserReviewDto
    {
        public string TargetUserId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; }
    }
}
