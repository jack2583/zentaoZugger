using System;
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
                            string ProductName = jsObj["title"].ToString().Substring(0, jsObj["title"].ToString().Length - 4);
                            if (jsObj["bugs"] != null)
                            {
                                //获取用户字典
                                Dictionary<string, string> usersDic = new Dictionary<string, string>();
                                usersDic.Add("", "未指派");
                                var jsObjUsers = JsonConvert.DeserializeObject(jsObj["users"].ToString()) as JObject;

                                JToken recordUser = jsObjUsers as JToken;
                                if (recordUser != null)
                                {
                                    foreach (JProperty jp in recordUser)
                                    {
                                        usersDic.Add(jp.Name, jp.Value.ToString());
                                    }
                                }


                                var jsObj2 = JsonConvert.DeserializeObject(jsObj["bugs"].ToString()) as JObject;
                                JToken record = jsObj2 as JToken;
                                if (record != null)
                                {
                                    foreach (JProperty jp in record)
                                    {
                                        var bug = jp.First;
                                        if (bug["status"].Value<string>() != "cancel")
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
                                                LastEditDate = bug["lastEditedDate"].Value<string>()
                                                ,
                                                Tip = "未关闭的Bug"
                                                ,
                                                Confirmed = Convert.Confirmed(bug["confirmed"].Value<string>())
                                                ,
                                                Resolution = Convert.Resolution(bug["resolution"].Value<string>())
                                                ,
                                                AssignedToName = usersDic[bug["assignedTo"].Value<string>()]
                                            };

                                            if (!ItemCollectionBackup.Contains(bugItem.ID))
                                            {
                                                NewItemCount = NewItemCount == 0 ? bugItem.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                            }
                                            bugItem.Product = ProductName;
                                            itemsList.Add(bugItem);
                                           // Dict.BugOfStoryDict.Add(bugItem.ID, bugItem);
                                        }
                                    }
                                    if (OnNewItemArrive != null
                                        && NewItemCount != 0)
                                    {
                                        OnNewItemArrive(ItemType.Task, NewItemCount);
                                    }
                                }
                                /*
                                JArray BugsArray = (JArray)JsonConvert.DeserializeObject(jsObj["bugs"].ToString());
                                foreach (var bug in BugsArray)
                                {
                                    //unclosedBug 显示未关闭
                                    if (bug["status"].Value<string>() != "closed")//&& j["status"].Value<string>() != "resolved"
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
                                            LastEditDate = bug["lastEditedDate"].Value<string>()
                                            ,
                                            Tip = "未关闭的Bug"
                                            ,
                                            Confirmed = Convert.Confirmed(bug["confirmed"].Value<string>())
                                            ,
                                            Resolution = Convert.Resolution(bug["resolution"].Value<string>())
                                            ,
                                            AssignedToName = usersDic[bug["assignedTo"].Value<string>()]
                                        };

                                        if (!ItemCollectionBackup.Contains(bugItem.ID))
                                        {
                                            NewItemCount = NewItemCount == 0 ? bugItem.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                        }
                                        bugItem.Product = ProductName;
                                        itemsList.Add(bugItem);
                                    }
                                }
                            */
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
