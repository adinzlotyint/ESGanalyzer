namespace ESGanalyzer.Backend.Exceptions {
    public class RegistrationFailedException : Exception {
        public RegistrationFailedException() : base("Registration failed. Please check your input.") { }

        public RegistrationFailedException(string message) : base(message) { }

        public RegistrationFailedException(string message, Exception inner) : base(message, inner) { }
    }

}
