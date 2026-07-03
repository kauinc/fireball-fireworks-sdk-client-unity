using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Fireball.Game.Client.Models;
using Fireball.Game.Client.Tools;
using UnityEngine;
using UnityEngine.Networking;

namespace Fireball.Game.Client.Modules
{
    public class Communicator
    {
        private IFireballLogger _logger;
        private MonoBehaviour _mono;

        public Communicator(MonoBehaviour mono, IFireballLogger logger)
        {
            _mono = mono;
            _logger = logger;
        }

        public void SendGET(string url, Dictionary<string, string> data = null, Action<string> onSuccess = null, Action<string> onError = null) =>
            _mono.StartCoroutine(SendGETCoroutine(url, data, onSuccess, onError));

        private IEnumerator SendGETCoroutine(string url, Dictionary<string, string> data, Action<string> onSuccess, Action<string> onError)
        {
            long responceCode = 0;
            string responceText = string.Empty;
            url = FireballTools.FormatUrlAndParams(url, data);

            _logger.Info($"Sending GET Request to URL = {url}");
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();
            using (UnityWebRequest client = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET, downloadHandler, null))
            {
                client.SetRequestHeader("Content-Type", "application/json");
                yield return client.SendWebRequest();

                if (client.result == UnityWebRequest.Result.ConnectionError ||
                    client.result == UnityWebRequest.Result.ProtocolError)
                {
                    responceText = client.error;
                    responceCode = client.responseCode;
                    _logger.Error($"GET Request Error: {responceText} ({responceCode})");
                    onError?.Invoke(responceText);
                }
                else
                {
                    responceText = client.downloadHandler?.text;
                    responceCode = client.responseCode;
                    _logger.Info($"GET Response: {responceText} ({responceCode})");
                    onSuccess?.Invoke(responceText);
                }
            }
        }

        public void SendPOST(string url, BaseMessage request, Action<string> onSuccess = null, Action<string> onError = null) =>
            _mono.StartCoroutine(SendPOSTCoroutine(url, request, onSuccess, onError));

        private IEnumerator SendPOSTCoroutine(string url, BaseMessage request, Action<string> onSuccess, Action<string> onError)
        {
            long responceCode = 0;
            string responceText = string.Empty;
            byte[] bytes = Encoding.UTF8.GetBytes(request.ToJson());

            _logger.Info($"Message - {request.Name} - Sending... (ActionId: {request.ActionId}, GameSession: {request?.GameSession})" +
                $"\nMesaage: {request.ToJson()}");

            UploadHandler uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(request.ToJson()));
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();

            using (UnityWebRequest client = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST, downloadHandler, uploadHandler))
            {
                client.SetRequestHeader("Content-Type", "application/json");
                client.SetRequestHeader("Accept", "application/json");
                client.uploadHandler.contentType = "application/json";

                yield return client.SendWebRequest();

                if (client.result == UnityWebRequest.Result.ConnectionError ||
                    client.result == UnityWebRequest.Result.ProtocolError)
                {
                    responceText = client.error;
                    responceCode = client.responseCode;
                    _logger.Error($"Message - {request.Name} - Error: {responceText} (ActionId: {request?.ActionId}, GameSession: {request?.GameSession})");
                    onError?.Invoke(responceText);
                }
                else
                {
                    responceText = client.downloadHandler.text;
                    responceCode = client.responseCode;
                    _logger.Info($"Message - {request.Name} - Sent (ActionId: {request?.ActionId}, GameSession: {request?.GameSession})");
                    onSuccess?.Invoke(responceText);
                }
            }
        }
    }
}
