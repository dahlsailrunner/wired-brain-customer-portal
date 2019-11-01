namespace WiredBrain.CustomerPortal.Web.Models
{
    public class ErrorModel
    {
        public string RequestId { get; set; }
        public string ApiRoute { get; set; }
        public string ApiStatus { get; set; }
        public string ApiErrorId { get; set; }
        public string ApiTitle { get; set; }
        public string ApiDetail { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    }
}
