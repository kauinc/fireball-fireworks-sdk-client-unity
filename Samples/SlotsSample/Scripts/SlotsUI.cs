using UnityEngine.UI;
using UnityEngine;

namespace SlotsSample
{
    public class SlotsUI : MonoBehaviour
    {
        [SerializeField] private Text _labelInit;
        [SerializeField] private Text _labelAuth;
        [SerializeField] private Text _labelSpin;
        [SerializeField] private Text _labelBalance;
        [SerializeField] private Text _labelBet;
        [SerializeField] private Text _labelWin;
        [SerializeField] private Button _buttonInit;
        [SerializeField] private Button _buttonAuth;
        [SerializeField] private Button _buttonSpin;
        [SerializeField] private Button _buttonReset;

        [Header("Success Panel")]
        [SerializeField] private Text _labelSuccess;
        [SerializeField] private RectTransform _panelSuccess;

        [Header("Error Panel")]
        [SerializeField] private Text _labelError;
        [SerializeField] private RectTransform _panelError;

        public void Start()
        {
            SetInitButton();
            SetAuthButton(enable: false);
            SetSpinButton(enable: false);
        }

        public void Initializing()
        {
            SetInitButton("Initializing...", false);
            SetAuthButton(enable: false);
            SetSpinButton(enable: false);
        }

        public void Initialized(bool success)
        {
            if (success)
            {
                SetInitButton("Initialized", false);
                SetAuthButton(enable: true);
                SetSpinButton(enable: false);
            }
            else
            {
                SetInitButton("Init", true);
                SetAuthButton(enable: false);
                SetSpinButton(enable: false);
            }
        }

        public void Authorizing()
        {
            SetAuthButton("Autorizing...", false);
            SetSpinButton(enable: false);
        }

        public void Authorized(bool success)
        {
            if (success)
            {
                SetAuthButton("Autorized", false);
                SetSpinButton(enable: true);
            }
            else
            {
                SetAuthButton("Auth", true);
                SetSpinButton(enable: false);
            }
        }

        public void Spinning(bool isSpinning)
        {
            if (isSpinning)
            {
                SetSpinButton("Spinning...", false);
            }
            else
            {
                SetSpinButton("Spin", true);
            }
        }

        public void UpdateBalance(string currency, long balance)
        {
            _labelBalance.text = $"Balance: {currency} {(balance * 0.01f):N2}";
        }

        public void UpdateBet(string currency, long bet)
        {
            _labelBet.text = $"Bet: {currency} {(bet * 0.01f):N2}";
        }

        public void UpdateWin(string currency, long win)
        {
            _labelWin.text = $"Win: {currency} {(win * 0.01f):N2}";
        }


        public void ShowSuccess(string message)
        {
            _labelSuccess.text = message;
            _panelSuccess.gameObject.SetActive(true);
            CancelInvoke(nameof(HideSuccess));
            Invoke(nameof(HideSuccess), 5.0f);
        }

        public void HideSuccess()
        {
            _panelSuccess.gameObject.SetActive(false);
        }

        public void ShowError(string message)
        {
            _labelError.text = message;
            _panelError.gameObject.SetActive(true);
            CancelInvoke(nameof(HideError));
            Invoke(nameof(HideError), 5.0f);
        }

        public void HideError()
        {
            _panelError.gameObject.SetActive(false);
        }


        private void SetInitButton(string label = "Init", bool enable = true)
        {
            _labelInit.text = label;
            _buttonInit.interactable = enable;
        }

        private void SetAuthButton(string label = "Auth", bool enable = true)
        {
            _labelAuth.text = label;
            _buttonAuth.interactable = enable;
        }

        private void SetSpinButton(string label = "Spin", bool enable = true)
        {
            _labelSpin.text = label;
            _buttonSpin.interactable = enable;
            _buttonReset.interactable = enable;
        }

    }
}
