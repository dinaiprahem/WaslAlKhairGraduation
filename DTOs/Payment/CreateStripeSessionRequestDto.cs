using WaslAlkhair.Api.Models;

namespace WaslAlkhair.Api.DTOs.Payment
{
	public class CreateStripeSessionRequestDto
	{
		public decimal Amount { get; set; }
		public DonationType Type { get; set; }
		public int? CategoryId { get; set; }
	}
}
