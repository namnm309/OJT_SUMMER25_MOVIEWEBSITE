namespace UI.Models
{
    public class EditProfileModel
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string IdentityCard { get; set; }
        public string Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? Gender { get; set; }
    }
}