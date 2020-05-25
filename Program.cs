using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bossinfo.Caller.MqttLogParserTesting
{
    class Program
    {
        static DateTime? preDateTime = null;

        static DateTime today;

        static MqttService mqs;

        static LogService lg;

        static string Topic = "";

        static string DevSn = "";

        static string MqIp = "";

        static void Main(string[] args)
        {
            try
            {
                var tmp = ReadLog($@"{AppDomain.CurrentDomain.BaseDirectory}\Data\Data.log");

                var closeDev = IOService.ReadTextByLine($@"{AppDomain.CurrentDomain.BaseDirectory}\Data\Close.txt").FirstOrDefault();

                DevSn = System.Configuration.ConfigurationManager.AppSettings["DevSn"];

                MqIp = System.Configuration.ConfigurationManager.AppSettings["MqIp"];

                closeDev.Replace("{DevSn}", DevSn);

                Topic = $"SmartBoard/Device/{DevSn}/ToDev";

                today = Convert.ToDateTime("2020-03-19 00:00:00");

                mqs = new MqttService(MqIp);

                lg = new LogService();

                Log($"DevSn:{DevSn}\tTopic:{Topic}");


                mqs.MqttPublish(Topic, closeDev);
                System.Threading.Thread.Sleep(5000);

                var send = tmp.Where(o => o.Need == true).ToList();

                send.ForEach(x =>
                {

                    MqPublish(x);

                });
                Log("測試結束");
            }
            catch (Exception ex)
            {
                Log($"錯誤\n{ex.ToString()}");
            }
            Environment.Exit(Environment.ExitCode);
        }


        static void MqPublish(DataModel dm)
        {
            if (dm.IsRestart)
            {
                mqs.MqttPublish(Topic, dm.Send);

                Thread.Sleep(60 * 1000);
                return;
            }

            //判斷是否有前一個時間
            if (!preDateTime.HasValue)
            {
                preDateTime = dm.DateTime;
                mqs.MqttPublish(Topic, dm.Send);
                Log($"{DateTime.Now.ToString("HHmmss")}\t{preDateTime.Value.ToString("yyyyMMdd HHmmss")}\tCmdDt:{dm.DateTime.ToString("yyyyMMdd HHmmss")}\t{dm.Cmd}\t{dm.ReqId}");
            }
            else
            {
                //判斷是否是正常時間
                if (dm.BigThanDefault)
                {
                    if (preDateTime.Value.CompareTo(today) > 0)
                    {
                        var dt1 = new TimeSpan(preDateTime.Value.Ticks);

                        var dt2 = new TimeSpan(dm.DateTime.Ticks);

                        var dtR = dt2 - dt1;

                        //等候時間是否大於60
                        if (dtR.TotalSeconds > 60)
                        {
                            Log($"睡{dtR.Seconds}秒");

                            System.Threading.Thread.Sleep(5 * 1000);

                            mqs.MqttPublish(Topic, dm.Send);
                            Log($"現在時間:{DateTime.Now.ToString("HHmmss")}\t{preDateTime.Value.ToString("yyyyMMdd HHmmss")}\tCmdDt:{dm.DateTime.ToString("yyyyMMdd HHmmss")}\t秒:{dtR.TotalSeconds}\t{dm.Cmd}\t{dm.ReqId}");
                        }
                        else if (dtR.TotalSeconds == 0)
                        {
                            Log($"睡{dtR.Seconds}秒");
                            //System.Threading.Thread.Sleep(1 * 100);
                            mqs.MqttPublish(Topic, dm.Send);
                            Log($"現在時間:{DateTime.Now.ToString("HHmmss")}\t{preDateTime.Value.ToString("yyyyMMdd HHmmss")}\tCmdDt:{dm.DateTime.ToString("yyyyMMdd HHmmss")}\t秒:{dtR.TotalSeconds}\t{dm.Cmd}\t{dm.ReqId}");
                        }
                        else
                        {
                            Log($"睡{dtR.Seconds}秒");

                            System.Threading.Thread.Sleep(5 * 1000);

                            mqs.MqttPublish(Topic, dm.Send);
                            Log($"現在時間:{DateTime.Now.ToString("HHmmss")}\t{preDateTime.Value.ToString("yyyyMMdd HHmmss")}\tCmdDt:{dm.DateTime.ToString("yyyyMMdd HHmmss")}\t秒:{dtR.TotalSeconds}\t{dm.Cmd}\t{dm.ReqId}");
                        }

                    }
                    else
                    {

                        System.Threading.Thread.Sleep(1 * 1000);
                        mqs.MqttPublish(Topic, dm.Send);
                        Log($"現在時間:{DateTime.Now.ToString("HHmmss")}\t{preDateTime.Value.ToString("yyyyMMdd HHmmss")}\tCmdDt:{dm.DateTime.ToString("yyyyMMdd HHmmss")}\t秒:0\t{dm.Cmd}\t{dm.ReqId}");
                    }
                }
                else
                {
                    mqs.MqttPublish(Topic, dm.Send);
                    Log($"現在時間:{DateTime.Now.ToString("HHmmss")}\t{preDateTime.Value.ToString("yyyyMMdd HHmmss")}\tCmdDt:{dm.DateTime.ToString("yyyyMMdd HHmmss")}\t秒:0\t{dm.Cmd}\t{dm.ReqId}");
                }
            }

            preDateTime = dm.DateTime;
        }

        static List<DataModel> ReadLog(string path)
        {
            var x = IOService.ReadTextByLine($@"{path}");

            var result = new List<DataModel>();

            x.ToList().ForEach(

                y =>
                {
                    result.Add(new DataModel() { Data = y });
                });
            return result;
        }

        static void Log(string data)
        {
            Console.WriteLine(data);

            lg.Info(data);
        }

    }

    public class DataModel
    {

        DateTime DefaultDt
        {
            get
            {
                return Convert.ToDateTime("2014-01-01 00:00:00");
            }
        }

        DateTime Today
        {
            get
            {
                return Convert.ToDateTime("2020-03-19 00:00:00");
            }
        }
        public string Data { get; set; }

        public DateTime DateTime
        {
            get
            {
                return Convert.ToDateTime((Data.Substring(0, 20)));
            }
        }

        public string Cmd
        {
            get
            {
                string[] ary4 = Regex.Split(Data.Substring(20), @" : ");

                return ary4.FirstOrDefault();
            }
        }

        public string Send
        {
            get
            {
                string[] ary4 = Regex.Split(Data.Substring(20), @" : ");

                return ary4.LastOrDefault();
            }
        }

        public string ReqId
        {
            get
            {
                string[] ary4 = Regex.Split(Send.Substring(20), @":");

                return ary4.LastOrDefault();
            }
        }
        public bool Need
        {

            get
            {
                switch (Cmd.Trim())
                {
                    case "REQ":
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool BigThanDefault
        {
            get
            {
                return this.DateTime.CompareTo(this.DefaultDt) > 0;
            }
        }

        public bool IsRestart
        {
            get
            {
                return Send.Contains("\"Param1\":\"Restart\"");
            }
        }
    }
}
