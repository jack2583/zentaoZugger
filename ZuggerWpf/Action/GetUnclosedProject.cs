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
    class GetUnclosedProject : ActionBase, IActionBase
    {


        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        ZuggerObservableCollection<ProjectItem> itemsList = null;
        public GetUnclosedProject(ZuggerObservableCollection<ProjectItem> zItems)
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

                string json = WebTools.Download(string.Format("{0}&{1}={2}", appconfig.GetAllProjectUrl, SessionName, SessionID));

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

                        if (jsObj["projectStats"] != null)
                        {
                            JArray jsArray = (JArray)JsonConvert.DeserializeObject(jsObj["projectStats"].ToString());

                            foreach (var j in jsArray)
                            {
                                //显示未关闭
                                if (j["status"].Value<string>() != "closed" && j["status"].Value<string>() != "resolved")
                                {
                                    ProjectItem pi = new ProjectItem()
                                    {
                                        Priority = Convert.Pri(j["pri"].Value<string>())
                                            ,
                                        ID = j["id"].Value<int>()
                                            ,
                                        Title = Util.EscapeXmlTag(j["name"].Value<string>())
                                            ,
                                        Tip = "Project"
                                            ,
                                        Status = Convert.Status(j["status"].Value<string>())
                                            ,
                                        Progress =j["hours"]["progress"].Value<string>() + "%"
                                    };

                                    if (!ItemCollectionBackup.Contains(pi.ID))
                                    {
                                        NewItemCount = NewItemCount == 0 ? pi.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                    }

                                    itemsList.Add(pi);
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
