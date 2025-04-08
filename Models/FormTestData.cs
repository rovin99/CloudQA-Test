namespace CloudQATestsMacLib.Models
{
    /// <summary>
    /// Data model for form test data
    /// </summary>
    public class FormTestData
    {
        /// <summary>
        /// First name to be entered in the form
        /// </summary>
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// Last name to be entered in the form
        /// </summary>
        public string LastName { get; set; } = string.Empty;
        
        /// <summary>
        /// Email to be entered in the form
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether to select Male gender (if false, selects Female)
        /// </summary>
        public bool IsMale { get; set; }
        
        /// <summary>
        /// Mobile number to be entered in the form
        /// </summary>
        public string Mobile { get; set; } = string.Empty;
        
        /// <summary>
        /// Date of birth to be entered in the form
        /// </summary>
        public string DateOfBirth { get; set; } = string.Empty;
        
        /// <summary>
        /// Constructor with required fields
        /// </summary>
        public FormTestData(string firstName, string mobile, bool isMale)
        {
            FirstName = firstName;
            Mobile = mobile;
            IsMale = isMale;
        }
        
        /// <summary>
        /// Full constructor with all fields
        /// </summary>
        public FormTestData(string firstName, string lastName, string email, bool isMale, string mobile, string dateOfBirth)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            IsMale = isMale;
            Mobile = mobile;
            DateOfBirth = dateOfBirth;
        }
    }
} 