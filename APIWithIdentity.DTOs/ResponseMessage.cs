namespace APIWithIdentity.DTOs
{
    public class ResponseMessage<T> 
    {
        public string Message { get; set; }
        
        public T Response { get; set; }
    }
}