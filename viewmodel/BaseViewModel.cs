using LTCingFW.utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LTCingFW
{
    public abstract class BaseViewModel : OrmBaseModel, INotifyPropertyChanged
    {
        public Dictionary<String, ValidResult> ValidResultDic = new Dictionary<string, ValidResult>();
        //双向绑定
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return;
            //单项验证
            if (value != null)
            {
                ValidResult result = ValidationFunc.Valid(this, propertyName, value.ToString());
                if (ValidResultDic.ContainsKey(propertyName))
                {
                    ValidResultDic[propertyName] = result;
                }
                else
                {
                    ValidResultDic.Add(propertyName, result);
                }
            }
            storage = value;
            this.OnPropertyChanged(propertyName);
        }

        //分页使用
        public DataTable TablePage
        {
            get
            {
                return tablePage;
            }
        }
        /// <summary>
        /// 当前页数，默认为1
        /// </summary>
        public override int CurrentPage
        {
            get { return current_page; }
            set { SetProperty(ref current_page, value); }
        }
        /// <summary>
        /// 每页数据条目数,默认十条
        /// </summary>
        public override int PageItemCount
        {
            get { return page_item_count; }
            set { SetProperty(ref page_item_count, value); }
        }
        /// <summary>
        /// 总页数 = （总数+每页数量-1）/每页数量
        /// </summary>
        public int TotalPageCount
        {
            get
            {
                return total_page_count;
            }
            set
            {
                SetProperty(ref total_page_count, value);
            }
        }
        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalItemCount
        {
            get { return total_item_count; }
            set
            {

                SetProperty(ref total_item_count, value);
                //此处要将TotalPageCount计算好
                TotalPageCount = (total_item_count + page_item_count - 1) / page_item_count;
            }
        }
        //顺序字典
        public Dictionary<String, String> OrderDic { get; } = new Dictionary<string, string>();
        public abstract void Clear();
    }
}
