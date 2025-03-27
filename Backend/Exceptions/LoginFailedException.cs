namespace ESGanalyzer.Backend.Exceptions {
    public class LoginFailedException : Exception {
        public LoginFailedException() : base("Login failed. Please check your input.") { }

        public LoginFailedException(string message) : base(message) { }

        public LoginFailedException(string message, Exception inner) : base(message, inner) { }
    }
}
