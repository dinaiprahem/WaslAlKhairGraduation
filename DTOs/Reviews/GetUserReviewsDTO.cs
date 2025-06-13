namespace WaslAlkhair.Api.DTOs.Reviews
{
    public class GetUserReviewsDTO
    {
        public string UserName { get; set; }
        public string? UserImageUrl { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
