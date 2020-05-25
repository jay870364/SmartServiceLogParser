using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bossinfo.Caller.MqttLogParserTesting
{
    public class IOService
    {
        /// <summary>
        /// 寫入文字檔案，全部覆蓋
        /// </summary>
        /// <param name="path">完整路徑，含檔名</param>
        /// <param name="textContent">文字內容</param>
        public static void WriteTextFIle(string path, string textContent)
        {
            //write string to file
            try
            {
                System.IO.File.WriteAllText($@"{path}", textContent, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                LogService.logger.Error(ex, "寫入文字檔案失敗");
            }

        }

        /// <summary>
        /// 讀取文字檔案
        /// </summary>
        /// <param name="path">完整路徑，含檔名</param>
        /// <returns></returns>
        public static string ReadTextFile(string path)
        {
            var result = string.Empty;

            try
            {
                result = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                LogService.logger.Error(ex, "讀取文字檔案錯誤");
            }

            return result;
        }

        public static string[] ReadTextByLine(string path)
        {
            var result = new string[] { };

            try
            {
                result = File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                LogService.logger.Error(ex, "讀取文字檔案錯誤");
            }

            return result;
        }

        /// <summary>
        /// 讀取文字檔並物件化，儲存的內容必須是Json
        /// </summary>
        /// <typeparam name="T">要轉換的物件</typeparam>
        /// <param name="path">文字檔路徑</param>
        /// <returns></returns>
        public static T ReadJsonFileToObj<T>(string path) where T : class, new()
        {
            T result = new T();

            try
            {
                result = ToolService.JsonToObj<T>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                LogService.logger.Error(ex, "讀取Json檔案錯誤");
            }

            return result;
        }

        /// <summary>
        /// 檢查檔案是否存在
        /// </summary>
        /// <param name="path">完整路徑，含檔名</param>
        /// <returns></returns>
        public static bool IsFileExist(string path)
        {
            return File.Exists(path);
        }
    }
}
