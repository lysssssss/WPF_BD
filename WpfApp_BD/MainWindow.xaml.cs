﻿/***********************Project Version1.5*************************
@项目名:北斗传输4.0(C#)
@File:MainWindow.xaml.cs
@File_Version:1.5a
@Author:lys
@QQ:591986780
@UpdateTime:2018年5月28日15:56:43

@说明:展示界面的动态显示

本程序基于.Net4.6.1编写的北斗短报文传输程序
界面使用WPF框架编写
在vs2017里运行通过

******************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using BD_Protocol;
namespace WpfApp_BD
{
    using UINT = System.UInt32;
    using UCHR = System.Byte;
    public class Box
    {
        public BD bdxx;
        public MainWindow win;

        public Box(MainWindow m, BD bd)
        {
            win = m;
            bdxx = bd;

        }

    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        BD bdxx;
        //DispatcherTimer autoTick = new DispatcherTimer();//定时发送
        public MainWindow(BD b)
        {
            bdxx = b;
            InitializeComponent();
            isfirstrun = false;
        }
        System.Timers.Timer timer_update;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //autoTick.Tick += new EventHandler(Seamphore_thread);//定时中断
            //autoTick.Interval = TimeSpan.FromMilliseconds(200);//设置自动间隔
            //autoTick.Start();//开启自动
            LoadID();
            timer_update = new System.Timers.Timer(500);//实例化Timer类，设置间隔时间为100毫秒；     
            timer_update.Elapsed += new System.Timers.ElapsedEventHandler(Seamphore_thread);//到达时间的时候执行事件；   
            timer_update.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；     
            timer_update.Enabled = true;//需要调用 timer.Start()或者timer.Enabled = true来启动它， timer.Start()的内部原理还是设置timer.Enabled = true;
        }

        public static class DispatcherHelper
        {
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            public static void DoEvents()
            {
                DispatcherFrame frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(ExitFrames), frame);
                try { Dispatcher.PushFrame(frame); }
                catch (InvalidOperationException ex) { WriteLog.WriteError(ex); }
            }
            private static object ExitFrames(object frame)
            {
                ((DispatcherFrame)frame).Continue = false;
                return null;
            }
        }
        void UIAction(Action action)//在主线程外激活线程方法
        {
            System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(App.Current.Dispatcher));
            System.Threading.SynchronizationContext.Current.Post(_ => action(), null);
        }

        public void Seamphore_thread(object sender, EventArgs e)
        {
            if ((bdxx.print_flag & BD.PRINT_STATUS) != 0)
            {
                string str = "";
                if ((bdxx.status & BD.STATUS_BIT_STEP) == BD.STEP_NONE)
                {
                    str = "未初始化";
                }
                else if ((bdxx.status & BD.STATUS_BIT_STEP) == BD.STEP_ICJC)
                {
                    str = "IC检测";
                }
                else if ((bdxx.status & BD.STATUS_BIT_STEP) == BD.STEP_READY)
                {
                    str = "就绪";
                }
                else if ((bdxx.status & BD.STATUS_BIT_STEP) == BD.STEP_SJSC)
                {
                    str = "时间输出";
                }
                else if ((bdxx.status & BD.STATUS_BIT_STEP) == BD.STEP_XJZJ)
                {
                    str = "系统自检";
                }
                else if ((bdxx.status & BD.STATUS_BIT_STEP) == BD.STEP_GNPS)
                {
                    str = "定位信息";
                }
                else if ((bdxx.status & BD.STATUS_BIT_STEP) == BD.STEP_GNTS)
                {
                    str = "时间信息";
                }
                else if ((bdxx.status & BD.STATUS_BIT_STEP) == BD.STEP_GNVS)
                {
                    str = "可视卫星";
                }
                UIAction(() =>
                {
                    Label_Init_text.Content = str;
                });
                bdxx.print_flag &= ~BD.PRINT_STATUS;
            }
            if ((bdxx.print_flag & BD.PRINT_DWXX) != 0)
            {

            }
            if ((bdxx.print_flag & BD.PRINT_BLOCK) != 0)
            {
                UIAction(() =>
                {
                    label_djs_text.Content = Convert.ToString(bdxx.SEND_BLOCKTIME) + "s";
                });
                bdxx.print_flag &= ~BD.PRINT_BLOCK;

            }
            if ((bdxx.print_flag & BD.PRINT_TXXX) != 0)
            {
                UIAction(() =>
                {
                    label_txxx_xxlb_text.Content = Convert.ToString(bdxx.txxx.xxlb, 2);
                    label_txxx_fxfd_text.Content = Convert.ToString(bdxx.txxx.fxfdz[0] * 256 * 256 + bdxx.txxx.fxfdz[1] * 256 + bdxx.txxx.fxfdz[2]);
                    label_txxx_fxsj_text.Content = Convert.ToString(bdxx.txxx.fxsj_h) + "时" + Convert.ToString(bdxx.txxx.fxsj_m) + "分";
                    label_txxx_dwcd_text.Content = Convert.ToString(bdxx.txxx.dwcd / 8.0) + "bytes(" + Convert.ToString(bdxx.txxx.dwcd) + "bits)";
                    label_txxx_crc_text.Content = Convert.ToString(bdxx.txxx.crc);
                    label_txxx_lasttime_text.Content = Convert.ToString(bdxx.gntx.year) + "年" + Convert.ToString(bdxx.gntx.month) + "月" + Convert.ToString(bdxx.gntx.day) + "日" + Convert.ToString(bdxx.gntx.hour) + String.Format(":{0:D2}", bdxx.gntx.minute) + String.Format(":{0:D2}", bdxx.gntx.second);
                    if (cb_txxx_hexordec.IsChecked == true)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < bdxx.txxx.dwnr.Length; i++)
                        {
                            sb.AppendFormat("{0:x2}" + " ", bdxx.txxx.dwnr[i]);
                        }
                        textbox_txxx_dwnr.Text = sb.ToString().ToUpper();
                    }
                    else
                    {
                        textbox_txxx_dwnr.Text = new ASCIIEncoding().GetString(bdxx.txxx.dwnr);
                    }
                });
                bdxx.print_flag &= ~BD.PRINT_TXXX;
            }
            if ((bdxx.print_flag & BD.PRINT_ICXX) != 0)
            {
                UIAction(() =>
                {
                    label_icxx_yhid_text.Content = Convert.ToString(bdxx.icxx.yhdz[0] * 256 * 256 + bdxx.icxx.yhdz[1] * 256 + bdxx.icxx.yhdz[2]);
                    label_icxx_zh_text.Content = Convert.ToString(bdxx.icxx.zh);
                    label_icxx_tbid_text.Content = Convert.ToString(bdxx.icxx.tbid);
                    label_icxx_yhtz_text.Content = Convert.ToString(bdxx.icxx.yhtz);
                    label_icxx_fwpd_text.Content = Convert.ToString(bdxx.icxx.fwpd);
                    label_icxx_txdj_text.Content = Convert.ToString(bdxx.icxx.txdj);
                    label_icxx_jmbz_text.Content = Convert.ToString(bdxx.icxx.jmbz);
                    label_icxx_xsyhs_text.Content = Convert.ToString(bdxx.icxx.xsyhzs);
                });
                bdxx.print_flag &= ~BD.PRINT_ICXX;
            }
            if ((bdxx.print_flag & BD.PRINT_ZJXX) != 0)
            {
                UIAction(() =>
                {
                    label_zjxx_iczt_text.Content = "0x" + Convert.ToString(bdxx.zjxx.iczt, 16);
                    label_zjxx_yjzt_text.Content = "0x" + Convert.ToString(bdxx.zjxx.yjzt, 16);
                    label_zjxx_dczt_text.Content = "0x" + Convert.ToString(bdxx.zjxx.dcdl, 16);
                    label_zjxx_rzzt_text.Content = "0x" + Convert.ToString(bdxx.zjxx.rzzt, 16);
                    label_zjxx_bsgl1_text.Content = Convert.ToString(bdxx.zjxx.bsgl[0]);
                    label_zjxx_bsgl2_text.Content = Convert.ToString(bdxx.zjxx.bsgl[1]);
                    label_zjxx_bsgl3_text.Content = Convert.ToString(bdxx.zjxx.bsgl[2]);
                    label_zjxx_bsgl4_text.Content = Convert.ToString(bdxx.zjxx.bsgl[3]);
                    label_zjxx_bsgl5_text.Content = Convert.ToString(bdxx.zjxx.bsgl[4]);
                    label_zjxx_bsgl6_text.Content = Convert.ToString(bdxx.zjxx.bsgl[5]);
                });
                bdxx.print_flag &= ~BD.PRINT_ZJXX;
            }
            if ((bdxx.print_flag & BD.PRINT_SJXX) != 0)
            {

            }
            if ((bdxx.print_flag & BD.PRINT_FKXX) != 0)
            {
                string str = "";
                if (bdxx.fkxx.flbz == 0)
                {
                    str = "成功,指令:" + (char)(bdxx.fkxx.fjxx[0]) + (char)(bdxx.fkxx.fjxx[1]) + (char)(bdxx.fkxx.fjxx[2]) + (char)(bdxx.fkxx.fjxx[3]);
                }
                else if (bdxx.fkxx.flbz == 1)
                    str = "失败,指令:" + (char)(bdxx.fkxx.fjxx[0]) + (char)(bdxx.fkxx.fjxx[1]) + (char)(bdxx.fkxx.fjxx[2]) + (char)(bdxx.fkxx.fjxx[3]);
                else if (bdxx.fkxx.flbz == 2)
                    str = "信号未锁定";
                else if (bdxx.fkxx.flbz == 3)
                    str = "电量不足";
                else if (bdxx.fkxx.flbz == 4)
                    str = "发射频度未到,时间:" + Convert.ToString(bdxx.fkxx.fjxx[3]) + "秒";
                else if (bdxx.fkxx.flbz == 5)
                    str = "加解密错误";
                else if (bdxx.fkxx.flbz == 6)
                    str = "CRC错误,指令:" + (char)(bdxx.fkxx.fjxx[0]) + (char)(bdxx.fkxx.fjxx[1]) + (char)(bdxx.fkxx.fjxx[2]) + (char)(bdxx.fkxx.fjxx[3]);
                else if (bdxx.fkxx.flbz == 7)
                    str = "用户级被抑制";
                else if (bdxx.fkxx.flbz == 8)
                    str = "抑制解除\n";
                str += "  " + Convert.ToString(bdxx.gntx.hour) + ":" + Convert.ToString(bdxx.gntx.minute) + ":" + Convert.ToString(bdxx.gntx.second);

                UIAction(() =>
                {
                    ListBoxItem temp = new ListBoxItem();
                    temp.Content = str;
                    temp.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                    temp.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    listbox_fkxx.Items.Insert(0, temp);
                });
                bdxx.print_flag &= ~BD.PRINT_FKXX;
            }
            if ((bdxx.print_flag & BD.PRINT_GNTX) != 0)
            {
                sbyte sq = bdxx.gntx.sqlx;
                string str = "";
                if (sq >= 0)
                {
                    str = "东";
                }
                else
                {
                    str = "西";
                    sq *= -1;
                }
                UIAction(() =>
                {
                    label_gntx_sq_text.Content = str + Convert.ToString(sq) + "区";
                    label_gntx_sj_text.Content = Convert.ToString(bdxx.gntx.year) + "年" + Convert.ToString(bdxx.gntx.month) + "月" + Convert.ToString(bdxx.gntx.day) + "日" + Convert.ToString(bdxx.gntx.hour) + String.Format(":{0:D2}", bdxx.gntx.minute) + String.Format(":{0:D2}", bdxx.gntx.second);
                });
                bdxx.print_flag &= ~BD.PRINT_GNTX;
            }
            if ((bdxx.print_flag & BD.PRINT_GNVX) != 0)
            {
                UIAction(() =>
                {
                    label_gnvx_gwxgs_text.Content = Convert.ToString(bdxx.gnvx.gps_wxgs);
                    label_gnvx_bwxgs_text.Content = Convert.ToString(bdxx.gnvx.bds_wxgs);
                    listbox_gnvx_bwxxx.Items.Clear();
                    ListBoxItem _temp = new ListBoxItem();
                    _temp.Content = "(卫星编号)(卫星仰角)(方位角)(信噪比)";
                    _temp.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                    _temp.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    listbox_gnvx_bwxxx.Items.Add(_temp);
                    listbox_gnvx_gwxxx.Items.Clear();
                    ListBoxItem _temp2 = new ListBoxItem();
                    _temp2.Content = "(卫星编号)(卫星仰角)(方位角)(信噪比)";
                    _temp2.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                    _temp2.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    listbox_gnvx_gwxxx.Items.Add(_temp2);
                    for (int i = 0; i < bdxx.gnvx.bds_wxgs; ++i)
                    {
                        ListBoxItem temp = new ListBoxItem();
                        temp.Content = "(" + Convert.ToString(bdxx.gnvx.bds_wxxx[i].wxbh) + ")(" + Convert.ToString(bdxx.gnvx.bds_wxxx[i].wxyj) + "°)(" + Convert.ToString(bdxx.gnvx.bds_wxxx[i].fwj) + "°)(" + Convert.ToString(bdxx.gnvx.bds_wxxx[i].xzb) + "db)";
                        temp.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        temp.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        listbox_gnvx_bwxxx.Items.Add(temp);
                    }

                    for (int i = 0; i < bdxx.gnvx.gps_wxgs; ++i)
                    {
                        ListBoxItem temp = new ListBoxItem();
                        temp.Content = "(" + Convert.ToString(bdxx.gnvx.gps_wxxx[i].wxbh) + ")(" + Convert.ToString(bdxx.gnvx.gps_wxxx[i].wxyj) + "°)(" + Convert.ToString(bdxx.gnvx.gps_wxxx[i].fwj) + "°)(" + Convert.ToString(bdxx.gnvx.gps_wxxx[i].xzb) + "db)";
                        temp.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        temp.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                        listbox_gnvx_gwxxx.Items.Add(temp);
                    }

                });
                bdxx.print_flag &= ~BD.PRINT_GNVX;
            }
            if ((bdxx.print_flag & BD.PRINT_GNPX) != 0)
            {
                UIAction(() =>
                {
                    label_gnpx_jdfw_text.Content = (char)bdxx.gnpx.jdfw;
                    label_gnpx_jd_text.Content = Convert.ToString(bdxx.gnpx.jd);
                    label_gnpx_jf_text.Content = Convert.ToString(bdxx.gnpx.jf);
                    label_gnpx_jm_text.Content = Convert.ToString(bdxx.gnpx.jm);
                    label_gnpx_jxm_text.Content = Convert.ToString(bdxx.gnpx.jxm);
                    label_gnpx_wdfw_text.Content = (char)bdxx.gnpx.wdfw;
                    label_gnpx_wd_text.Content = Convert.ToString(bdxx.gnpx.wd);
                    label_gnpx_wf_text.Content = Convert.ToString(bdxx.gnpx.wf);
                    label_gnpx_wm_text.Content = Convert.ToString(bdxx.gnpx.wm);
                    label_gnpx_wxm_text.Content = Convert.ToString(bdxx.gnpx.wxm);
                    label_gnpx_gd_text.Content = Convert.ToString(bdxx.gnpx.gd) + "m";
                    label_gnpx_sd_text.Content = Convert.ToString(bdxx.gnpx.sd / 10.0) + "m/s";
                    label_gnpx_fx_text.Content = Convert.ToString(bdxx.gnpx.fx) + "°";
                    label_gnpx_wxs_text.Content = Convert.ToString(bdxx.gnpx.wxs);
                    label_gnpx_zt_text.Content = bdxx.gnpx.zt == 1 ? "已定位" : "未定位";
                    label_gnpx_jdxs_text.Content = Convert.ToString(bdxx.gnpx.jdxs);
                    label_gnpx_gjwc_text.Content = Convert.ToString(bdxx.gnpx.gjwc / 10.0) + "m";
                    textbox_gnpx_zhzb.Text = Convert.ToString((((bdxx.gnpx.wxm / 60.0) + bdxx.gnpx.wm) / 60.0 + bdxx.gnpx.wf) / 60.0 + bdxx.gnpx.wd) + "," + Convert.ToString((((bdxx.gnpx.jxm / 60.0) + bdxx.gnpx.jm) / 60.0 + bdxx.gnpx.jf) / 60.0 + bdxx.gnpx.jd);
                });
                bdxx.print_flag &= ~BD.PRINT_GNPX;

            }
            if ((bdxx.print_flag & BD.PRINT_FIRST_ADDRESS) != 0)
            {
                UIAction(() =>
                {
                    label_zhdz.Content = bdxx.address;
                });
                bdxx.print_flag &= ~BD.PRINT_FIRST_ADDRESS;
            }
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // this.s();Application
            // Window.
            // myStartMain.app.Shutdown();
            try
            {
                bdxx.mycom.Close();
            }
            catch (Exception ex)
            {
                WriteLog.WriteError(ex);

            }
            System.Environment.Exit(0);
        }


        /// <summary>
        /// 字符串转换16进制字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private string strToHexByte(string hexString)
        {
            try
            {
                byte[] bs = null;
                bs = System.Text.Encoding.ASCII.GetBytes(hexString.Trim());
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bs.Length; i++)
                {
                    sb.AppendFormat("{0:x2}" + " ", bs[i]);
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {

                WriteLog.WriteError(ex);
            }
            return "";
        }
        /// <summary>
        /// 将一条十六进制字符串转换为ASCII
        /// </summary>
        /// <param name="hexstring">一条十六进制字符串</param>
        /// <returns>返回一条ASCII码</returns>
        public static string HexStringToASCII(string hexstring)
        {
            try
            {
                byte[] bt = HexStringToBinary(hexstring);
                string lin = "";
                for (int i = 0; i < bt.Length; i++)
                {
                    lin = lin + bt[i] + " ";
                }


                string[] ss = lin.Trim().Split(new char[] { ' ' });
                char[] c = new char[ss.Length];
                int a;
                for (int i = 0; i < c.Length; i++)
                {
                    a = Convert.ToInt32(ss[i]);
                    c[i] = Convert.ToChar(a);
                }

                string b = new string(c);
                return b;
            }
            catch (Exception ex)
            {

                WriteLog.WriteError(ex);
            }
            return "";
        }
        /**/
        /// <summary>
        /// 16进制字符串转换为二进制数组
        /// </summary>
        /// <param name="hexstring">用空格切割字符串</param>
        /// <returns>返回一个二进制字符串</returns>
        public static byte[] HexStringToBinary(string hexstring)
        {
            try
            {
                string[] tmpary = hexstring.Trim().Split(' ');
                byte[] buff = new byte[tmpary.Length];
                for (int i = 0; i < buff.Length; i++)
                {
                    buff[i] = Convert.ToByte(tmpary[i], 16);
                }
                return buff;
            }
            catch ( Exception ex)
            {

                WriteLog.WriteError(ex);
            }
            return null;
        }



        private void cb_txxx_hexordec_Click(object sender, RoutedEventArgs e)
        {
            if (this.cb_txxx_hexordec.IsChecked == true)
            {
                cb_txxx_hexordec.Content = "十六进制";
            }
            else
            {
                cb_txxx_hexordec.Content = "ASCII";
            }

            if (textbox_txxx_dwnr.Text != string.Empty)
            {
                if (this.cb_txxx_hexordec.IsChecked == true)
                {

                    textbox_txxx_dwnr.Text = strToHexByte(textbox_txxx_dwnr.Text);
                }
                else
                {

                    textbox_txxx_dwnr.Text = HexStringToASCII(textbox_txxx_dwnr.Text);
                }
            }

        }

        private void btn_setcombaud_Click(object sender, RoutedEventArgs e)
        {

        }

        void LoadID()
        {
            try
            {
                DataSet ds2 = MyDataBase.Select_UserId();
                DataSetToList_For_UserId(ds2, 0);
                cbb_id.ItemsSource = Cbb_id;
                cbb_id.SelectedValuePath = "Key";
                //cbb_id.DisplayMemberPath = "Value";
                cbb_id.SelectedIndex = -1;
            }
            catch (Exception ex)
            {

                WriteLog.WriteError(ex);
            }

        }
        public void DataSetToList_For_UserId(DataSet ds, int tableIndext)
        {
            try
            {
                if (ds == null || ds.Tables.Count <= 0 || tableIndext < 0)
                {
                    return;
                }
                DataTable dt = ds.Tables[tableIndext]; //取得DataSet里的一个下标为tableIndext的表，然后赋给dt  
                Cbb_id = new List<string>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i][0] != DBNull.Value)
                    {
                        Cbb_id.Add(Convert.ToString(dt.Rows[i][0]));
                    }
                    else
                    {
                        Cbb_id.Add("");
                    }
                }
            }
            catch (Exception ex)
            {

                WriteLog.WriteError(ex);
            }
            //确认参数有效  
            
        }
        List<string> Cbb_id { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (e.Source is Button)
                {
                    if (!isfirstrun)
                    {
                        int id = Convert.ToInt32(cbb_id.SelectedItem);
                        //MessageBox.Show(cbb_zlgn.Text);
                        if (cbb_zlgn.Text.Equals("重启"))
                        {
                            byte[] aaa = new byte[1];
                            UCHR[] _id = new UCHR[3];
                            _id[0] = Convert.ToByte((id >> 16) & 0xff);
                            _id[1] = Convert.ToByte((id >> 8) & 0xff);
                            _id[2] = Convert.ToByte(id & 0xff);
                            aaa[0] = 1;
                            bdxx.BD_send(ref aaa, (UINT)(1), _id);
                        }
                        if (cbb_zlgn.Text.Equals("蜂鸣一秒"))
                        {
                            byte[] aaa = new byte[1];
                            UCHR[] _id = new UCHR[3];
                            _id[0] = Convert.ToByte((id >> 16) & 0xff);
                            _id[1] = Convert.ToByte((id >> 8) & 0xff);
                            _id[2] = Convert.ToByte(id & 0xff);
                            aaa[0] = 2;
                            bdxx.BD_send(ref aaa, (UINT)(1), _id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                WriteLog.WriteError(ex);
            }
            
        }
        bool isfirstrun = true;
    }
}
