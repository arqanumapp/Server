namespace ArqanumServer.Models.Entities
{
    public class Request
    {
        public Guid Id { get; set; } 

        public string RecipientId { get; set; }

        public byte[] Payload { get; set; }

        public byte[] PayloadSignature { get; set; }
    }
}
