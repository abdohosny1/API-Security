namespace API_Security.helper
{
    public class AuthResult
    {
        public List<string> Errors { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
    }
}
