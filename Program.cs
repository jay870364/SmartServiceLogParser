using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bossinfo.Caller.MqttLogParserTesting
{
    class Program
    {
        static DateTime? preDateTime = null;

        static DateTime today;

        static MqttService mqs;

        static LogService lg;


        static string DevTopic;

        static string DevId;

        static string MqIp;
        static string SDate;

        static void Main(string[] args)
        {
            UnitTesting();
        }


        static void UnitTesting()
        {

            MqIp = ToolService.GetConfig("MqIp");

            DevId = ToolService.GetConfig("DevId");

            SDate = ToolService.GetConfig("SDate");

            DevTopic = $"SmartBoard/Device/{DevId}/ToDev";



            var tmp = ReadLog($@"{Directory.GetCurrentDirectory()}\Data\Data.log");

            var closeDev = IOService.ReadTextByLine($@"{Directory.GetCurrentDirectory()}\Data\Data.log").FirstOrDefault().Replace("{{DevId}}", DevId);

            today = Convert.ToDateTime(SDate);

            mqs = new MqttService(MqIp);

            lg = new LogService();

            var run = true;

            while (run)
            {
                mqs.MqttPublish(DevTopic, closeDev);
                System.Threading.Thread.Sleep(3000);
                try
                {
                    var send = tmp.Where(o => o.Need == true).ToList();

                    //send.Count();
                    send.ForEach(x =>
                    {

                        MqPublish(x);
                        //System.Threading.Thread.Sleep(1000);
                        //if (i > 10)
                        //{
                        //    return;
                        //}

                        //i++;
                    });
                    Log("測試結束");
                }
                catch (Exception ex)
                {
                    Log($"錯誤\n{ex.ToString()}");
                }
                break;
            }
        }


        static void MqPublish(DataModel dm)
        {
            //判斷是否有前一個時間
            if (!preDateTime.HasValue)
            {
                preDateTime = dm.DateTime;
                mqs.MqttPublish(DevTopic, dm.Send);
                Log($"現在時間:{DateTime.Now.ToString("HHmmss")}\t{preDateTime.Value.ToString("yyyyMMdd HHmmss")}\tCmdDt:{dm.DateTime.ToString("yyyyMMdd HHmmss")}\t{dm.Cmd}\t{dm.Send.Substring(0, 50)}");
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
                        Log(dm.Send);
                        //等候時間是否大於60
                        if (dtR.TotalSeconds > 60)
                        {
                            Log($"睡{dtR.Seconds}秒");

                            System.Threading.Thread.Sleep(5 * 1000);

                            mqs.MqttPublish(DevTopic, dm.Send);
                            Log($"現在時間:{DateTime.Now.ToString("HHmmss")}\t{preDateTime.Value.ToString("yyyyMMdd HHmmss")}\tCmdDt:{dm.DateTime.ToString("yyyyMMdd HHmmss")}\t秒:{dtR.TotalSeconds}\t{dm.Cmd}\t{dm.Send.Substring(0, 50)}");
                        }
                        else if (dtR.TotalSeconds == 0)
                        {
                            Log($"睡{dtR.Seconds}秒");

                            mqs.MqttPublish(DevTopic, dm.Send);
                            Log($"現在時間:{DateTime.Now.ToString("HHmmss")}\t{preDateTime.Value.ToString("yyyyMMdd HHmmss")}\tCmdDt:{dm.DateTime.ToString("yyyyMMdd HHmmss")}\t秒:{dtR.TotalSeconds}\t{dm.Cmd}\t{dm.Send.Substring(0, 50)}");
                        }
                        else
                        {
                            Log($"睡{dtR.Seconds}秒");

                            System.Threading.Thread.Sleep(5 * 1000);

                            mqs.MqttPublish(DevTopic, dm.Send);
                            Log($"現在時間:{DateTime.Now.ToString("HHmmss")}\t{preDateTime.Value.ToString("yyyyMMdd HHmmss")}\tCmdDt:{dm.DateTime.ToString("yyyyMMdd HHmmss")}\t秒:{dtR.TotalSeconds}\t{dm.Cmd}\t{dm.Send.Substring(0, 50)}");
                        }

                    }
                    else
                    {

                        System.Threading.Thread.Sleep(1 * 1000);
                        mqs.MqttPublish(DevTopic, dm.Send);
                        Log($"現在時間:{DateTime.Now.ToString("HHmmss")}\t{preDateTime.Value.ToString("yyyyMMdd HHmmss")}\tCmdDt:{dm.DateTime.ToString("yyyyMMdd HHmmss")}\t秒:0\t{dm.Cmd}\t{dm.Send.Substring(0, 50)}");
                    }
                }
                else
                {
                    mqs.MqttPublish(DevTopic, dm.Send);
                    Log($"現在時間:{DateTime.Now.ToString("HHmmss")}\t{preDateTime.Value.ToString("yyyyMMdd HHmmss")}\tCmdDt:{dm.DateTime.ToString("yyyyMMdd HHmmss")}\t秒:0\t{dm.Cmd}\t{dm.Send.Substring(0, 50)}");
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
                    if (y.Length >= 20)
                    {
                        result.Add(new DataModel() { Data = y });
                    }
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

                try
                {


                    return ary4.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ary4.ToString());
                    Console.WriteLine(ex.ToString());
                    return string.Empty;
                }
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

    }
}
