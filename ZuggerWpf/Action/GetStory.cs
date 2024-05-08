using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using log4net;
using System.CodeDom.Compiler;

namespace ZuggerWpf
{
    class GetStory : ActionBase, IActionBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        ZuggerObservableCollection<StoryItem> itemsList = null;

        public GetStory(ZuggerObservableCollection<StoryItem> zItems)
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

                string json = WebTools.Download(string.Format("{0}&{1}={2}", appconfig.GetStoryUrl, SessionName, SessionID));

                if (!string.IsNullOrEmpty(json) && IsNewJson(json))
                {
                    ItemCollectionBackup.AddRange(itemsList.Select(f => f.ID));
                    itemsList.Clear();

                    var jsObj = JsonConvert.DeserializeObject(json) as JObject;

                    if (jsObj != null && jsObj["status"].Value<string>() == "success")
                    {
                        json = jsObj["data"].Value<string>();

                        jsObj = JsonConvert.DeserializeObject(json) as JObject;

                        if (jsObj["stories"] != null)
                        {
	                        jsObj = JsonConvert.DeserializeObject(jsObj["stories"].ToString()) as JObject;

                            JToken record = jsObj as JToken;
                            foreach (JProperty jp in record)
                            {
                                var jpFirst = jp.First;
                                if (jpFirst["status"].Value<string>() != "cancel")
                                {
                                    StoryItem stroryItem = new StoryItem()
                                    {
                                        Priority = Convert.Pri(jpFirst["pri"].Value<string>())
                                        ,
                                        ID = jpFirst["id"].Value<int>()
                                        ,
                                        Title = jpFirst["title"].Value<string>()
                                        ,
                                        OpenDate = jpFirst["openedDate"].Value<string>()
                                        ,
                                        Stage = Convert.Stage(jpFirst["stage"].Value<string>())
                                        ,
                                        Execution= jpFirst["productTitle"].Value<string>()
                                    };

                                    if (!ItemCollectionBackup.Contains(stroryItem.ID))
                                    {
                                        NewItemCount = NewItemCount == 0 ? stroryItem.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                    }

                                    itemsList.Add(stroryItem);
                                }
                            }


                            if (OnNewItemArrive != null
                                && NewItemCount != 0)
                            {
                                OnNewItemArrive(ItemType.Story, NewItemCount);
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
