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
    class GetOpenedByMeBug : ActionBase, IActionBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ZuggerObservableCollection<BugItem> itemsList = null;

        public GetOpenedByMeBug(ZuggerObservableCollection<BugItem> zItems)
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
                    string json = string.Format(appconfig.GetOpenedByMeBugUrl, productIds[i]);
                    json = WebTools.Download(string.Format("{0}&{1}={2}", json, SessionName, SessionID));

                    jsonList[i] =json;
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
                                //获取产品字典
                                Dictionary<string, string> productDic = new Dictionary<string, string>();
                                var jsObjProduct = JsonConvert.DeserializeObject(jsObj["products"].ToString()) as JObject;
                                JToken recordProduct = jsObjProduct as JToken;
                                if (recordProduct != null)
                                {
                                    foreach (JProperty jp in recordProduct)
                                    {
                                        productDic.Add(jp.Name,jp.Value.ToString());
                                    }
                                }

                                JArray BugsArray = (JArray)JsonConvert.DeserializeObject(jsObj["bugs"].ToString());

                                foreach (var bug in BugsArray)
                                {
                                    //openedbyme 显示未关闭
                                    if (bug["status"].Value<string>() != "closed")// && j["status"].Value<string>() != "resolved"
                                    {
                                        BugItem bugItem = new BugItem()
                                        {
                                            Priority = Convert.Pri(bug["pri"].Value<string>())
                                    ,
                                            Severity = Convert.Severity(bug["severity"].Value<string>())
                                            ,
                                            ID = bug["id"].Value<int>()
                                            ,
                                            Title = Util.EscapeXmlTag(bug["title"].Value<string>())
                                            ,
                                            OpenDate = bug["openedDate"].Value<string>()
                                            ,
                                            LastEdit = bug["lastEditedDate"].Value<string>()
                                            ,
                                            Tip = "我开的Bug"
                                            ,
                                            Confirmed = Convert.Confirmed(bug["confirmed"].Value<string>())
                                            ,
                                            Resolution = Convert.Resolution(bug["resolution"].Value<string>())
                                            ,
                                            Product= productDic[bug["product"].Value<string>()]
                                        };

                                        if (!ItemCollectionBackup.Contains(bugItem.ID))
                                        {
                                            NewItemCount = NewItemCount == 0 ? bugItem.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                        }

                                        itemsList.Add(bugItem);
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
                logger.Error(string.Format("GetOpenedByMeBug Error: {0}", exp.ToString()));
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
        #region ActionBaseInterface Members

        public event NewItemArrive OnNewItemArrive;

        #endregion
    }
}
