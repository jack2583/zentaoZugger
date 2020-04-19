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
                            JArray jsArray = (JArray)JsonConvert.DeserializeObject(jsObj["tasks"].ToString());
                            foreach (var j in jsArray)
                            {
                                if (j["status"].Value<string>() != "done" && j["status"].Value<string>() != "cancel")
                                {
                                    int priID = int.Parse(j["pri"].Value<string>());
                                    TaskItem ti = new TaskItem()
                                    {
                                        Priority = Enum.GetName(typeof(CustomEnum.CustomPri), priID)
                                        ,
                                        ID = j["id"].Value<int>()
                                        ,
                                        Title = Util.EscapeXmlTag(j["name"].Value<string>())
                                        ,
                                        Deadline = j["deadline"].Value<string>()
                                        ,
                                        Tip = "Task"
                                         ,
                                        Type = ConvertType(j["type"].Value<string>())
                                            ,
                                        Status = ConvertType(j["status"].Value<string>())
                                    };

                                    if (!ItemCollectionBackup.Contains(ti.ID))
                                    {
                                        NewItemCount = NewItemCount == 0 ? ti.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                    }

                                    itemsList.Add(ti);
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
