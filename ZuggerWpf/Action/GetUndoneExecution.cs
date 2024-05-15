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
    class GetUndoneExecution : ActionBase, IActionBase
    {


        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        ZuggerObservableCollection<ExecutionItem> itemsList = null;
        public GetUndoneExecution(ZuggerObservableCollection<ExecutionItem> zItems)
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

                string json = WebTools.Download(string.Format("{0}&{1}={2}", appconfig.GetExecutionUrl, SessionName, SessionID));

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

                        if (jsObj["executionStats"] != null)
                        {
                            //获取所属项目字典
                            //Dictionary<string, string> projectsDic = new Dictionary<string, string>();
                            //var jsObjProjects = JsonConvert.DeserializeObject(jsObj["projects"].ToString()) as JObject;

                            //JToken recordUser = jsObjProjects as JToken;
                            //if (recordUser != null)
                            //{
                            //    foreach (JProperty jp in recordUser)
                            //    {
                            //        projectsDic.Add(jp.Name, jp.Value.ToString());
                            //    }
                            //}

                            JArray BugsArray = (JArray)JsonConvert.DeserializeObject(jsObj["executionStats"].ToString());
                            foreach (var ExecutionJp in BugsArray)
                            {
                                if (ExecutionJp["status"].Value<string>() != "closed")// && j["status"].Value<string>() != "resolved"
                                {
                                    ExecutionItem executionItem = new ExecutionItem()
                                    {
                                        Project = Convert.Pri(ExecutionJp["projectName"].Value<string>())
                                            ,
                                        ID = ExecutionJp["id"].Value<int>()
                                            ,
                                        Title = Util.EscapeXmlTag(ExecutionJp["team"].Value<string>())
                                            ,
                                        Tip = "Execution"
                                            ,
                                        Status = Convert.Status(ExecutionJp["status"].Value<string>())
                                         ,
                                        Begin = ExecutionJp["begin"].Value<string>()
                                         ,
                                        End = ExecutionJp["end"].Value<string>()
                                         ,
                                        Estimate = ExecutionJp["estimate"].Value<string>()
                                         ,
                                        Consumed = ExecutionJp["consumed"].Value<string>()
                                         ,
                                        Progress = ExecutionJp["progress"].Value<string>()+"%"

                                    };

                                    if (!ItemCollectionBackup.Contains(executionItem.ID))
                                    {
                                        NewItemCount = NewItemCount == 0 ? executionItem.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                    }

                                    itemsList.Add(executionItem);
                                }
                            }
                        }
                        isSuccess = true;
                    }
                }
            }
            catch (Exception exp)
            {
                logger.Error(string.Format("GetExecutionId Error: {0}", exp.ToString()));
            }

            return isSuccess;
        }

        #region ActionBaseInterface Members

        public event NewItemArrive OnNewItemArrive;

        #endregion
    }
}
