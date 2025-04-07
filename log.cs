using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace api
{
    public class log
    {
        private const string LOG_URL = "APIのURL";   //ログの送信先URL
        private const string LOG_VERSION = "0";                             // ログのバージョン(今後のメタデータの拡充を見据えて)
        private const string APP_VERSION = "0.0";                           // アプリケーションのバージョン

        /// <summary>
        /// ログレベルの設定
        /// </summary>
        private enum LOGLEVEL
        {
            INFO,
            DEBUG,
            WARNING,
            ERROR
        }


        /// <summary>
        /// ログをAPIに送信する.
        /// </summary>
        /// <param name="logLevel">ログレベル</param>
        /// <param name="message">メッセージ</param>
        /// <returns>成否, レスポンス(Json)</returns>
        private static async UniTask<(bool, JsonNode)> Post(LOGLEVEL logLevel,string message)
        {
            bool isSuccess = false;
            JsonNode json = null;

            WWWForm form = new WWWForm();

            switch (logLevel)
            {
                case LOGLEVEL.INFO:
                    form.AddField("logLevel", "INFO");
                    break;

                case LOGLEVEL.DEBUG:
                    form.AddField("logLevel", "DEBUG");
                    break;

                case LOGLEVEL.WARNING:
                    form.AddField("logLevel", "WARNING");
                    break;

                case LOGLEVEL.ERROR:
                    form.AddField("logLevel", "ERROR");
                    break;

                default:
                    return (false, null);
            }

            form.AddField("message", message);
            form.AddField("metadata", GenerateMetaData());
            form.AddField("timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

            using (UnityWebRequest request = UnityWebRequest.Post(LOG_URL, form))
            {
                request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
                request.useHttpContinue = false;

                await request.SendWebRequest().ToUniTask();  // ← ここで UniTask 化！

                if (request.responseCode == 201)
                {
                    try
                    {
                        json = JsonNode.Parse(request.downloadHandler.text);
                        isSuccess = true;
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError("JSON Parse Error: " + e.Message);
                        json = null;
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("Login Failed: " + request.responseCode);
                }
            }

            return (isSuccess, json);
        }


        /// <summary>
        /// 端末のIPv4アドレスを返してくれるやーつ.
        /// </summary>
        /// <returns>IPv4アドレス</returns>
        private static string GetLocalIPAddress()
        {
            string localIP = "";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                // IPv4だけに限定（IPv6除外）
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        /// <summary>
        /// ログ用のメタデータ(json)を作ってくれる関数.
        /// </summary>
        /// <returns>メタデータ</returns>
        private static string GenerateMetaData()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string ip = GetLocalIPAddress();
            string platform = Application.platform.ToString();
            string osVersion = SystemInfo.operatingSystem;
            string deviceModel = SystemInfo.deviceModel;
            string screenResolution = Screen.width + "x" + Screen.height;
            string language = Application.systemLanguage.ToString();

            string json = $"{{\n" +
                $" \"LogVersion\" : \"{LOG_VERSION}\",\n" +           // ログのバージョン(今後のメタデータの拡充を見据えて)
                $" \"DeviceId\" : \"{deviceId}\",\n" +                // 一意のデバイスID(環境によってはアプデで値が変わるらしい)
                $" \"AppVersion\" : \"{APP_VERSION}\",\n" +           // アプリケーションのバージョン
                $" \"ip\" : \"{ip}\",\n" +                            // IPアドレス
                $" \"Platform\" : \"{platform}\",\n" +                // プラットフォーム
                $" \"OsVersion\" : \"{osVersion}\",\n" +              // OSのバージョン
                $" \"DeviceModel\" : \"{deviceModel}\",\n" +          // デバイスのモデル
                $" \"ScreenResolution\" : \"{screenResolution}\",\n" +// 画面解像度
                $" \"Language\" : \"{language}\",\n" +                // 言語
                $"}}";

            return json;
        }

        /// <summary>
        /// Infoのログを送信する.
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <returns>成否, レスポンス(Json)</returns>
        public static async UniTask<(bool, JsonNode)> Info(string message)
        {
            UnityEngine.Debug.Log(message);
            return await Post(LOGLEVEL.INFO,message);
        }


        /// <summary>
        /// Debugのログを送信する.
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <returns>成否, レスポンス(Json)</returns>
        public static async UniTask<(bool, JsonNode)> Debug(string message)
        {
            UnityEngine.Debug.Log(message);
            return await Post(LOGLEVEL.DEBUG, message);
        }


        /// <summary>
        /// Warningのログを送信する.
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <returns>成否, レスポンス(Json)</returns>
        public static async UniTask<(bool, JsonNode)> Warning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
            return await Post(LOGLEVEL.WARNING, message);
        }


        /// <summary>
        /// Errorのログを送信する.
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <returns>成否, レスポンス(Json)</returns>
        public static async UniTask<(bool, JsonNode)> Error(string message)
        {
            UnityEngine.Debug.LogError(message);
            return await Post(LOGLEVEL.ERROR, message);
        }
    }
}