namespace KAU.FireballSDK.Models
{
    public class ErrorResponse : BaseResponse
    {
        public const string TIMEOUT_REASON = "Message aborted due time out {0} sec";
        public const string NAME_ERROR = "error";
        public const string NAME_TIMEOUT = "timeout";

        public int Code { get; set; }
        public string Reason { get; set; }

        public bool IsCustomError => !string.IsNullOrEmpty(Name) && Name.Equals(NAME_ERROR);
        public bool IsTimeout => !string.IsNullOrEmpty(Name) && Name.Equals(NAME_TIMEOUT);

        public static ErrorResponse CustomError(string actionId, string reason, int code = 0)
        {
            return new ErrorResponse()
            {
                ActionId = actionId,
                Name = NAME_ERROR,
                Reason = reason,
                Code = code,
            };
        }

        public static ErrorResponse TimeoutResponse(string actionId, float timeout, int code = 0)
        {
            return new ErrorResponse()
            {
                ActionId = actionId,
                Name = NAME_TIMEOUT,
                Reason = string.Format(TIMEOUT_REASON, timeout),
                Code = code,
            };
        }
    }
}
