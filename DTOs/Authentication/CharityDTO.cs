using System.ComponentModel.DataAnnotations;

namespace WaslAlkhair.Api.DTOs.Authentication
{
    public class CharityDTO
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string CharityName { get; set; }
        public string? CharityRegistrationNumber { get; set; }
        public string? CharityMission { get; set; }
        public DateOnly EstablishedAt { get; set; }
        public string Address {  get; set; }
        public string PhoneNumber { get; set; }
    }
}
