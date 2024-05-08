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
                            jsObj = JsonConvert.DeserializeObject(jsObj["projectStats"].ToString()) as JObject;

                            JToken record = jsObj as JToken;
                            foreach (JProperty jp in record)
                            {
                                var ProjectJp = jp.First;
                                if (ProjectJp["status"].Value<string>() != "cancel")
                                {
                                    ProjectItem projectItem = new ProjectItem()
                                    {
                                        Priority = Convert.Pri(ProjectJp["pri"].Value<string>())
                                            ,
                                        ID = ProjectJp["id"].Value<int>()
                                            ,
                                        Title = Util.EscapeXmlTag(ProjectJp["name"].Value<string>())
                                            ,
                                        Tip = "Project"
                                            ,
                                        Status = Convert.Status(ProjectJp["status"].Value<string>())
                                          
                                    };

                                    if (!ItemCollectionBackup.Contains(projectItem.ID))
                                    {
                                        NewItemCount = NewItemCount == 0 ? projectItem.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                    }

                                    itemsList.Add(projectItem);
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
