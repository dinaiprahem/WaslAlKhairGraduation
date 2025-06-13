using System.ComponentModel.DataAnnotations;
using WaslAlkhair.Api.Models;

public class UserReview
{
    public int Id { get; set; }

    public string ReviewerId { get; set; }  // الي بيكتب الريفيو
    public AppUser Reviewer { get; set; }

    public string TargetUserId { get; set; }  // الي بيتكتب عنه الريفيو
    public AppUser TargetUser { get; set; }

    public int Rating { get; set; } // مثلاً من 1 لـ 5

    [MaxLength(500)]
    public string Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
