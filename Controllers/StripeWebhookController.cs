using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using WaslAlkhair.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace WaslAlkhair.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StripeWebhookController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly IConfiguration _configuration;

		public StripeWebhookController(AppDbContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
		}

		[HttpPost]
		public async Task<IActionResult> HandleStripeWebhook()
		{
			var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
			var endpointSecret = _configuration["Stripe:WebhookSecret"];

			try
			{
				var stripeEvent = EventUtility.ConstructEvent(
					json,
					Request.Headers["Stripe-Signature"],
					endpointSecret,
				    throwOnApiVersionMismatch: false

				);

				if (stripeEvent.Type == Events.CheckoutSessionCompleted)
				{
					var session = stripeEvent.Data.Object as Session;

					// اطبعي الـ Session Id علشان تتأكدي
					Console.WriteLine($"✅ Stripe Session ID from webhook: {session?.Id}");

					var donation = await _context.Donations
						.FirstOrDefaultAsync(d => d.StripeSessionId == session.Id);

					if (donation != null)
					{
						donation.IsPaid = true;
						donation.PaymentConfirmedAt = DateTime.UtcNow;
						await _context.SaveChangesAsync();

						Console.WriteLine("✅ Donation payment confirmed");
					}
					else
					{
						Console.WriteLine("❌ Donation not found for session id: " + session?.Id);
					}
				}

				return Ok();
			}
			catch (Exception ex)
			{
				Console.WriteLine("❌ Webhook error: " + ex.Message);
				return BadRequest($"⚠️ Webhook error: {ex.Message}");
			}
		}

	}
}
