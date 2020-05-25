using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bossinfo.Caller.MqttLogParserTesting
{
    public class ToolService
    {
        /// <summary>
        /// 序列化物件
        /// </summary>
        /// <param name="obj">傳入的物件</param>
        /// <param name="formatting">格式化</param>
        /// <returns></returns>
        public static string ToJsonString(object obj, Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.None, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            return JsonConvert.SerializeObject(obj, formatting, new JsonSerializerSettings() { NullValueHandling = nullValueHandling });
        }

        /// <summary>
        /// 反序列化物件
        /// </summary>
        /// <typeparam name="T">泛型物件</typeparam>
        /// <param name="JsonString">Json字串</param>
        /// <returns></returns>
        public static T JsonToObj<T>(string JsonString)
        {
            return JsonConvert.DeserializeObject<T>(JsonString);
        }

        /// <summary>
        /// 將物件序列化成XML格式字串
        /// </summary>
        /// <typeparam name="T">物件型別</typeparam>
        /// <param name="obj">物件</param>
        /// <returns>XML格式字串</returns>
        public static string Serialize<T>(T obj) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            var stringWriter = new StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                serializer.Serialize(writer, obj);
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// 將XML格式字串反序列化成物件
        /// </summary>
        /// <typeparam name="T">物件型別</typeparam>
        /// <param name="xmlString">XML格式字串</param>
        /// <returns>反序列化後的物件</returns>
        public static T Deserialize<T>(string xmlString) where T : class
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(xmlString))
            {
                object deserializationObj = deserializer.Deserialize(reader);
                return deserializationObj as T;
            };
        }

        /// <summary>
        /// 取得列舉
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <returns></returns>
        public static T GetEnum<T>(string text)
        {
            return (T)System.Enum.Parse(typeof(T), text);

            throw new ArgumentException("Not found.", "description");
            // or return default(T);
        }

        /// <summary>
        /// 取得列舉描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi != null)
            {
                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return attributes.Length > 0 ? attributes[0].Description : value.ToString();
            }
            else
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// 取得本機IP
        /// </summary>
        /// <param name="OrgNo">組織代碼</param>
        /// <returns></returns>
        public static string GetClientIP(string OrgNo = "")
        {
            NetworkInterface[] interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            int len = interfaces.Length;
            string mip = "";

            try
            {
                for (int i = 0; i < len; i++)
                {
                    NetworkInterface ni = interfaces[i];
                    IPInterfaceProperties property = ni.GetIPProperties();
                    List<string> lstIPAddress = new List<string>();
                    IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());

                    foreach (IPAddress ipa in IpEntry.AddressList)
                    {
                        if (ipa.AddressFamily == AddressFamily.InterNetwork)
                            lstIPAddress.Add(ipa.ToString());
                    }
                    //return lstIPAddress; // result: 192.168.1.17 ......

                    mip = lstIPAddress.FirstOrDefault();
                    #region 測試中
                    //if (OrgNo.Contains("T6"))//如果是台大
                    //{
                    //    if (property.DnsSuffix.Contains("hch.gov.tw"))//驗證網卡DNS Domain
                    //    {
                    //        foreach (UnicastIPAddressInformation ip in property.UnicastAddresses)
                    //        {
                    //            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    //            {
                    //                mip = ip.Address.ToString();
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    foreach (IPAddress ipa in IpEntry.AddressList)
                    //    {
                    //        if (ipa.AddressFamily == AddressFamily.InterNetwork)
                    //            lstIPAddress.Add(ipa.ToString());
                    //    }
                    //    return lstIPAddress.FirstOrDefault();
                    //}
                    #endregion

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            LogService.logger.Info($"抓到的IP：{mip}");
            return mip;
        }

        /// <summary>
        /// 取得MAC位址
        /// </summary>
        /// <param name="OrgNo">傳入組織代碼判斷，第一次建立不傳入</param>
        /// <returns></returns>
        public static string GetMac(string OrgNo = "")
        {
            string Mac = "";
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                int len = interfaces.Length;


                for (int i = 0; i < len; i++)
                {
                    NetworkInterface ni = interfaces[i];
                    IPInterfaceProperties property = ni.GetIPProperties();

                    if (OrgNo.Contains("T6"))//台大竹北生醫的代碼判斷
                    {
                        if (property.DnsSuffix.Contains("hch.gov.tw"))//驗證網卡Domain Name
                        {
                            foreach (UnicastIPAddressInformation ip in property.UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    Mac = MacParse(ni.GetPhysicalAddress());
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (UnicastIPAddressInformation ip in property.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                Mac = MacParse(ni.GetPhysicalAddress());
                            }
                        }
                    }
                }
                return Mac;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// MAC位址輸出格式化
        /// </summary>
        /// <param name="physicalAddress"></param>
        /// <returns></returns>
        static string MacParse(PhysicalAddress physicalAddress)
        {
            var mac = physicalAddress.ToString();

            var result = string.Empty;

            if (mac.Length > 12)
            {
                return "";
            }
            for (var i = 0; i < mac.Length; i++)
            {
                if (i % 2 != 0 && i != 0 && i < mac.Length - 1)
                {
                    result += $"{mac[i]}-";
                }
                else
                {
                    result += $"{mac[i]}";
                }
            }

            return result;
        }
    }
}
