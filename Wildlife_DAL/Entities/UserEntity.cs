namespace Wildlife_DAL.Entities
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }

        public ICollection<ObservationEntity> Observations { get; set; } = new List<ObservationEntity>();
    }
}
