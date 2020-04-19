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

                for (int i = 0; i < productIds.Count; i++)
                {
                    List<int> projectIds = GetProjectId(productIds[i]);
                    //string[] projectList = new string[productIds.Count];

                    for (int j = 0; j < projectIds.Count; j++)
                    {
                        string json = string.Format(appconfig.GetUnclosedTaskUrl, projectIds[j]);
                        json = WebTools.Download(string.Format("{0}&{1}={2}", json, SessionName, SessionID));

                        jsonList.Add(json);
                    }                    
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

                            if (jsObj["tasks"] != null)
                            {
                                json = jsObj["tasks"].ToString();
                                jsObj = JsonConvert.DeserializeObject(json) as JObject;

                                JToken record = jsObj as JToken;
                                foreach (JProperty jp in record)
                                {
                                    var taskp = jp.First;
                                    if (taskp["status"].Value<string>() != "cancel")
                                    {
                                        int priID =int.Parse(taskp["pri"].Value<string>());

                                        TaskItem ti = new TaskItem()
                                        {
                                            Priority = Enum.GetName(typeof(CustomEnum.CustomPri), priID)
                                            ,
                                            ID = taskp["id"].Value<int>()
                                            ,
                                            Title = Util.EscapeXmlTag(taskp["name"].Value<string>())
                                            ,
                                            Deadline = taskp["deadline"].Value<string>()
                                            ,
                                            Tip = "Task"
                                            ,
                                            Type= ConvertType(taskp["type"].Value<string>())
                                            ,
                                            Status= ConvertType(taskp["status"].Value<string>())
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
                                productIds.Add(Convert.ToInt32(jp.Name));
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
        #region 任务枚举
        /// <summary>
        /// 任务状态
        /// </summary>
        /// <param name="eWord"></param>
        /// <returns></returns>
        private string ConvertStatus(string eWord)
        {
            string cword = string.Empty;

            switch (eWord.ToLower().Trim())
            {
                case "wait":
                    cword = "未开始";
                    break;
                case "doing":
                    cword = "进行中";
                    break;
                case "done":
                    cword = "已完成";
                    break;
                case "pause":
                    cword = "已暂停";
                    break;
                case "cancel":
                    cword = "已取消";
                    break;
                case "closed":
                    cword = "已关闭";
                    break;
                default:
                    eWord.ToLower().Trim();
                    break;
            }

            return cword;
        }
        private string ConvertType(string eWord)
        {
            string cword = string.Empty;

            switch (eWord.ToLower().Trim())
            {
                case "design":
                    cword = "设计";
                    break;
                case "document":
                    cword = "文档";
                    break;
                case "devel":
                    cword = "开发";
                    break;
                case "server":
                    cword = "服务端";
                    break;
                case "client":
                    cword = "客户端";
                    break;
                case "cinfig":
                    cword = "配置";
                    break;
                case "numerical":
                    cword = "数值";
                    break;
                case "art":
                    cword = "美术";
                    break;
                case "test":
                    cword = "测试";
                    break;
                case "discuss":
                    cword = "讨论";
                    break;
                case "ui":
                    cword = "UI界面";
                    break;
                case "affair":
                    cword = "事务";
                    break;
                case "misc":
                    cword = "其他";
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
