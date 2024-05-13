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
    class BugItem : ItemBase
    {
        #region ItemBase 成员

        public int ID { get; set; }

        public string Title { get; set; }

        #endregion

        public string Priority { get; set; }

        public string Severity { get; set; }

        public string OpenDate { get; set; }
        public string AssignedToName { get; set; }

        public string LastEditDate { get; set; }

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

        public string Product { get; set; }
        public string Status { get; set; }
        public int ColorInt
        {
            get
            {
                if (Status == "激活")
                {
                    if (Severity == "优化" || Severity == "轻微" || Severity == "普通")
                    {
                        if (Confirmed == "未确认")
                        {
                            return 0;//蓝
                        }
                        else
                        {
                            return 1; //灰

                        }
                    }
                    else
                    {
                        if (Confirmed == "未确认")
                        {
                            return 2;//橙
                        }
                        else
                        {
                            return 3; //红

                        }
                    }
                }
                else
                {
                    if (Resolution == "已解决")
                    {
                        return 4; //绿；
                    }
                    else
                    {
                        return 5; //深绿色
                    }
                }
            }
        }
    }
    #endregion

    #region Task 任务
    class TaskItem : ItemBase
    {
        #region ItemBase 成员

        public int ID { get; set; }

        public string Title { get; set; }

        #endregion

        public string Priority { get; set; }

        public string Deadline { get; set; }
        public string AssignedToName { get; set; }
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
        private string status;
        public string Status 
        {
            get { 
                return status+"("+ Progress +"%)"; 
            }
            set { status = value; }
        }
        /// <summary>
        /// 项目进度百分比值
        /// </summary>
        public string Progress { get; set; }

        public string ProjectName { get; set; }
       // public float? Estimate { get; set; } // 最初预计工时
       //// public DateTime? Deadline { get; set; } // 任务截止日期
       // public DateTime? EstStarted { get; set; } // 预计开始日期
       // public DateTime? RealStarted { get; set; } // 实际开始日期
       // public int PlanDuration { get; set; } // 计划持续天数
       // public int RealDuration { get; set; } // 实际持续天数
       // public DateTime OpenedDate { get; set; } // 创建日期
    }

    #endregion

    #region Story 需求
    class StoryItem : ItemBase
    {
        #region ItemBase 成员

        public int ID { get; set; }

        public string Title { get; set; }

        #endregion
        public string Priority { get; set; }

        public string OpenDate { get; set; }
        public string AssignedToName { get; set; }

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
        /// <summary>
        /// 所属执行
        /// </summary>
        public string Execution { get; set; }
    }
    #endregion
    #region Project 项目
    class ProjectItem : ItemBase
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
    class ToDoItem : ItemBase
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
