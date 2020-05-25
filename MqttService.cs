using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Bossinfo.Caller.MqttLogParserTesting
{
    public class MqttService : IDisposable
    {
        private MqttClient mqttClient;

        public event EventHandler SubscribeReceived;

        public bool IsConnected;

        public Exception ConnectException;

        private string guid = Guid.NewGuid().ToString();

        private string ip;

        private List<string> topicList;

        private bool isReset = true;

        public static LogService log = new LogService();

        public bool isCreateMqtt = false;

        public MqttService(string ip)
        {
            //errorCode = Enums.Common.ErrorCode.MqttIP錯誤;
            //return;
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new Exception("請輸入MQTT IP位置");
            }

            this.ip = ip;
            mqttClient = new MqttClient(this.ip);

            try
            {
                byte code = mqttClient.Connect(guid);

                IsConnected = true;
                mqttClient.ConnectionClosed -= MqttClient_ConnectionClosed;
                mqttClient.ConnectionClosed += MqttClient_ConnectionClosed;
                log.Info($"MQTT連線...");
            }
            catch (Exception ex)
            {
                IsConnected = false;
                ConnectException = ex;
                log.Info($"MQTT連線失敗...");

                throw new Exception("MQTT連線失敗...");
            }

        }

        /// <summary>
        /// 關閉連線
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MqttClient_ConnectionClosed(object sender, EventArgs e)
        {
            log.Info($"MQTT已中斷...");

            if (isReset)
            {
                log.Info($"MQTT重新連線...");
                CreateMqtt();
            }
            else
            {
                log.Info($"MQTT關閉重新連線...");
            }
        }

        /// <summary>
        /// 建立物件
        /// </summary>
        private void CreateMqtt()
        {
            if (isCreateMqtt)
            {
                log.Info($"MQTT重新連線...已執行，跳過不執行");
                return;
            }

            isCreateMqtt = true;

            var connected = mqttClient.IsConnected;

            while (mqttClient != null && !mqttClient.IsConnected)
            {
                try
                {
                    byte code = mqttClient.Connect(Guid.NewGuid().ToString());
                    IsConnected = true;

                    mqttClient.ConnectionClosed -= MqttClient_ConnectionClosed;
                    mqttClient.ConnectionClosed += MqttClient_ConnectionClosed;

                    Subscribe(this.topicList);
                }
                catch (Exception ex)
                {
                    log.Error($"MQTT無法連線：{ex.ToString()}");
                    log.Info($"MQTT無法連線，重新執行..." + mqttClient.GetHashCode());
                }

                System.Threading.Thread.Sleep(1000);
            }

            isCreateMqtt = false;
        }

        /// <summary>
        /// Dispose 實作
        /// </summary>
        public void Dispose()
        {
            log.Info($"MQTT關閉連線...");
            isReset = false;
            if (mqttClient != null && mqttClient.IsConnected)
            {
                try
                {

                    mqttClient.Disconnect();
                }
                catch (Exception ex)
                {
                    log.Info($"MQTT關閉連線..." + ex.ToString());
                }
            }
        }

        /// <summary>
        /// 訂閱
        /// </summary>
        /// <param name="topicList"></param>
        public void Subscribe(List<string> topicList)
        {
            this.topicList = topicList;

            foreach (var topic in this.topicList)
            {
                string[] topics = new string[] { topic };

                ushort msgId = mqttClient.Subscribe(topics, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }

            mqttClient.MqttMsgPublishReceived -= MqttMsgPublishReceived;
            mqttClient.MqttMsgPublishReceived += MqttMsgPublishReceived;
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            MqttServiceMsgPublishEventArgs args = new MqttServiceMsgPublishEventArgs
            {
                Topic = e.Topic,
                Message = Encoding.UTF8.GetString(e.Message)
            };

            SubscribeReceived(this, args);
        }

        /// <summary>
        /// 推送Mqtt
        /// </summary>
        /// <param name="topic">Topic</param>
        /// <param name="result">發送的結果</param>
        public void MqttPublish(string topic, string result)
        {
            try
            {
                mqttClient.Publish(topic, Encoding.UTF8.GetBytes(result), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);

            }
            catch (Exception ex)
            {
                log.Error($"MQTT無法發送：{ex.ToString()}");
            }
        }


        //public string IsMqttConnExist(string ip)
        //{
        //    if (string.IsNullOrWhiteSpace(ip))
        //    {
        //        return "請輸入MQTT IP位置";
        //    }

        //    this.ip = ip;
        //    mqttClient = new MqttClient(this.ip);

        //    try
        //    {
        //        byte code = mqttClient.Connect(guid);

        //        IsConnected = true;
        //        mqttClient.ConnectionClosed -= MqttClient_ConnectionClosed;
        //        mqttClient.ConnectionClosed += MqttClient_ConnectionClosed;

        //        log.Info($"MQTT連線...");
        //        mqttClient.Disconnect();
        //        return Enums.Common.TrueOrFalse.True.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        IsConnected = false;
        //        ConnectException = ex;

        //        log.Info($"MQTT連線失敗...");
        //        mqttClient.Disconnect();
        //        return Enums.Common.TrueOrFalse.False.ToString();
        //    }
        //}
    }

    public class MqttServiceMsgPublishEventArgs : EventArgs
    {
        public string Topic { get; set; }
        public string Message { get; set; }
    }
}
