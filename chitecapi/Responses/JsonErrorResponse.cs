namespace chitecapi.Responses
{
    public class JsonErrorResponse
    {
        public JsonErrorResponse(int error, int errorType, string errorMessage)
        {
            Error = error;
            ErrorType = errorType;
            ErrorMessage = errorMessage;
        }

        public int Error { get; }
        public int ErrorType { get; }
        public string ErrorMessage { get; }

        public string JsonString
        {
            get
            {
                if (int.TryParse(ErrorMessage, out var errorMessageNumber))
                {
                    return
                    "{" + $"\"error\":{Error},\"error_type\":{ErrorType},\"error_message\":{errorMessageNumber}" + "}";
                }

                return
                    "{" + $"\"error\":{Error},\"error_type\":{ErrorType},\"error_message\":\"{ErrorMessage}\"" + "}";
            }
        }
    }
}