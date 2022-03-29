using UnityEngine;

namespace KAU.FireballSDK.Models
{
    [System.Serializable]
    public class ErrorResponse : BaseResponse
    {
        public const string TYPE_ERROR = "error";
        public const string TYPE_TIMEOUT = "timeout";

        public const string MESSAGE_TIMEOUT = "Message aborted due time out {0} sec";

        public string reason;
        public string type;
        public ErrorMessage errorMessage = new ErrorMessage();

        public bool HasMessage => !string.IsNullOrEmpty(reason) || (errorMessage != null && errorMessage.HasMessage);
        public string Message => HasMessage ? (errorMessage != null && errorMessage.HasMessage ? errorMessage.Message : reason) : string.Empty;

        public bool IsCustomError => !string.IsNullOrEmpty(Name) && Name.Equals(TYPE_ERROR);
        public bool IsTimeout => !string.IsNullOrEmpty(Name) && Name.Equals(TYPE_TIMEOUT) || !string.IsNullOrEmpty(type) && type.Equals(TYPE_TIMEOUT);

        public static string MakeReason(string message)
        {
            return message;
        }

        public static ErrorResponse ParseError(string json)
        {
            ErrorResponse error = JsonUtility.FromJson<ErrorResponse>(json);
            error.errorMessage = JsonUtility.FromJson<ErrorMessage>(json);
            return error;
        }
    }
}
