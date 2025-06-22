using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using WaslAlkhair.Api.DTOs.Payment;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Data;
using Google;
using System.Security.Claims;

namespace WaslAlkhair.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentsController : ControllerBase
	{
		private readonly AppDbContext _context;

		public PaymentsController(AppDbContext context)
		{
			_context = context;
		}
		[HttpPost("create-session")]
		public async Task<ActionResult<CreateStripeSessionResponseDto>> CreateStripeSession([FromBody] CreateStripeSessionRequestDto request)
		{
			// check if user is quest or authenticated
			string? userId = User.Identity != null && User.Identity.IsAuthenticated
				? User.FindFirstValue(ClaimTypes.NameIdentifier)
				: null;

			var options = new SessionCreateOptions
			{
				PaymentMethodTypes = new List<string> { "card" },
				LineItems = new List<SessionLineItemOptions>
		{
			new SessionLineItemOptions
			{
				PriceData = new SessionLineItemPriceDataOptions
				{
					UnitAmountDecimal = request.Amount * 100,
					Currency = "usd",
					ProductData = new SessionLineItemPriceDataProductDataOptions
					{
						Name = $"Donation - {request.Type}"
					}
				},
				Quantity = 1
			}
		},
				Mode = "payment",
				SuccessUrl = "https://example.com/success?session_id={CHECKOUT_SESSION_ID}",
				CancelUrl = "https://example.com/cancel",
			};

			var service = new SessionService();
			var session = await service.CreateAsync(options);

			var donation = new Donation
			{
				Amount = request.Amount,
				DonorId = userId, 
				Type = request.Type,
				CategoryId = request.CategoryId != 0 ? request.CategoryId : null,
				StripeSessionId = session.Id,
				DonatedAt = DateTime.UtcNow
			};

			_context.Donations.Add(donation);
			await _context.SaveChangesAsync();

			return Ok(new CreateStripeSessionResponseDto
			{
				StripeSessionId = session.Id,
				StripeCheckoutUrl = session.Url
			});
		}


	}
}
