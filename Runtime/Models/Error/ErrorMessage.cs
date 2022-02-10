using System.Collections.Generic;

namespace KAU.FireballSDK.Models
{
    [System.Serializable]
    public class ErrorMessage
    {
        public List<ErrorReason> reason;
        public bool HasMessage => reason != null && reason.Count > 0;
        public string Message => HasMessage ? reason[0].message : null;
    }
}
