using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Text.RegularExpressions;

namespace ZuggerWpf
{
    /// <summary>
    /// 新Item到达
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="newItemID">0：无新item，负数：多个item，正数：表示ItemID，只有一个</param>
    delegate void NewItemArrive(ItemType type, int newItemID);

    interface ItemBase
    {
        int ID { get; set; }

        string Title { get; set; }

        string Tip { get; set; }        
    }

    class ZuggerObservableCollection<T> : ObservableCollection<T> where T : ItemBase
    {
        protected override void ClearItems()
        {
            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
        }
    }


    #region Bug BUG
    struct BugItem : ItemBase
    {
        #region ItemBase 成员

        public int ID { get; set; }

        public string Title { get; set; }

        #endregion

        public string Priority { get; set; }

        public string Severity { get; set; }

        public string OpenDate { get; set; }

        public string LastEdit { get; set; }

        private string tip;
        public string Tip
        {
            get
            {
                return string.IsNullOrEmpty(tip) ? "Bug" : tip;
            }
            set 
            {
                tip = value;
            }
        }
        /// <summary>
        /// 是否确认
        /// </summary>
        public string Confirmed { get; set; }
        /// <summary>
        /// 解决方案
        /// </summary>
        public string Resolution { get; set; }
    }

    #endregion

    #region Task 任务
    struct TaskItem : ItemBase
    {
        #region ItemBase 成员

        public int ID { get; set; }

        public string Title { get; set; }

        #endregion

        public string Priority { get; set; }

        public string Deadline { get; set; }        

        private string tip;
        public string Tip
        {
            get
            {
                return string.IsNullOrEmpty(tip) ? "Task" : tip;
            }
            set
            {
                tip = value;
            }
        }
        public string Type { get; set; }
        public string Status { get; set; }
        /// <summary>
        /// 项目进度百分比值
        /// </summary>
        public string Progress { get; set; }
    }

    #endregion

    #region Story 需求
    struct StoryItem : ItemBase
    {
        #region ItemBase 成员

        public int ID { get; set; }

        public string Title { get; set; }

        #endregion
        public string Priority { get; set; }

        public string OpenDate { get; set; }

        private string tip;
        public string Tip
        {
            get
            {
                return string.IsNullOrEmpty(tip) ? "Story" : tip;
            }
            set
            {
                tip = value;
            }
        }

        public string Stage { get; set; }
    }
    #endregion
    #region Project 项目
    struct ProjectItem : ItemBase
    {
        #region ItemBase 成员

        public int ID { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        #endregion
        /// <summary>
        /// 优先级 
        /// </summary>
        public string Priority { get; set; }

        public string OpenDate { get; set; }

        private string tip;
        public string Tip
        {
            get
            {
                return string.IsNullOrEmpty(tip) ? "Project" : tip;
            }
            set
            {
                tip = value;
            }
        }

        public string Status { get; set; }
        /// <summary>
        /// 项目进度百分比值
        /// </summary>
        public string Progress { get; set; }


    }
    #endregion
    #region ToDo 待办
    struct ToDoItem : ItemBase
    {
        #region ItemBase 成员

        public int ID { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        #endregion
        /// <summary>
        /// 优先级 
        /// </summary>
        public string Priority { get; set; }

        public string OpenDate { get; set; }

        private string tip;
        public string Tip
        {
            get
            {
                return string.IsNullOrEmpty(tip) ? "ToDo" : tip;
            }
            set
            {
                tip = value;
            }
        }

        public string Type { get; set; }
        public string Status { get; set; }
    }
    #endregion

    interface IActionBase
    {
        /// <summary>
        /// 新item到达通知
        /// </summary>
        event NewItemArrive OnNewItemArrive;
    }

    abstract class ActionBase
    {
        /// <summary>
        /// 向pms发请求并返回是否成功
        /// </summary>
        public abstract bool Action();

        internal static string SessionName;

        internal static string SessionID;

        protected static string RandomNumber;

        protected DateTime LastGetSessionTime;

        /// <summary>
        /// 上次取得字符串的md5码，比较这次的，如果相同说明没有更新，不用走Json序列化
        /// </summary>
        protected string LastMd5;        

        protected int SessionTimeOut = ConfigurationManager.AppSettings["SessionTimeOut"] == null ? 20 : int.Parse(ConfigurationManager.AppSettings["SessionTimeOut"]);

        /// <summary>
        /// 0：无新item，负数：多个新item，正数：表示ItemID，只可能有一个
        /// </summary>
        protected int NewItemCount { get; set; }

        /// <summary>
        /// 取新数据前item列表的备份
        /// </summary>
        protected List<int> ItemCollectionBackup = new List<int>();       

        protected static Regex Md5Reg = new Regex(@"""md5""\s*:\s*""(?<key>[\d\w]+)""");

        protected bool IsNewJson(string json)
        {
            bool isNewJson = true;

            MatchCollection mc = Md5Reg.Matches(json);
            if (mc.Count > 0)
            {
                string nowMd5 = string.Empty;
                foreach (Match m in mc)
                { 
                    nowMd5 = string.Concat(nowMd5, m.Groups["key"].Value);
                }

                if (!string.IsNullOrEmpty(LastMd5) && LastMd5.CompareTo(nowMd5) == 0)
                {
                    isNewJson = false;
                }
                else
                {
                    LastMd5 = nowMd5;
                }                
            }

            return isNewJson;
        }
    }
}
