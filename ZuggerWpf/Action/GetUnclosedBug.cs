﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using log4net;

namespace ZuggerWpf
{
    class GetUnclosedBug : ActionBase, IActionBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ZuggerObservableCollection<BugItem> itemsList = null;

        public GetUnclosedBug(ZuggerObservableCollection<BugItem> zItems)
        {
            itemsList = zItems;
        }

        public override bool Action()
        {
            bool isSuccess = false;
            NewItemCount = 0;

            List<int> productIds = GetProductId();
            string[] jsonList = new string[productIds.Count];

            try
            {
                ApplicationConfig appconfig = IOHelper.LoadIsolatedData();

                for (int i = 0; i < productIds.Count; i++)
                {
                    string json = string.Format(appconfig.GetUnclosedBugUrl, productIds[i]);
                    json = WebTools.Download(string.Format("{0}&{1}={2}", json, SessionName, SessionID));

                    jsonList[i] = json;
                }

                bool isNewJson = IsNewJson(string.Concat(jsonList));

                if (!isNewJson)
                {
                    return true;
                }

                ItemCollectionBackup.AddRange(itemsList.Select(f => f.ID));
                itemsList.Clear();

                foreach (string strjson in jsonList)
                {
                    string json = strjson;

                    if (!string.IsNullOrEmpty(json) && isNewJson)
                    {
                        var jsObj = JsonConvert.DeserializeObject(json) as JObject;

                        if (jsObj != null && jsObj["status"].Value<string>() == "success")
                        {
                            json = jsObj["data"].Value<string>();

                            jsObj = JsonConvert.DeserializeObject(json) as JObject;

                            if (jsObj["bugs"] != null)
                            {
                                JArray jsArray = (JArray)JsonConvert.DeserializeObject(jsObj["bugs"].ToString());

                                foreach (var j in jsArray)
                                {
                                    //unclosedBug 显示未关闭
                                    if (j["status"].Value<string>() != "closed")//&& j["status"].Value<string>() != "resolved"
                                    {
                                        BugItem bi = new BugItem()
                                        {
                                            Priority = Convert.Pri(j["pri"].Value<string>())
                                    ,
                                            Severity = Convert.Severity(j["severity"].Value<string>())
                                            ,
                                            ID = j["id"].Value<int>()
                                            ,
                                            Title = Util.EscapeXmlTag(j["title"].Value<string>())
                                            ,
                                            OpenDate = j["openedDate"].Value<string>()
                                            ,
                                            LastEdit = j["lastEditedDate"].Value<string>()
                                            ,
                                            Tip = "未关闭的Bug"
                                            ,
                                            Confirmed = Convert.Confirmed(j["confirmed"].Value<string>())
                                            ,
                                            Resolution = Convert.Resolution(j["resolution"].Value<string>())
                                        };

                                        if (!ItemCollectionBackup.Contains(bi.ID))
                                        {
                                            NewItemCount = NewItemCount == 0 ? bi.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                        }

                                        itemsList.Add(bi);
                                    }
                                }
                            }

                            isSuccess = true;
                        }
                    }
                }

                if (OnNewItemArrive != null
                    && NewItemCount != 0)
                {
                    OnNewItemArrive(ItemType.Bug, NewItemCount);
                }

                ItemCollectionBackup.Clear();
            }
            catch (Exception exp)
            {
                logger.Error(string.Format("GetUnclosedBug Error: {0}", exp.ToString()));
            }

            return isSuccess;
        }

        private List<int> GetProductId()
        {
            List<int> productIds = new List<int>();
            try
            {
                ApplicationConfig appconfig = IOHelper.LoadIsolatedData();

                string json = WebTools.Download(string.Format("{0}&{1}={2}", appconfig.GetProductUrl, SessionName, SessionID));

                if (!string.IsNullOrEmpty(json))
                {
                    var jsObj = JsonConvert.DeserializeObject(json) as JObject;

                    if (jsObj != null && jsObj["status"].Value<string>() == "success")
                    {
                        json = jsObj["data"].Value<string>();

                        jsObj = JsonConvert.DeserializeObject(json) as JObject;

                        if (jsObj["products"] != null)
                        {
                            jsObj = JsonConvert.DeserializeObject(jsObj["products"].ToString()) as JObject;
                            JProperty jp = jsObj.First as JProperty;

                            while (jp != null)
                            {
                                productIds.Add(System.Convert.ToInt32(jp.Name));
                                jp = jp.Next as JProperty;
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                logger.Error(string.Format("GetProductId Error: {0}", exp.ToString()));
            }

            return productIds;
        }
        #region 任务枚举
        private string ConvertResolution(string eWord)
        {
            string cword = string.Empty;

            switch (eWord.ToLower().Trim())
            {
                case "bydesign":
                    cword = "设计如此";
                    break;
                case "duplicate":
                    cword = "重复BUG";
                    break;
                case "external":
                    cword = "外部原因";
                    break;
                case "fixed":
                    cword = "已解决";
                    break;
                case "notrepro":
                    cword = "无法重现";
                    break;
                case "postponed":
                    cword = "延期处理";
                    break;
                case "willnotfix":
                    cword = "不予解决";
                    break;
                case "toshory":
                    cword = "转为需求";
                    break;
                default:
                    eWord.ToLower().Trim();
                    break;
            }

            return cword;
        }
        private string ConvertConfirmed(string eWord)
        {
            string cword = string.Empty;

            switch (eWord.ToLower().Trim())
            {
                case "0":
                    cword = "未确认";
                    break;
                case "1":
                    cword = "确认";
                    break;
                default:
                    eWord.ToLower().Trim();
                    break;
            }

            return cword;
        }
        #endregion
        #region ActionBaseInterface Members

        public event NewItemArrive OnNewItemArrive;

        #endregion
    }
}
