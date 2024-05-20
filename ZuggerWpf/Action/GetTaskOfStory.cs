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
    class GetTaskOfSory : ActionBase, IActionBase
    {
        ZuggerObservableCollection<TaskItem> itemsList = null;

        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public GetTaskOfSory(ZuggerObservableCollection<TaskItem> zItems)
        {
            itemsList = zItems;
        }

        public override bool Action()
        {
            bool isSuccess = false;
            NewItemCount = 0;

            List<string> jsonList = new List<string>();


            try
            {
                ApplicationConfig appconfig = IOHelper.LoadIsolatedData();

#if DEBUG
                if (Dict.StoryID == 27)
                    Dict.StoryID = 27;
#endif
                string jsonstr = string.Format(appconfig.GetStoryTasksUrl,Dict.StoryID);
                jsonstr = WebTools.Download(string.Format("{0}&{1}={2}", jsonstr, SessionName, SessionID));

                bool isNewJson = IsNewJson(jsonstr);
                if (isNewJson)
                {
                    jsonList.Add(jsonstr);
                }

                if (jsonList == null || jsonList.Count == 0)
                    return true;


                ItemCollectionBackup.AddRange(itemsList.Select(f => f.ID));
                itemsList.Clear();
                Dict.TaskOfStoryDict.Clear();
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
                        //string ExecutionName = jsObj["title"].ToString().Substring(0, jsObj["title"].ToString().Length - 5);
                            if (jsObj["tasks"] != null)
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
                                var jsObj2 = JsonConvert.DeserializeObject(jsObj["tasks"].ToString()) as JObject;
                                JToken record = jsObj2 as JToken;
                                if (record != null)
                                {
                                    foreach (JProperty jp in record)
                                    {
                                        var jpFirst = jp.First;
                                        if (jpFirst["status"].Value<string>() != "cancel")
                                        {
                                            TaskItem taskItem = new TaskItem()
                                            {
                                                Priority = Convert.Pri(jpFirst["pri"].Value<string>())
                                                     ,
                                                ID = jpFirst["id"].Value<int>()
                                                     ,
                                                Title = Util.EscapeXmlTag(jpFirst["name"].Value<string>())
                                                     ,
                                                Tip = "Task"
                                                
                                                      ,
                                                Status = Convert.Status(jpFirst["status"].Value<string>())
                                                      ,
                                                Progress = jpFirst["progress"].Value<string>()
                                                     ,
                                                AssignedToName = usersDic[jpFirst["assignedTo"].Value<string>()]
                                                 ,
                                                ClosedReason = jpFirst["closedReason"].Value<string>()

                                            };

                                            if (!ItemCollectionBackup.Contains(taskItem.ID))
                                            {
                                                NewItemCount = NewItemCount == 0 ? taskItem.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                            }
                                            //taskItem.ProjectName = ExecutionName;


                                            itemsList.Add(taskItem);
                                            Dict.TaskOfStoryDict.Add(taskItem.ID, taskItem);
                                        }
                                    }


                                    if (OnNewItemArrive != null
                                        && NewItemCount != 0)
                                    {
                                        OnNewItemArrive(ItemType.Task, NewItemCount);
                                    }
                                }
                            }
                            isSuccess = true;
                        }
                    }
                }
                if (OnNewItemArrive != null && NewItemCount != 0)
                {
                    OnNewItemArrive(ItemType.Task, NewItemCount);
                }
                ItemCollectionBackup.Clear();
            }
            catch (Exception exp)
            {
                logger.Error(string.Format("GetUnclosedTask Error:{0}", exp.ToString()));
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
        private List<int> GetProjectId(int productId)
        {
            List<int> projectIds = new List<int>();
            try
            {
                ApplicationConfig appconfig = IOHelper.LoadIsolatedData();

                string json = string.Format(appconfig.GetProjectUrl, productId);

                json = WebTools.Download(string.Format("{0}&{1}={2}", json, SessionName, SessionID));

                if (!string.IsNullOrEmpty(json))
                {
                    var jsObj = JsonConvert.DeserializeObject(json) as JObject;

                    if (jsObj != null && jsObj["status"].Value<string>() == "success")
                    {
                        json = jsObj["data"].Value<string>();

                        jsObj = JsonConvert.DeserializeObject(json) as JObject;

                        if (jsObj["projectStats"] != null)
                        {
                            JArray jsArray = (JArray)JsonConvert.DeserializeObject(jsObj["projectStats"].ToString());

                            foreach (var j in jsArray)
                            {
                                //显示未关闭
                                if (j["status"].Value<string>() != "closed" && j["status"].Value<string>() != "resolved")
                                {
                                    projectIds.Add(j["id"].Value<int>());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                logger.Error(string.Format("GetProjectId Error: {0}", exp.ToString()));
            }

            return projectIds;
        }
        private List<int> GetProjectId()
        {
            List<int> projectIds = new List<int>();
            try
            {
                ApplicationConfig appconfig = IOHelper.LoadIsolatedData();

                //string json = string.Format(appconfig.GetProjectUrl, productId);

                string json = WebTools.Download(string.Format("{0}&{1}={2}", appconfig.GetAllProjectUrl, SessionName, SessionID));

                if (!string.IsNullOrEmpty(json))
                {
                    var jsObj = JsonConvert.DeserializeObject(json) as JObject;

                    if (jsObj != null && jsObj["status"].Value<string>() == "success")
                    {
                        json = jsObj["data"].Value<string>();

                        jsObj = JsonConvert.DeserializeObject(json) as JObject;

                        if (jsObj["projectStats"] != null)
                        {
                            JArray jsArray = (JArray)JsonConvert.DeserializeObject(jsObj["projectStats"].ToString());

                            foreach (var j in jsArray)
                            {
                                //显示未关闭
                                if (j["status"].Value<string>() != "closed" && j["status"].Value<string>() != "resolved")
                                {
                                    projectIds.Add(j["id"].Value<int>());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                logger.Error(string.Format("GetProjectId Error: {0}", exp.ToString()));
            }

            return projectIds;
        }
#region ActionBaseInterface Members

        public event NewItemArrive OnNewItemArrive;

#endregion
    }
}
