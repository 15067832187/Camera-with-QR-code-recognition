#region 初始引用
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
#endregion

#region 额外引用
using System.IO;
using ZXing;
using AForge.Video.DirectShow;
using Size = System.Drawing.Size;
#endregion

namespace MyCa
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection videoDevices;
        public Form1()
        {
            InitializeComponent();
        }

        #region  事件集合
        private void Form1_Load(object sender, EventArgs e)
        {
            Waitlink();//进入等待连接状态
            Lb1Change("程序启动成功，请选择视频输入设备并连接");
            LoadCa();
        } 

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {CloseLink();}
 
        private void Button1_Click(object sender, EventArgs e)//连接开始按钮
        {
            Lb1Change("连接建立中...");
            CameraLink();
            Linking();
        }

        private void Button2_Click(object sender, EventArgs e)//断开连接按钮
        {
            Waitlink();
            CloseLink();
        }      

        private void Button3_Click(object sender, EventArgs e)//拍照按钮
        {
            Takephoto();
        }

        private void Button4_Click(object sender, EventArgs e)//解析开始按钮
        {
            Jiexing();
        }

        private void Button5_Click(object sender, EventArgs e)//结束解析按钮
        {
            Linking();
        }
        #endregion

        #region  方法集合
        private void LoadCa()//加载视频输入设备
        {
            try
            {
                //枚举所有视频输入设备
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                    throw new ApplicationException();
                foreach (FilterInfo device in videoDevices)
                {
                    comboBox1.Items.Add(device.Name);
                    Lb1Change("检测到视频输入设备，请选择");
                }
                comboBox1.SelectedIndex = 0;
            }
            catch (ApplicationException)
            {
                comboBox1.Items.Add("未检测到视频输入设备");
                Lb1Change("未检测到视频输入设备，请检测您的设备完好，谢谢");
            }
        }

        private void CameraLink()//连接视频输入设备
        {
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[comboBox1.SelectedIndex].MonikerString);
            videoSource.DesiredFrameSize = new Size(320, 240);
            videoSource.DesiredFrameRate = 1;
            vp.VideoSource = videoSource;
            vp.Start();
            Lb1Change("连接建立成功！");
        }

        private void CloseLink()//关闭连接
        {
            Lb1Change("检测到用户断开连接，开始断开");
            vp.SignalToStop();
            vp.WaitForStop();
            Lb1Change("断开成功！");
        }
        
        private void Lb1Change(string instring)//下方提示
        {
            listBox1.Items.Add(DateTime.Now.ToLongTimeString().ToString()+"    "+instring);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        public void Takephoto()//拍照功能
        {
            try
            {
                Bitmap bitmap = new Bitmap(460, 385);
                Rectangle rect1 = new Rectangle(40, 80, 460, 385);
                vp.DrawToBitmap(bitmap, rect1);
                string fileName = "D:/temp/未命名图片.jpg";
                bitmap.Save(fileName, ImageFormat.Jpeg);
                Lb1Change(fileName + "    保存成功!");
                bitmap.Dispose();
            }
            catch (Exception)
            {
                Lb1Change("拍照失败");
            }
        }

        public void Alluers()//解析功能整合
        {
            try
            {
                Bitmap bitmap = new Bitmap(460, 385);
                Rectangle rect1 = new Rectangle(40, 80, 460, 385);
                vp.DrawToBitmap(bitmap, rect1);
                BarcodeReader reader = new BarcodeReader();
                Result result = reader.Decode(bitmap);
                string resultText = result.Text;
                Lb1Change("解析成功，正在转入：" + resultText);
                Lb1Change("解析结束");
                Linking();
                System.Diagnostics.Process.Start("iexplore.exe", resultText);//打开浏览器
            }
            catch (Exception)
            {
                 Lb1Change("解析失败,请将二维码对准摄像头");
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)//1000ms重新解析一次
        {
            Alluers();
        }
        #endregion

        #region 快捷状态切换方法
        public void Waitlink()//等待连接：（开启：连接）（禁用：断开连接，拍照，解析相关）
        {
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            timer1.Enabled = false;
        }

        public void Linking()//连接中（开启：断开连接，拍照，开始解析）（禁用：连接，结束解析）
        {
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = false;
            timer1.Enabled = false;
        }

        public void Jiexing()//解析中（开启：结束解析）（禁用：连接相关，拍照，开始解析）
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = true;
            timer1.Enabled = true;
        }
        #endregion

        #region  备用：删除保存的图片
        /// <summary>
        /// 删除文件夹以及文件
        /// </summary>
        /// <param name="directoryPath"> 文件夹路径 </param>
        /// <param name="fileName"> 文件名称 </param>
        public static void DeleteDirectory(string directoryPath, string fileName)
        {

            //删除文件
            for (int i = 0; i < Directory.GetFiles(directoryPath).ToList().Count; i++)
            {
                if (Directory.GetFiles(directoryPath)[i] == fileName)
                {
                    File.Delete(fileName);
                }
            }
        }
        #endregion

        #region  备用：解析二维码初始代码
        private void Startjx()//解析二维码
        {
            try
            {
                string path = "D:/temp/未命名图片.jpg";
                FileStream fs = new FileStream(path, FileMode.Open);
                Bitmap bitmap = Bitmap.FromStream(fs) as Bitmap;
                BarcodeReader reader = new BarcodeReader();
                Result result = reader.Decode(bitmap);
                string resultText = result.Text;
                Lb1Change("解析成功，正在转入："+resultText);
                Lb1Change("解析结束");
                timer1.Enabled = false;
                System.Diagnostics.Process.Start("iexplore.exe", resultText);
            }
            catch (Exception)
            {
                Lb1Change("解析失败,请将二维码对准摄像头");
                //DeleteDirectory("D:/temp", "未命名图片.jpg");
            }
        }
        #endregion
    }
}
