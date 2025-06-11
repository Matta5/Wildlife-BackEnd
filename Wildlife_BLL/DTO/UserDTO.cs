namespace Wildlife_BLL.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        
        // Statistics properties
        public int TotalObservations { get; set; }
        public int UniqueSpeciesObserved { get; set; }
    }

}
