namespace chitecapi.Responses
{
    public class JsonDataResponse
    {
        private readonly string data;
        private readonly string prefixData;

        public JsonDataResponse(string data)
        {
            this.data = data;
        }

        public JsonDataResponse(string prefixData, string data)
        {
            this.data = data;
            this.prefixData = prefixData.TrimStart('{').TrimEnd('}');
        }

        public string JsonString
        {
            get
            {
                return string.IsNullOrEmpty(prefixData)
                    ? "{" + $"\"data\":{data},\"error\":0,\"error_type\":0,\"error_message\":0" + "}"
                    : "{" + $"{prefixData},\"data\":{data},\"error\":0,\"error_type\":0,\"error_message\":0" + "}";
            }
        }
    }
}