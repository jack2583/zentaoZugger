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
                            JArray jsArray = (JArray)JsonConvert.DeserializeObject(jsObj["stories"].ToString());

                            foreach (var j in jsArray)
                            {
                                StoryItem bi = new StoryItem()
                                {
                                    Priority = Convert.Pri(j["pri"].Value<string>())
                                    ,
                                    ID = j["id"].Value<int>()
                                    ,
                                    Title = Util.EscapeXmlTag(j["title"].Value<string>())
                                    ,
                                    OpenDate = j["openedDate"].Value<string>()
                                    ,
                                    Stage = Convert.Stage(j["stage"].Value<string>())
                                    ,
                                    Tip = "需求"
                                };

                                if (!ItemCollectionBackup.Contains(bi.ID))
                                {
                                    NewItemCount = NewItemCount == 0 ? bi.ID : (NewItemCount > 0 ? -2 : NewItemCount - 1);
                                }

                                itemsList.Add(bi);                                
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
