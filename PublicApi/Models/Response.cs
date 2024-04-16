namespace PublicApi.Models
{
    public class Response
    {
        public bool Success { get; set; }  
        public string Message { get; set; }
        public object? Data { get; set; }


        public Response(bool success, string message, object data) 
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}
