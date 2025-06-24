namespace WaslAlkhair.Api.MLModels
{
    public class ModelInput
    {
        public float Label { get; set; }
        public uint UserIdEncoded { get; set; }
        public uint ItemIdEncoded { get; set; }
        public uint DonationIdEncoded { get; set; }

        public float Rating { get; set; }
    }
} 