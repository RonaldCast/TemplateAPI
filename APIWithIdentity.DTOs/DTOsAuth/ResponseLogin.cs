namespace APIWithIdentity.DTOs.DTOsAuth
{
    public class ResponseLogin
    {
       public string Token { get; set; }
       public  string RefreshToken { get; set; }
       public UserResponse User { get; set; }
    }
}