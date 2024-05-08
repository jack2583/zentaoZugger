using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using log4net;

namespace ZuggerWpf
{
    class GetBug : ActionBase, IActionBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ZuggerObservableCollection<BugItem> itemsList = null;

        public GetBug(ZuggerObservableCollection<BugItem> zItems)
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
                string strtemp = string.Format("{0}&{1}={2}", appconfig.GetBugUrl, SessionName, SessionID);
                string json = WebTools.Download(string.Format("{0}&{1}={2}", appconfig.GetBugUrl, SessionName, SessionID));

                if (!string.IsNullOrEmpty(json) && IsNewJson(json))
                {
                    ItemCollectionBackup.AddRange(itemsList.Select(f => f.ID));
                    itemsList.Clear();

                    var jsObj = JsonConvert.DeserializeObject(json) as JObject;

                    if (jsObj != null && jsObj["status"].Value<string>() == "success")
                    {
                        json = jsObj["data"].Value<string>();

                        jsObj = JsonConvert.DeserializeObject(json) as JObject;

                        if (jsObj["bugs"] != null)
                        {
                            JArray BugsArray = (JArray)JsonConvert.DeserializeObject(jsObj["bugs"].ToString());

                            foreach (var bug in BugsArray)
                            {
                                if (bug["status"].Value<string>() != "closed")// && j["status"].Value<string>() != "resolved"
                                {
                                    BugItem bugItem = new BugItem()
                                    {
                                        Priority = Convert.Pri(bug["pri"].Value<string>())
                                    ,
                                        Severity = Convert.Severity(bug["severity"].Value<string>())
                                    ,
                                        ID = bug["id"].Value<int>()
                                    ,
                                        Title = Util.EscapeXmlTag(bug["title"].Value<string>())
                                    ,
                                        OpenDate = bug["openedDate"].Value<string>()
                                    ,
                                        LastEdit = bug["lastEditedDate"].Value<string>()
                                    ,
                                        Tip = "Bug"
                                    ,
                                        Confirmed = Convert.Confirmed(bug["confirmed"].Value<string>())
                                    ,
                                        Resolution = Convert.Resolution(bug["resolution"].Value<string>())
                                    ,
                                        Product = bug["productName"].Value<string>()
                                    };

                                    if (!ItemCollectionBackup.Contains(bugItem.ID))
                                    {
                                        NewItemCount = NewItemCount == 0 ? bugItem.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                    }

                                    itemsList.Add(bugItem);
                                }
                            }

                            if (OnNewItemArrive != null
                                && NewItemCount != 0)
                            {
                                OnNewItemArrive(ItemType.Bug, NewItemCount);
                            }
                        }

                        isSuccess = true;

                        ItemCollectionBackup.Clear();
                    }
                }
            }
            catch (Exception exp)
            {
                logger.Error(string.Format("GetBug Error: {0}", exp.ToString()));                
            }

            return isSuccess;
        }
     
        #region ActionBaseInterface Members

        public event NewItemArrive OnNewItemArrive;

        #endregion
    }
}
