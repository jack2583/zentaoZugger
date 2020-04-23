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
    class GetUnclosedTask : ActionBase, IActionBase
    {
        ZuggerObservableCollection<TaskItem> itemsList = null;

        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public GetUnclosedTask(ZuggerObservableCollection<TaskItem> zItems)
        {
            itemsList = zItems;
        }

        public override bool Action()
        {
            bool isSuccess = false;
            NewItemCount = 0;

            List<int> productIds = GetProductId();
            //string[] jsonList = new string[productIds.Count];
            List<string> jsonList = new List<string>();
            

            try
            {
                ApplicationConfig appconfig = IOHelper.LoadIsolatedData();

                //for (int i = 0; i < productIds.Count; i++)
                //{
                    List<int> projectIds = GetProjectId();
                    //string[] projectList = new string[productIds.Count];

                    for (int j = 0; j < projectIds.Count; j++)
                    {
                        string json = string.Format(appconfig.GetUnclosedTaskUrl, projectIds[j]);
                        json = WebTools.Download(string.Format("{0}&{1}={2}", json, SessionName, SessionID));

                        jsonList.Add(json);
                    }                    
                //}

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

                            if (jsObj["tasks"] != null )
                            {
                                json = jsObj["tasks"].ToString();
                                if (json == "[]")
                                    continue;

                                jsObj = JsonConvert.DeserializeObject(json) as JObject;

                                JToken record = jsObj as JToken;
                                foreach (JProperty jp in record)
                                {
                                    var taskp = jp.First;
                                    if (taskp["status"].Value<string>() != "cancel")
                                    {
                                        TaskItem ti = new TaskItem()
                                        {
                                            Priority = Convert.Pri(taskp["pri"].Value<string>())
                                            ,
                                            ID = taskp["id"].Value<int>()
                                            ,
                                            Title = Util.EscapeXmlTag(taskp["name"].Value<string>())
                                            ,
                                            Deadline = taskp["deadline"].Value<string>()
                                            ,
                                            Tip = "Task"
                                            ,
                                            Type= Convert.Type(taskp["type"].Value<string>())
                                            ,
                                            Status= Convert.Status(taskp["status"].Value<string>())
                                            ,
                                            Progress = taskp["progress"].Value<string>() + "%"
                                        };

                                        if (!ItemCollectionBackup.Contains(ti.ID))
                                        {
                                            NewItemCount = NewItemCount == 0 ? ti.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                        }

                                        itemsList.Add(ti);
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
