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
    class GetUnclosedToDo : ActionBase, IActionBase
    {
        ZuggerObservableCollection<ToDoItem> itemsList = null;

        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public GetUnclosedToDo(ZuggerObservableCollection<ToDoItem> zItems)
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

                string json = WebTools.Download(string.Format("{0}&{1}={2}", appconfig.GetAllToDoUrl, SessionName, SessionID));
                bool isNewJson = IsNewJson(string.Concat(json));

                if (!isNewJson)
                {
                    return true;
                }

                ItemCollectionBackup.AddRange(itemsList.Select(f => f.ID));
                itemsList.Clear();
                if (!string.IsNullOrEmpty(json))
                {
                    var jsObj = JsonConvert.DeserializeObject(json) as JObject;

                    if (jsObj != null && jsObj["status"].Value<string>() == "success")
                    {
                        json = jsObj["data"].Value<string>();

                        jsObj = JsonConvert.DeserializeObject(json) as JObject;

                        if (jsObj["todos"] != null)
                        {
                            JArray jsArray = (JArray)JsonConvert.DeserializeObject(jsObj["todos"].ToString());

                            foreach (var j in jsArray)
                            {
                                //显示未关闭
                                if (j["status"].Value<string>() != "closed" && j["status"].Value<string>() != "done")
                                {
                                    ToDoItem ti = new ToDoItem()
                                    {
                                        Priority = Convert.Pri(j["pri"].Value<string>())
                                            ,
                                        ID = j["id"].Value<int>()
                                            ,
                                        Title = Util.EscapeXmlTag(j["name"].Value<string>())
                                            ,
                                        Tip = "Project"
                                            ,
                                        Type = Convert.Status(j["type"].Value<string>())
                                            ,
                                        Status = Convert.Status(j["status"].Value<string>())
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
            catch (Exception exp)
            {
                logger.Error(string.Format("GetProjectId Error: {0}", exp.ToString()));
            }

            return isSuccess;
        }

        #region ActionBaseInterface Members

        public event NewItemArrive OnNewItemArrive;

        #endregion
    }
}
