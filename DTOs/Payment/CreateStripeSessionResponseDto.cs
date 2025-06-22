namespace WaslAlkhair.Api.DTOs.Payment
{
	public class CreateStripeSessionResponseDto
	{
		public string StripeSessionId { get; set; }
		public string StripeCheckoutUrl { get; set; }
	}
}