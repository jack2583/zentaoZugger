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
                            JArray todosArray = (JArray)JsonConvert.DeserializeObject(jsObj["todos"].ToString());

                            foreach (var todo in todosArray)
                            {
                                //显示未关闭
                                if (todo["status"].Value<string>() != "closed" && todo["status"].Value<string>() != "done")
                                {
                                    ToDoItem todoItem = new ToDoItem()
                                    {
                                        Priority = Convert.Pri(todo["pri"].Value<string>())
                                            ,
                                        ID = todo["id"].Value<int>()
                                            ,
                                        Title = Util.EscapeXmlTag(todo["name"].Value<string>())
                                            ,
                                        Tip = "Project"
                                            ,
                                        Type = Convert.Status(todo["type"].Value<string>())
                                            ,
                                        Status = Convert.Status(todo["status"].Value<string>())
                                    };

                                    if (!ItemCollectionBackup.Contains(todoItem.ID))
                                    {
                                        NewItemCount = NewItemCount == 0 ? todoItem.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                    }

                                    itemsList.Add(todoItem);
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
