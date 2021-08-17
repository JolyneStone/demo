using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;

namespace PrintDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Microsoft XPS Document Writer 是测试时使用的，实际使用中，要替换成真正的打印机
            var printer = PrinterFactory.GetPrinter("Microsoft XPS Document Writer");
            printer.SetPageSize(80, null);
            printer.NewLine();
            var img = GetLogo();
            printer.PrintImage(img, StringAlignment.Center);
            printer.NewLine();
            printer.NewLine();
            printer.PrintText("永辉超市", FontSize.Large, alignment: StringAlignment.Center);
            printer.NewLine();
            printer.NewLine();
            printer.PrintText("单号：XD000269");
            printer.PrintText("流水号：000269", offset: 0.5f);
            printer.NewLine();
            printer.PrintText("收银员：***");
            printer.PrintText("日期：" + DateTime.Now.ToString("yyyy/MM/dd"), offset: 0.5f);
            printer.NewLine();
            printer.PrintText("VIP客户卡号：001");
            printer.NewLine();
            printer.PrintSolidLine();
            printer.NewLine();
            printer.PrintText("名称");
            printer.PrintText("单价", offset: 0.35f);
            printer.PrintText("数量", offset: 0.65f);
            printer.PrintText("金额", alignment: StringAlignment.Far);
            printer.NewLine();
            printer.PrintText("芹菜", width: 0.35f);
            printer.PrintText("2.9", width: 0.2f, offset: 0.35f);
            printer.PrintText("1", width: 0.2f, offset: 0.65F);
            printer.PrintText("2.9", alignment: StringAlignment.Far);
            printer.NewLine();
            printer.PrintDottedLine();
            printer.NewLine();
            printer.PrintText("合计");
            printer.PrintText("1", offset: 0.65f);
            printer.PrintText("2.90", alignment: StringAlignment.Far);
            printer.NewLine();
            printer.PrintText("满0.00减0.00折扣");
            printer.PrintText("-0.00", alignment: StringAlignment.Far);
            printer.NewLine();
            printer.PrintText("优惠金额：2.90");
            printer.PrintText("实收金额：0", offset: 0.5f);
            printer.NewLine();
            printer.PrintText("收款金额：0.00");
            printer.PrintText("找零金额：-2.90", offset: 0.5f);
            printer.NewLine();
            printer.PrintDottedLine();
            printer.NewLine();
            printer.PrintText("会员卡：001");
            printer.NewLine();
            printer.PrintText("本次积分：");
            printer.PrintText("会员余额：43.87", offset: 0.5f);
            printer.NewLine();
            printer.PrintText("可用积分：");
            printer.NewLine();
            printer.PrintSolidLine();
            printer.NewLine();
            printer.PrintText("永辉超市", FontSize.Large, alignment: StringAlignment.Center);
            printer.NewLine();
            printer.PrintText("欢迎光临，谢谢惠顾！", FontSize.Large, alignment: StringAlignment.Center);
            printer.NewLine();

            printer.Print();
            GC.Collect();
            
            Console.ReadKey();
        }

        private static Image GetLogo()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "logo.jpg");
            using (var fileStream = File.OpenRead(path))
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                return Image.FromStream(fileStream);
            }
        }
    }
}
