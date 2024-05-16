using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using System.Web.UI;

namespace ZuggerWpf
{
    public class Count2Background : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            int num = int.Parse(value.ToString());


            if (num <= 5)
            {

                //Color.FromArgb(透明度, R, G, B)
                return new SolidColorBrush(Color.FromArgb(255, 255, 204, 204)); 
            }
            if (num <= 10)
            {
                return new SolidColorBrush(Color.FromArgb(255, 255, 170, 170));
            }
            if (num <= 15)
            {
                return new SolidColorBrush(Color.FromArgb(255, 255, 102, 102));
            }
            if (num <= 20)
            {
                return new SolidColorBrush(Color.FromArgb(255, 255, 68, 68));
            }
            else
            {
                 return Brushes.Red;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class Date2Color : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            DateTime date;

            if (!DateTime.TryParse(value.ToString(), out date))
            {
                date = DateTime.Now;
            }

            int days = (DateTime.Now - date).Days;

            if (days <= 7)
            {
                return Brushes.Brown;
            }
            //else if (days > 3 && days < 7)
            //{
            //    return Brushes.Orange;
            //}
            else
            {
                return Brushes.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class TaskProgress2Color : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            int intProgress=int.Parse(value.ToString());

            if (intProgress==0)
            {
                return new SolidColorBrush(Color.FromArgb(255, 0, 82, 204)); // 蓝色
            }
            else if(intProgress <100)
            {
                return new SolidColorBrush(Color.FromArgb(255, 255, 87, 51)); // 红色
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(255, 40, 167, 69)); // 绿色
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    /// <summary>
    /// BUG字体颜色
    /// </summary>
    public class Bug2Color : IValueConverter
    {
        // 在转换器类中
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 假设您要根据 LastEdit 属性的值来返回不同的背景色
            switch (int.Parse(value.ToString()))
            {
                case 0:
                    return Brushes.Black; // 默认黑色
                    return new SolidColorBrush(Color.FromArgb(255, 0, 169, 242)); // 蓝色
                case 1:
                    return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)); // 黑色

                case 2:
                    return new SolidColorBrush(Color.FromArgb(255, 245, 154, 35)); // 橙色

                case 3:
                    return new SolidColorBrush(Color.FromArgb(255, 255, 0, 135)); // 红色

                case 4:
                    return new SolidColorBrush(Color.FromArgb(255, 0, 164, 0)); // 绿色

                case 5:
                    return new SolidColorBrush(Color.FromArgb(255, 217, 0, 0)); // 红色
                default:
                    // 默认情况下，可以选择返回一个默认颜色，或者保持 break 语句不做任何返回
                    break;
            }

            return Brushes.Black; // 默认黑色
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// BUG确认状态颜色
    /// </summary>
    public class BugConfirmed2Color : IValueConverter
    {
        // 在转换器类中
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 假设您要根据 LastEdit 属性的值来返回不同的背景色
            switch (value.ToString())
            {
                case "未确认":
                    return new SolidColorBrush(Color.FromArgb(255, 255, 87, 51)); // 红色
             
                default:
                    return new SolidColorBrush(Color.FromArgb(255, 0, 164, 0)); // 绿色
                    break;
            }

            return Brushes.Black; // 默认黑色
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 执行状态颜色
    /// </summary>
    public class ExecutionProgress2Color : IValueConverter
    {
        // 在转换器类中
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 假设您要根据 LastEdit 属性的值来返回不同的背景色
            switch (value.ToString())
            {
                case "0":
                    return new SolidColorBrush(Color.FromArgb(255, 0, 82, 204)); // 蓝色

                default:
                    return new SolidColorBrush(Color.FromArgb(255, 255, 87, 51)); // 红色
                    break;
            }

            return Brushes.Black; // 默认黑色
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 根据需求状态改颜色
    /// </summary>
    public class StoryStage2Color : IValueConverter
    {
        // 在转换器类中
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 假设您要根据 LastEdit 属性的值来返回不同的背景色
            switch (value.ToString().ToLower().Trim())
            {
                case "未开始":
                    return new SolidColorBrush(Color.FromArgb(255, 45, 45, 45)); // 深灰色

                case "已计划":
                    return new SolidColorBrush(Color.FromArgb(255, 0, 123, 255)); // 浅蓝色

                case "已立项":
                    return new SolidColorBrush(Color.FromArgb(255, 0, 82, 204)); // 蓝色

                case "研发中":
                    return new SolidColorBrush(Color.FromArgb(255, 255, 87, 51)); // 红色

                case "研发完毕":
                    return new SolidColorBrush(Color.FromArgb(255, 40, 167, 69)); // 绿色

                case "测试中":
                    return new SolidColorBrush(Color.FromArgb(255, 255, 159, 67)); // 橙色

                case "测试完毕":
                    return new SolidColorBrush(Color.FromArgb(255, 105, 166, 123)); // 浅绿色

                case "已验收":
                    return new SolidColorBrush(Color.FromArgb(255, 32, 96, 60)); // 深绿色

                case "已发布":
                    return new SolidColorBrush(Color.FromArgb(255, 73, 80, 87)); // 深灰色

                default:
                    // 默认情况下，可以选择返回一个默认颜色，或者保持 break 语句不做任何返回
                    break;
            }

            return Brushes.Black; // 默认黑色
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class LastEditToBackgroundColorConverter : IValueConverter
    {
        // 在转换器类中
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 假设您要根据 LastEdit 属性的值来返回不同的背景色
            if (value is DateTime lastEdit)
            {
                // 这里可以根据您的逻辑来决定背景色
                // 举例来说，如果 LastEdit 是在一周内，背景色为黄色；否则为白色
                if (lastEdit > DateTime.Now.AddDays(-7))
                {
                    return new SolidColorBrush(Colors.Yellow);
                }
                else
                {
                    return new SolidColorBrush(Colors.White);
                }
            }
            return Brushes.Transparent; // 默认背景色
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Datasource2Visible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class Bool2Visible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            bool v;
            if (bool.TryParse(value.ToString(), out v))
            {
                return !v;
            }

            return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            bool v;

            if(bool.TryParse(value.ToString(), out v))
            {
                return v;
            }

            return v;
        }
    }

    public class Count2Visible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            int v;
            if (int.TryParse(value.ToString(), out v))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class HeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            double maxHeight;

            if (double.TryParse(value.ToString(), out maxHeight))
            {
                double para;
                if (parameter != null && double.TryParse(parameter.ToString(), out para))
                {
                    return maxHeight * para;
                }
            }

            return maxHeight;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class MinuteRule : ValidationRule
    {
        private int _min;
        private int _max;

        public MinuteRule()
        {
        }

        public int Min
        {
            get { return _min; }
            set { _min = value; }
        }

        public int Max
        {
            get { return _max; }
            set { _max = value; }
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int age = 0;

            try
            {
                if (((string)value).Length > 0)
                    age = Int32.Parse(value.ToString());
            }
            catch
            {
                return new ValidationResult(false, "请输入数字");
            }

            if ((age < Min) || (age > Max))
            {
                return new ValidationResult(false,
                  "请输入正确的分钟数，范围: " + Min + " - " + Max + "");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
