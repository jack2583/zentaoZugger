﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace ZuggerWpf
{
    [Serializable]
    public class ApplicationConfig
    {
        /// <summary>
        /// 查询bug数间隔，分钟
        /// </summary>
        int requestInterval = 5;
        public int RequestInterval
        {
            get
            {
                if (requestInterval > 60 || requestInterval < 1)
                {
                    requestInterval = 5;
                }

                return requestInterval;
            }
            set { requestInterval = value; }
        }

        //string pmsHost = "http://10.53.132.36:88";
        string pmsHost = string.Empty; 

        public string PMSHost
        {
            get { return pmsHost; }
            set { pmsHost = value; }
        }

        /*
         * API 接口，参照禅道后台-二次开发-API 查看
            分为两种访问方式:
            PATH_INFO方式（伪静态的访问方式）
            1、访问 http://x.com/api-getsessionid.json 获取禅道session信息
            2、使用上一步获取的session作为url参数登录禅道 http://x.com/user-login.json?account=admin&password=123456&zentaosid=2bc87e66e25d8e9b0939aff1756f4132
            3、使用 API 功能操作 http://x.com/project-index-no.json?zentaosid=6v9bl9hp3o5chvdd46u5fg36g4

            GET方式
            1、http://127.0.0.1/zentao/index.php?m=api&f=getSessionID&t=json
            2、http://127.0.0.1/zentao/index.php?m=user&f=login&t=json&account=admin&password=123456&zentaosid=k3g0h321bieq07a5ffh4i3q4h2
            3、http://127.0.0.1/zentao/index.php?m=company&f=browse&t=json&zentaosid=k3g0h321bieq07a5ffh4i3q4h2

            获取的权限与sessionid有关
         */
        /// <summary>
        /// 获得session地址 1、http://127.0.0.1/zentao/index.php?m=api&f=getSessionID&t=json
        /// </summary>
        public string SessionUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "api-getsessionid.json?a=1" : "?m=api&f=getSessionID&t=json");
            }
        }

        /// <summary>
        /// 登陆url
        /// </summary>
        public string LoginUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "user-login.json?a=1" : "?m=user&f=login");
            }
        }

        /// <summary>
        /// 取bug的url
        /// </summary>
        public string GetBugUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "my-work-bug.json?a=1" : "?m=my&f=bug&t=json");
            }
        }

        /// <summary>
        /// 取task
        /// </summary>
        public string GetTaskUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "my-work-task.json?a=1" : "?m=my&f=task&t=json");
            }
        }
        /// <summary>
        /// 取全部执行 http://xd.xiaojingwl.cn:81/zentao/execution-all-undone.html
        /// </summary>
        public string GetExecutionUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "execution-all-undone.json?" : "?m=execution&f=all&t=json");
            }
        }
        /// <summary>
        /// 取unclosedtask 未关闭的全部任务 
        /// </summary>
        public string GetUnclosedTaskUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "execution-task-{0}-unclosed-0-assignedTo_asc.json?a=1" : "?m=execution&f=task&executionID={0}&t=json");
            }
        }
        /// <summary>
        /// 取产品列表
        /// </summary>
        public string GetProductUrl
        {
            get
            {
                //GET  /zentao/product-browse-[productID]-[browseType]-[param]-[orderBy]-[recTotal]-[recPerPage]-[pageID].json
                //return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "product-browse.json?a=1" : "?m=bug&f=browse&browseType=openedByMe&param=0&orderBy=id_desc&recTotal=0&recPerPage=2147483647&pageID=1&t=json");
                //return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "product-browse.json?a=1" : "?m=bug&f=browse&browseType=openedByMe&t=json");
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "product-browse.json?a=1" : "?m=bug&f=browse&browseType=openedByMe&t=json");
            }
        }

        /// <summary>
        /// 取项目列表  
        /// </summary>
        public string GetProjectUrl
        {
            get
            {
                //GET  /zentao/project-browse-[projectID].json
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "project-browse.json?a=1" : "?m=bug&f=browse&browseType=openedByMe&t=json");
            }
        }
        /// <summary>
        /// 取所有项目列表
        /// </summary>
        public string GetAllProjectUrl
        {
            get
            {
                //project-all-all.html
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "project-browse-0-doing.json?a=1" : "?m=project&f=browse&t=json");
            }
        }

        /// <summary>
        /// 取所有ToDo列表  
        /// </summary>
        public string GetAllToDoUrl
        {
            get
            {
                //my-todo-all
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "my-todo.json?a=1" : "?m=bug&f=browse&browseType=openedByMe&t=json");
            }
        }
        /// <summary>
        /// 取由我创建的bug
        /// </summary>
        public string GetOpenedByMeBugUrl
        {
            get
            {
                //GET  /zentao/bug-browse-[productID]-[branch]-[browseType]-[param]-[orderBy]-[recTotal]-[recPerPage]-[pageID].json
                //GET  /zentao/bug-browse-[产品id]-[分支固定为0]-[视图类型unclosed或openedByMe]-[param]-[orderBy]-[recTotal]-[recPerPage]-[pageID].json
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "bug-browse-{0}-0-openedByMe-0-severity_asc.json?a=1"
                    : "?m=bug&f=browse&productID={0}&browseType=openedByMe&param=0&orderBy=id_desc&recTotal=0&recPerPage=2147483647&pageID=1&t=json");
            }          
        }
        /// <summary>
        /// 取未关闭的bug
        /// </summary>
        public string GetUnclosedBugUrl
        {
            get
            {
                //GET  /zentao/bug-browse-[productID]-[branch]-[browseType]-[param]-[orderBy]-[recTotal]-[recPerPage]-[pageID].json
                //GET  /zentao/bug-browse-[产品id]-[分支固定为0]-[视图类型unclosed或openedByMe]-[param]-[orderBy]-[recTotal]-[recPerPage]-[pageID].json
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "bug-browse-{0}-all-unclosed-0-severity_asc.json?a=1"
                    : "?m=bug&f=browse&productID={0}&browseType=unclosed&param=0&orderBy=id_desc&recTotal=0&recPerPage=2147483647&pageID=1&t=json");
            }
        }
        /// <summary>
        /// 取story 即 需求
        /// </summary>
        public string GetStoryUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "my-work-story.json?a=1" : "?m=my&f=story&t=json");
            }
        }
        /// <summary>
        /// 取unclosedstory 即 未关闭的全部需求
        /// </summary>
        public string GetUnclosedStoryUrl
        {
            get
            {
                // GET  /zentao/product-browse-[productID]-[browseType]-    [param]-[orderBy]-[recTotal]-[recPerPage]-[pageID].json
                //GET  /zentao/bug-browse-[productID]-[branch]-[browseType]-[param]-[orderBy]-[recTotal]-[recPerPage]-[pageID].json
                //return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "product-browse-{0}-0-unclosed-id_desc-0-2147483647.json?a=1"
                 //  : "?m=product&f=browse&productID={0}&browseType=unclosed&param=0&orderBy=id_desc&recTotal=0&recPerPage=2147483647&pageID=1&t=json");

                //return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "bug-browse-{0}-0-unclosed-id_desc-0-2147483647.json?a=1"
                //   : "?m=bug&f=browse&productID={0}&browseType=unclosed&param=0&orderBy=id_desc&recTotal=0&recPerPage=2147483647&pageID=1&t=json");
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "execution-story-{0}-story-assignedTo_asc-unclosed-0-10-50.json?a=1" : "?m=execution&f=story&executionID={0}&t=json");
            }
        }
        /// <summary>
        /// 某个需求下的所有bug
        /// </summary>
        public string GetStoryBugsUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "story-bugs-{0}.json?a=1"
                   : "?m=story&f=bugs&storyID={0}&t=json");
            }
        }
        /// <summary>
        /// 某个需求下的所有任务
        /// </summary>
        public string GetStoryTasksUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "story-tasks-{0}.json?a=1"
                   : "?m=story&f=tasks&storyID={0}&t=json");
            }
        }
        /// <summary>
        /// 查看bug的url
        /// </summary>
        public string ViewBugUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "bug-view-{0}.html" : "?m=bug&f=view&bugID={0}");
            }
        }

        /// <summary>
        /// 查看bug明细的url
        /// </summary>
        public string ViewBugDetailUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "bug-view-{0}.json" : "?m=bug&f=view&id={0}&t=json");
            }
        }

        //上传图片附件地址
        //http://10.53.132.36:88/data/upload/1/201105/2114482405248b01.jpg

        /// <summary>
        /// 查看bug的url
        /// </summary>
        public string ViewTaskUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "task-view-{0}.html" : "?m=task&f=view&t=html&id={0}");
            }
        }

        /// <summary>
        /// 查看需求的url
        /// </summary>
        public string ViewStoryUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "execution-storyView-{0}.html" : "?m=story&f=view&storyID={0}");
            }
        }
        /// <summary>
        /// 查看项目的url
        /// </summary>
        public string ViewProjectUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "projectstory-story-{0}.html" : "?m=projectstory&f=story");
            }
        }
        /// <summary>
        /// 查看执行的url
        /// </summary>
        public string ViewExecutionUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "execution-story-{0}-story-order_desc-unclosed.html" : "?m=execution&f=story");
            }
        }
        /// <summary>
        /// 解决bug
        /// </summary>
        public string ResolveBugUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "bug-resolve-{0}.html" : "?m=bug&f=resolve&bugID={0}"); 
            }
        }

        /// <summary>
        /// 编辑bug
        /// </summary>
        public string EditBugUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "bug-edit-{0}.html" : "?m=bug&f=edit&bugID={0}");
            }
        }


        /// <summary>
        /// 激活bug
        /// </summary>
        public string ActiveBugUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "bug-activate-{0}.html" : "?m=bug&f=activate&bugID={0}");
            }
        }

        /// <summary>
        /// 编辑任务
        /// </summary>
        public string EditTaskUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "task-edit-{0}.html" : "?m=task&f=edit&taskid={0}");
            }
        }

        /// <summary>
        /// 编辑需求
        /// </summary>
        public string EditStoryUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, IsPATH_INFORequest ? "story-edit-{0}.html" : "?m=story&f=edit&storyID={0}");
            }
        }

        /// <summary>
        /// 获取是Path_info还是Get访问
        /// </summary>
        //{"version":"1.4","requestType":"PATH_INFO","pathType":"clean","requestFix":"-","moduleVar":"m","methodVar":"f","viewVar":"t","sessionVar":"sid"}        
        public string GetPMSConfigUrl
        {
            get
            {
                return Util.URLCombine(pmsHost, "index.php?mode=getconfig");
            }
        }

        public string BuildPMSConfigUrl(string pmsUrl)
        {
            return Util.URLCombine(pmsUrl, "index.php?mode=getconfig");
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// GET方法还是PATH_INFO访问方式
        /// </summary>
        public bool IsPATH_INFORequest { get; set; }

        private bool isTaskNotify = false;
        /// <summary>
        /// 是否显示任务栏消息通知
        /// </summary>
        public bool IsTaskNotify
        {
            get { return isTaskNotify; }
            set { isTaskNotify = value; }
        }

        private bool isAutoRun = true;
        /// <summary>
        /// 是否随机启动
        /// </summary>
        public bool IsAutoRun
        {
            get { return isAutoRun; }
            set { isAutoRun = value; }
        }

        private bool showOpendByMe = true;
        /// <summary>
        ///是否显示openedbyme
        /// </summary>
        public bool ShowOpendByMe
        {
            get { return showOpendByMe; }
            set { showOpendByMe = value; }
        }
        private bool showUnclosedBug = false;
        /// <summary>
        ///是否显示全部未关闭的BUG
        /// </summary>
        public bool ShowUnclosedBug
        {
            get { return showUnclosedBug; }
            set { showUnclosedBug = value; }
        }
        private bool showBug = true;
        /// <summary>
        ///是否显示bug
        /// </summary>
        public bool ShowBug
        {
            get { return showBug; }
            set { showBug = value; }
        }

        private bool showTask = true;
        /// <summary>
        ///是否显示task
        /// </summary>
        public bool ShowTask
        {
            get { return showTask; }
            set { showTask = value; }
        }
        private bool showUnclosedTask = false;
        /// <summary>
        ///是否显示未关闭的全部task
        /// </summary>
        public bool ShowUnclosedTask
        {
            get { return showUnclosedTask; }
            set { showUnclosedTask = value; }
        }
        private bool showStory = true;
        /// <summary>
        ///是否显示需求
        /// </summary>
        public bool ShowStory
        {
            get { return showStory; }
            set { showStory = value; }
        }
        private bool showUnclosedStory = false;
        /// <summary>
        ///是否显示全部未关闭的需求
        /// </summary>
        public bool ShowUnclosedStory
        {
            get { return showUnclosedStory; }
            set { showUnclosedStory = value; }
        }
        private bool showUnclosedProject = false;
        /// <summary>
        ///是否显示全部未关闭的项目
        /// </summary>
        public bool ShowUnclosedProject
        {
            get { return showUnclosedProject; }
            set { showUnclosedProject = value; }
        }
        private bool showUndoneExecution = true;
        /// <summary>
        ///是否显示全部未关闭的执行
        /// </summary>
        public bool ShowUndoneExecution
        {
            get { return showUndoneExecution; }
            set { showUndoneExecution = value; }
        }
        private bool showUnclosedToDo = false;
        /// <summary>
        ///是否显示全部未关闭的待办
        /// </summary>
        public bool ShowUnclosedToDo
        {
            get { return showUnclosedToDo; }
            set { showUnclosedToDo = value; }
        }
        public bool IsConfigUpdated
        {
            get;
            set;
        }

    }
}
