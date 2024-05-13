using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZuggerWpf
{
   public static class Convert
    {
        /// <summary>
        /// 严重程度
        /// </summary>
        /// <param name="eWord"></param>
        /// <returns></returns>
        public static string Severity(string eWord)
        {
            string cword = string.Empty;

            switch (eWord.ToLower().Trim())
            {
                case "1":
                    cword = "致命";
                    break;
                case "2":
                    cword = "严重";
                    break;
                case "3":
                    cword = "普通";
                    break;
                case "4":
                    cword = "轻微";
                    break;
                case "5":
                    cword = "优化";
                    break;
                default:
                    eWord.ToLower().Trim();
                    break;
            }
            return cword;
        }
        /// <summary>
        /// 优先级别
        /// </summary>
        /// <param name="eWord"></param>
        /// <returns></returns>
        public static string Pri(string eWord)
        {
            string cword = string.Empty;

            switch (eWord.ToLower().Trim())
            {
                case "1":
                    cword = "极";
                    break;
                case "2":
                    cword = "高";
                    break;
                case "3":
                    cword = "中";
                    break;
                case "4":
                    cword = "低";
                    break;
                default:
                    eWord.ToLower().Trim();
                    break;
            }
            return cword;
        }
        /// <summary>
        /// BUG解决方案
        /// </summary>
        /// <param name="eWord"></param>
        /// <returns></returns>
        public static string Resolution(string eWord)
        {
            string cword = string.Empty;

            switch (eWord.ToLower().Trim())
            {
                case "bydesign":
                    cword = "设计如此";
                    break;
                case "duplicate":
                    cword = "重复BUG";
                    break;
                case "external":
                    cword = "外部原因";
                    break;
                case "fixed":
                    cword = "已解决";
                    break;
                case "notrepro":
                    cword = "无法重现";
                    break;
                case "postponed":
                    cword = "延期处理";
                    break;
                case "willnotfix":
                    cword = "不予解决";
                    break;
                case "toshory":
                    cword = "转为需求";
                    break;
                default:
                    eWord.ToLower().Trim();
                    break;
            }

            return cword;
        }
        /// <summary>
        /// BUG是否确认
        /// </summary>
        /// <param name="eWord"></param>
        /// <returns></returns>
        public static string Confirmed(string eWord)
        {
            string cword = string.Empty;

            switch (eWord.ToLower().Trim())
            {
                case "0":
                    cword = "未确认";
                    break;
                case "1":
                    cword = "已确认";
                    break;
                default:
                    eWord.ToLower().Trim();
                    break;
            }
            return cword;
        }

        /// <summary>
        /// 任务状态
        /// </summary>
        /// <param name="eWord"></param>
        /// <returns></returns>
        public static string Status(string eWord)
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
                case "active":
                    cword = "激活";
                    break;
                case "resolved":
                    cword = "已解决";
                    break;
                default:
                    eWord.ToLower().Trim();
                    break;
            }

            return cword;
        }
        /// <summary>
        /// 任务类型
        /// </summary>
        /// <param name="eWord"></param>
        /// <returns></returns>
        public static string Type(string eWord)
        {
            string cword = string.Empty;

            switch (eWord.ToLower().Trim())
            {
                case "design":
                    cword = "写策划案";//设计
                    break;
                case "document":
                    cword = "写文档";
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
                case "config":
                    cword = "配置";
                    break;
                case "numerical":
                    cword = "数值";
                    break;
                case "shuzhi":
                    cword = "数值设定";
                    break;
                case "art":
                    cword = "美术";
                    break;
                case "test":
                    cword = "测试";
                    break;
                case "peibiao":
                    cword = "配表";
                    break;
                case "discuss":
                    cword = "讨论";
                    break;
                case "ui":
                    cword = "UI需求";
                    break;
                case "affair":
                    cword = "事务";
                    break;
                case "testcase":
                    cword = "测试用例";
                    break;
                case "case":
                    cword = "用例";
                    break;
                case "custom":
                    cword = "自定义";
                    break;
                case "cycle":
                    cword = "周期";
                    break;
                case "bug":
                    cword = "Bug";
                    break;
                case "task":
                    cword = "任务";
                    break;
                case "story":
                    cword = "需求";
                    break;
                case "other":
                    cword = "其它";
                    break;
                default:
                    eWord.ToLower().Trim();
                    break;
            }

            return cword;
        }
        /// <summary>
        /// 需求阶段
        /// </summary>
        /// <param name="eWord"></param>
        /// <returns></returns>
        public static string Stage(string eWord)
        {
            string cword = string.Empty;

            switch (eWord.ToLower().Trim())
            {
                case "wait":
                    cword = "未开始";
                    break;
                case "planned":
                    cword = "已计划";
                    break;
                case "projected":
                    cword = "已立项";
                    break;
                case "developing":
                    cword = "研发中";
                    break;
                case "developed":
                    cword = "研发完毕";
                    break;
                case "testing":
                    cword = "测试中";
                    break;
                case "tested":
                    cword = "测试完毕";
                    break;
                case "verified":
                    cword = "已验收";
                    break;
                case "released":
                    cword = "已发布";
                    break;
                default:
                    eWord.ToLower().Trim();
                    break;
            }

            return cword;
        }
    }

}
