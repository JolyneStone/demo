using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintDemo
{
    public interface IPrinter
    {
        /// <summary>
        /// 设置页面大小
        /// </summary>
        /// <param name="paperWidth"></param>
        /// <param name="paperHight"></param>
        void SetPageSize(double paperWidth, int? paperHight);

        /// <summary>
        /// 打印文字
        /// </summary>
        /// <param name="content"></param>
        /// <param name="fontSize"></param>
        /// <param name="stringAlignment"></param>
        /// <param name="width"></param>
        /// <param name="offset"></param>
        void PrintText(
            string content,
            FontSize fontSize = FontSize.Normal,
            StringAlignment stringAlignment = StringAlignment.Near,
            float width = 1,
            float offset = 0
            );

        /// <summary>
        /// 打印图片
        /// </summary>
        /// <param name="image"></param>
        /// <param name="stringAlignment"></param>
        void PrintImage(Image image, StringAlignment stringAlignment = StringAlignment.Near);

        /// <summary>
        /// 打印一行
        /// </summary>
        void PrintSolidLine();

        /// <summary>
        /// 打印一行虚线
        /// </summary>
        /// <param name="fontSize"></param>
        void PrintDottedLine();

        /// <summary>
        /// 新起一行
        /// </summary>
        void NewLine();

        /// <summary>
        /// 开始打印
        /// </summary>
        void Print();
    }
}
