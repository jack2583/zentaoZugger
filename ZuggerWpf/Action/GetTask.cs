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
    class GetTask : ActionBase, IActionBase
    {
        ZuggerObservableCollection<TaskItem> itemsList = null;

        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public GetTask(ZuggerObservableCollection<TaskItem> zItems)
        {
            itemsList = zItems;
        }

        public override bool Action()
        {
            bool isSuccess = false;
            NewItemCount = 0;

            try
            {
                ApplicationConfig appconfig = IOHelper.LoadIsolatedData();

                string json = WebTools.Download(string.Format("{0}&{1}={2}", appconfig.GetTaskUrl, SessionName, SessionID));

                if (!string.IsNullOrEmpty(json) && IsNewJson(json))
                {
                    ItemCollectionBackup.AddRange(itemsList.Select(f => f.ID));

                    itemsList.Clear();

                    var jsObj = JsonConvert.DeserializeObject(json) as JObject;

                    if (jsObj != null && jsObj["status"].Value<string>() == "success")
                    {
                        json = jsObj["data"].Value<string>();

                        jsObj = JsonConvert.DeserializeObject(json) as JObject;

                        if (jsObj["tasks"] != null)
                        {
                            JArray tasksArray = (JArray)JsonConvert.DeserializeObject(jsObj["tasks"].ToString());
                            foreach (var task in tasksArray)
                            {
                                if (task["status"].Value<string>() != "cancel")
                                {
                                    TaskItem taskItem = new TaskItem()
                                    {
                                        Priority = Convert.Pri(task["pri"].Value<string>())
                                        ,
                                        ID = task["id"].Value<int>()
                                        ,
                                        Title = Util.EscapeXmlTag(task["name"].Value<string>())
                                        ,
                                        Deadline = task["deadline"].Value<string>()
                                        ,
                                        Tip = "Task"
                                         ,
                                        Type = Convert.Type(task["type"].Value<string>())
                                         ,
                                        Status = Convert.Status(task["status"].Value<string>())
                                         ,
                                        Progress = task["progress"].Value<string>()
                                        ,
                                        ProjectName= task["executionName"].Value<string>()
                                    };

                                    if (!ItemCollectionBackup.Contains(taskItem.ID))
                                    {
                                        NewItemCount = NewItemCount == 0 ? taskItem.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                    }

                                    itemsList.Add(taskItem);
                                }
                            }

                            if (OnNewItemArrive != null
                                && NewItemCount != 0)
                            {
                                OnNewItemArrive(ItemType.Task, NewItemCount);
                            }
                        }

                        isSuccess = true;

                        ItemCollectionBackup.Clear();
                    }
                }
            }
            catch (Exception exp)
            {
                logger.Error(string.Format("GetTask Error:{0}", exp.ToString()));
            }

            return isSuccess;
        }
        #region ActionBaseInterface Members

        public event NewItemArrive OnNewItemArrive;

        #endregion
    }
}
