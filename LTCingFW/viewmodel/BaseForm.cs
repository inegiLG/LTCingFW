using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LTCingFW.utils;

namespace LTCingFW
{
    public class BaseForm:Form
    {
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // BaseForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "BaseForm";
            this.ResumeLayout(false);

        }
        //父窗口
        private Form fatherForm;
        public Form FatherForm
        {
            get
            {
                return fatherForm;
            }
        }
        //Controller
        public object Controller { get; set; }

        //查詢使用的Model
        public BaseViewModel QueryModel { get; set; }
        //页面使用的Model
        public BaseViewModel Model { get; set; }
        //控制器名
        public string ControllerName { get; set; }

        #region 构造函数
        public BaseForm() { }
        public BaseForm(Form father) {
            fatherForm = father;
        }
        public BaseForm(Form father,BaseViewModel model)
        {
            fatherForm = father;
            Model = model;
        }
        #endregion

        #region 分页
        /// <summary>
        /// 跳转、查询
        /// </summary>
        public void go_page_btn_Click()
        {
            SelectPage();
        }
        /// <summary>
        /// 首页
        /// </summary>
        public void first_page_btn_Click()
        {
            Model.CurrentPage = 1;
            SelectPage();
        }
        /// <summary>
        /// 上一页
        /// </summary>
        public void previous_page_btn_Click()
        {
            if (Model.CurrentPage > 1)
            {
                Model.CurrentPage = Model.CurrentPage - 1;
            }
            SelectPage();
        }
        /// <summary>
        /// 下一页
        /// </summary>
        public void next_page_btn_Click()
        {
            if (Model.CurrentPage < Model.TotalPageCount)
            {
                Model.CurrentPage = Model.CurrentPage + 1;
            }
            SelectPage();
        }
        /// <summary>
        /// 尾页
        /// </summary>
        public void lastest_page_btn_Click()
        {
            Model.CurrentPage = Model.TotalPageCount;
            SelectPage();
        }
        /// <summary>
        /// 分页查询
        /// </summary>
        public  void SelectPage()
        {
            object kepc = LTCingFWSet.GetInstanceBean(ControllerName);
            MethodInfo info = kepc.GetType().GetMethod("SelectPage",BindingFlags.Public|BindingFlags.Instance);
            DataTable dt_data = (DataTable)info.Invoke(kepc,null);
            Model.TablePage.Rows.Clear();
            FwUtilFunc.TransferDataTable(dt_data, Model.TablePage);
            if (dt_data != null && dt_data.Columns.Contains("EXCUTE_RESULT") )
            {
                foreach (DataRow row in Model.TablePage.Rows)
                {
                    if (row["EXCUTE_RESULT"].ToString() == "0")
                    {
                        row["EXCUTE_RESULT"] = "成功";
                    }
                    else
                    {
                        row["EXCUTE_RESULT"] = "失败";
                    }
                }
            }
            
        }
        #endregion

        #region 函数
        //获取控件
        public object GetControlByName(String name) {
            FieldInfo info = this.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance);
            return info.GetValue(this);
        }
        #endregion

        public object ExecControllerMethod(String ControllerName ,String MethodName,object[] parms)
        {
            object kepc = LTCingFWSet.GetInstanceBean(ControllerName);
            MethodInfo info = kepc.GetType().GetMethod(MethodName, BindingFlags.Public | BindingFlags.Instance);
            object dt_data = info.Invoke(kepc, parms);
            return dt_data;
        }


    }
}
