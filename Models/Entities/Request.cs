namespace ArqanumServer.Models.Entities
{
    public class Request
    {
        public Guid Id { get; set; } 

        public RequestMethod Method { get; set; }

        public string RecipientId { get; set; }

        public byte[] Payload { get; set; }
    }

    public enum RequestMethod
    {
        Contact = 0,
        Message = 1,
    }
}
