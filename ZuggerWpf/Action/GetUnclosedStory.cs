using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;

namespace ZuggerWpf
{
    class GetUnclosedStory : ActionBase, IActionBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ZuggerObservableCollection<StoryItem> itemsList = null;

        public GetUnclosedStory(ZuggerObservableCollection<StoryItem> zItems)
        {
            itemsList = zItems;
        }

        public override bool Action()
        {
            bool isSuccess = false;
            NewItemCount = 0;

            List<int> executionIds = GetExecutionId();
            string[] jsonList = new string[executionIds.Count];

            try
            {
                ApplicationConfig appconfig = IOHelper.LoadIsolatedData();

                for (int i = 0; i < executionIds.Count; i++)
                {
                    string json = string.Format(appconfig.GetUnclosedStoryUrl, executionIds[i]);
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
                            string ExecutionName= jsObj["title"].ToString().Substring(0, jsObj["title"].ToString().Length - 7);
                            if (jsObj["stories"] != null)
                            {
                                //获取用户字典
                                Dictionary<string, string> usersDic = new Dictionary<string, string>();
                                var jsObjUsers = JsonConvert.DeserializeObject(jsObj["users"].ToString()) as JObject;
                                
                                JToken recordUser = jsObjUsers as JToken;
                                if (recordUser != null)
                                {
                                    foreach (JProperty jp in recordUser)
                                    {
                                        usersDic.Add(jp.Name, jp.Value.ToString());
                                    }
                                }

                                jsObj = JsonConvert.DeserializeObject(jsObj["stories"].ToString()) as JObject;

                                JToken record = jsObj as JToken;
                                if(record != null)
                                {
                                    foreach (JProperty jp in record)
                                    {
                                        var jpFirst = jp.First;
                                        if (jpFirst["status"].Value<string>() != "cancel")
                                        {
                                            StoryItem storyItem = new StoryItem()
                                            {
                                                Priority = Convert.Pri(jpFirst["pri"].Value<string>())
                                                ,
                                                ID = jpFirst["id"].Value<int>()
                                                ,
                                                Title = jpFirst["title"].Value<string>()
                                                ,
                                                OpenDate = jpFirst["openedDate"].Value<string>()
                                                ,
                                                Stage = Convert.Stage(jpFirst["stage"].Value<string>())
                                                ,
                                                AssignedToName = usersDic[jpFirst["assignedTo"].Value<string>()]
                                            };

                                            if (!ItemCollectionBackup.Contains(storyItem.ID))
                                            {
                                                NewItemCount = NewItemCount == 0 ? storyItem.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                            }
                                            storyItem.Execution = ExecutionName;
                                            itemsList.Add(storyItem);
                                        }
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
                logger.Error(string.Format("GetUnclosedStory Error: {0}", exp.ToString()));
            }

            return isSuccess;
        }

        private string ConvertStage(string eWord)
        {
            string cword = string.Empty;

            switch (eWord.ToLower().Trim())
            {
                case "wait":
                    cword = "未开始";
                    break;
                case "planned":
                    cword = "已计划";
                    break;
                case "projected":
                    cword = "已立项";
                    break;
                case "developing":
                    cword = "研发中";
                    break;
                case "developed":
                    cword = "研发完毕";
                    break;
                case "testing":
                    cword = "测试中";
                    break;
                case "tested":
                    cword = "测试完毕";
                    break;
                case "verified":
                    cword = "已验收";
                    break;
                case "released":
                    cword = "已发布";
                    break;
                default:
                    eWord.ToLower().Trim();
                    break;
            }

            return cword;
        }
        private List<int> GetExecutionId()
        {
            List<int> executionIds = new List<int>();
            try
            {
                ApplicationConfig appconfig = IOHelper.LoadIsolatedData();

                string json = WebTools.Download(string.Format("{0}&{1}={2}", appconfig.GetExecutionUrl, SessionName, SessionID));

                if (!string.IsNullOrEmpty(json))
                {
                    var jsObj = JsonConvert.DeserializeObject(json) as JObject;

                    if (jsObj != null && jsObj["status"].Value<string>() == "success")
                    {
                        json = jsObj["data"].Value<string>();

                        jsObj = JsonConvert.DeserializeObject(json) as JObject;

                        if (jsObj["executionStats"] != null)
                        {
                            JArray jsArray = (JArray)JsonConvert.DeserializeObject(jsObj["executionStats"].ToString());
                            foreach (var j in jsArray)
                            {
                                executionIds.Add(System.Convert.ToInt32(j["id"].ToString()));
                            }

                        }
                    }
                }
            }
            catch (Exception exp)
            {
                logger.Error(string.Format("GetProductId Error: {0}", exp.ToString()));
            }

            return executionIds;
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
