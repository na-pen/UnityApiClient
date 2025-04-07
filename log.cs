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
        private const string LOG_URL = "API��URL";   //���O�̑��M��URL
        private const string LOG_VERSION = "0";                             // ���O�̃o�[�W����(����̃��^�f�[�^�̊g�[����������)
        private const string APP_VERSION = "0.0";                           // �A�v���P�[�V�����̃o�[�W����

        /// <summary>
        /// ���O���x���̐ݒ�
        /// </summary>
        private enum LOGLEVEL
        {
            INFO,
            DEBUG,
            WARNING,
            ERROR
        }


        /// <summary>
        /// ���O��API�ɑ��M����.
        /// </summary>
        /// <param name="logLevel">���O���x��</param>
        /// <param name="message">���b�Z�[�W</param>
        /// <returns>����, ���X�|���X(Json)</returns>
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

                await request.SendWebRequest().ToUniTask();  // �� ������ UniTask ���I

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
        /// �[����IPv4�A�h���X��Ԃ��Ă�����[��.
        /// </summary>
        /// <returns>IPv4�A�h���X</returns>
        private static string GetLocalIPAddress()
        {
            string localIP = "";
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                // IPv4�����Ɍ���iIPv6���O�j
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        /// <summary>
        /// ���O�p�̃��^�f�[�^(json)������Ă����֐�.
        /// </summary>
        /// <returns>���^�f�[�^</returns>
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
                $" \"LogVersion\" : \"{LOG_VERSION}\",\n" +           // ���O�̃o�[�W����(����̃��^�f�[�^�̊g�[����������)
                $" \"DeviceId\" : \"{deviceId}\",\n" +                // ��ӂ̃f�o�C�XID(���ɂ���Ă̓A�v�f�Œl���ς��炵��)
                $" \"AppVersion\" : \"{APP_VERSION}\",\n" +           // �A�v���P�[�V�����̃o�[�W����
                $" \"ip\" : \"{ip}\",\n" +                            // IP�A�h���X
                $" \"Platform\" : \"{platform}\",\n" +                // �v���b�g�t�H�[��
                $" \"OsVersion\" : \"{osVersion}\",\n" +              // OS�̃o�[�W����
                $" \"DeviceModel\" : \"{deviceModel}\",\n" +          // �f�o�C�X�̃��f��
                $" \"ScreenResolution\" : \"{screenResolution}\",\n" +// ��ʉ𑜓x
                $" \"Language\" : \"{language}\",\n" +                // ����
                $"}}";

            return json;
        }

        /// <summary>
        /// Info�̃��O�𑗐M����.
        /// </summary>
        /// <param name="message">���b�Z�[�W</param>
        /// <returns>����, ���X�|���X(Json)</returns>
        public static async UniTask<(bool, JsonNode)> Info(string message)
        {
            UnityEngine.Debug.Log(message);
            return await Post(LOGLEVEL.INFO,message);
        }


        /// <summary>
        /// Debug�̃��O�𑗐M����.
        /// </summary>
        /// <param name="message">���b�Z�[�W</param>
        /// <returns>����, ���X�|���X(Json)</returns>
        public static async UniTask<(bool, JsonNode)> Debug(string message)
        {
            UnityEngine.Debug.Log(message);
            return await Post(LOGLEVEL.DEBUG, message);
        }


        /// <summary>
        /// Warning�̃��O�𑗐M����.
        /// </summary>
        /// <param name="message">���b�Z�[�W</param>
        /// <returns>����, ���X�|���X(Json)</returns>
        public static async UniTask<(bool, JsonNode)> Warning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
            return await Post(LOGLEVEL.WARNING, message);
        }


        /// <summary>
        /// Error�̃��O�𑗐M����.
        /// </summary>
        /// <param name="message">���b�Z�[�W</param>
        /// <returns>����, ���X�|���X(Json)</returns>
        public static async UniTask<(bool, JsonNode)> Error(string message)
        {
            UnityEngine.Debug.LogError(message);
            return await Post(LOGLEVEL.ERROR, message);
        }
    }
}