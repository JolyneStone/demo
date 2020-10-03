using CodeGenerator.Razor;
using System.Linq;
using System.Text;

namespace CodeGenerator.SqlServer
{
    public abstract class SqlServerRazorPageView : RazorPageViewBase<ModelEntity>
    {
        /// <summary>
        /// 获取主键参数列表
        /// </summary>
        /// <returns></returns>
        protected string GetPrimaryKeyParams()
        {
            if (Model == null)
                return "";
            StringBuilder strParameter = new StringBuilder();
            foreach (var c in Model.Properties)
            {
                if (c.IsKey)
                {
                    strParameter.AppendFormat("{0} {1},", c.Type, GetParameterName(c.Name));
                }
            }
            if (strParameter.ToString().Length > 1)
            {
                return strParameter.ToString().Remove(strParameter.ToString().Length - 1);
            }

            return "";
        }

        public string GetPrimaryKeyParamsValue()
        {
            StringBuilder strParameter = new StringBuilder();
            foreach (var c in Model.Properties)
            {
                if (c.IsKey)
                {
                    strParameter.AppendFormat("{0},", GetParameterName(c.Name));
                }
            }
            if (strParameter.ToString().Length > 1)
            {
                return strParameter.ToString().Remove(strParameter.ToString().Length - 1);
            }

            return "";
        }

        protected ModelProperty[] GetPKColumns()
        {
            return Model.Properties != null ? Model.Properties.Where(d => d.IsKey).ToArray() : new ModelProperty[0];
        }

        protected ModelProperty[] GetNoPKAndIdentityColumns()
        {
            return Model.Properties != null ? Model.Properties.Where(d => !(d.IsKey || d.IsIdentity)).ToArray() : new ModelProperty[0];
        }

        /// <summary>
        /// 表是否有标识主键
        /// </summary>
        /// <returns></returns>
        protected bool HasIdentityPK(out ModelProperty property)
        {
            if (Model.Properties == null)
            {
                property = null;
                return false;
            }
            property = Model.Properties.FirstOrDefault(c => c.IsKey && c.IsIdentity);
            return property != null;
        }

        /// <summary>
        /// 表是否有标识列
        /// </summary>
        /// <returns></returns>
        protected bool HasIdentity(out ModelProperty property)
        {
            if (Model.Properties == null)
            {
                property = null;
                return false;
            }
            property = Model.Properties.Where(c => c.IsIdentity).OrderByDescending(d => d.IsKey).FirstOrDefault();
            return property != null;
        }

        public string GetParameterName(string name)
        {
            return name.ToLowerCamelCase();
        }
    }
}
