using AD5933_Lib;
using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ZedGraph;

namespace AD5933
{
    public partial class MainForm : Form
    {
        private AD5933_Eval eval = null;
     //   private static LineItem CurveReal;
     //   private static LineItem CurveImaginary;
        private static LineItem CurveMagnitude;
    //    private static LineItem CurvePhase;
        private static LineItem CurveCole;
        private static bool mustCalibrate = true;

        public bool Streaming = false;


        // public FileStream fs = new FileStream("Test.txt", FileMode.Create);
        // // First, save the standard output.
        // public TextWriter tmp = Console.Out;
        // public StreamWriter sw;

        // folloing is a try of IRONPython

        // public static ScriptRuntime pyRuntime = Python.CreateRuntime();
        // public static dynamic obj = pyRuntime.UseFile("test.py");

        // neccessity for socket init 
        private static byte[] result = new byte[1024];
        private static IPAddress host = IPAddress.Parse("10.221.55.7");
        private static IPAddress localhost = IPAddress.Parse("127.0.0.1");
        private static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static string CONF_MES = "confirmed...";

        public static void socketInit()
        {
            clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Console.WriteLine("f**k you!!!!");
            clientSocket.Connect(new IPEndPoint(localhost, 1213));
            Console.WriteLine("socket init done...");
            waitRecv();
        }


        public static bool sendMessage(String mes)
        {
            try
            {
                clientSocket.Send(Encoding.ASCII.GetBytes(mes + "#"));
                Console.WriteLine("sended message... " + mes);
                return true;
            }
            catch
            {
                Console.WriteLine("sending error!!!!");
                return false;
            }
        }

        public static bool sendTest(String mes)
        {
            try
            {
                clientSocket.Send(Encoding.ASCII.GetBytes(mes));
                return true;
            }
            catch
            {
                Console.WriteLine("sending error!!!!");
                return false;
            }
        }

        public static bool recvMessage()
        {
            try
            {
                //通过clientSocket接收数据
                int receiveNumber = clientSocket.Receive(result);
                Console.WriteLine("接收客户端{0}消息{1}", clientSocket.RemoteEndPoint.ToString(), Encoding.ASCII.GetString(result, 0, receiveNumber));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public static void waitRecv()
        {
            bool confirm;
            while (true)
            {
                confirm = recvMessage();
                if (confirm)
                    break;
                else
                    continue;
            }
        }

        private static void socketClose()
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        public MainForm()
        {
            InitializeComponent();
            initGraph();
            Application.Idle += Application_Idle;
            socketInit();
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            btnConnect.Enabled = (eval == null) && (cbBoardList.SelectedIndex > -1);
            btnDisconnect.Enabled = eval != null;
            btnGetTemperature.Enabled = eval != null;
            btnCalibrate.Enabled = eval != null;
            btnSweep.Enabled = (!mustCalibrate) && (eval != null);
            btnStream.Enabled = (!mustCalibrate) && (eval != null);
            btnBackground.Enabled = (!mustCalibrate) && (eval != null);
            btnOneTest.Enabled = (!mustCalibrate) && (eval != null);
            btnPCali.Enabled = (!mustCalibrate) && (eval != null);
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            cbBoardList.Items.Clear();
            cbBoardList.Items.AddRange((
                from b in AD5933_Eval.Boards
                select b.ToString()).ToArray());
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (cbBoardList.SelectedIndex > -1)
            {
                var item = cbBoardList.SelectedItem.ToString();

                byte part = Convert.ToByte(item);
                eval = new AD5933_Eval(part)
                {
                    StartFrequency = 10000,
                    IncFrequency = 2000,
                    Steps = 45,
                    SettlingCycles = 15,
                    PGAControl = AD5933_Eval.PgaGain.x1,
                    CalibrationResistor = 330000.0,
                    ExcitationVoltage = AD5933_Eval.OutputRange.Range1,
                };

                propertyGrid1.SelectedObject = eval;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (eval != null)
            {
                propertyGrid1.SelectedObject = null;

                eval.Dispose();
                eval = null;

            }
        }

        private void btnGetTemperature_Click(object sender, EventArgs e)
        {

            var temp = eval.ReadTemperature();
            MessageBox.Show(String.Format("Temperature {0}", temp));
        }

        private void initGraph()
        {
            
            var pane = mainGraph.GraphPane;
            pane.Title.Text = "AD5933";

            pane.XAxis.Title.Text = "Frequency";
            pane.XAxis.Scale.MinAuto = true;
            pane.XAxis.Scale.MaxAuto = true;

            pane.YAxis.Title.Text = "Impedance";
            pane.YAxis.Scale.MinAuto = true;
            pane.YAxis.Scale.MaxAuto = true;

            pane.Y2Axis.Title.Text = "Phase";
            pane.Y2Axis.Scale.MinAuto = true;
            pane.Y2Axis.Scale.MaxAuto = true;
            pane.Y2Axis.IsVisible = true;

            mainGraph.AxisChange();

          //  CurveReal = pane.AddCurve("Real", new PointPairList(), Color.Blue, SymbolType.Diamond);
          //  CurveImaginary = pane.AddCurve("Imaginary", new PointPairList(), Color.Green, SymbolType.Diamond);

            CurveMagnitude = pane.AddCurve("Absolute Imp.", new PointPairList(), Color.Black, SymbolType.Circle);
         //   CurvePhase = pane.AddCurve("Phase", new PointPairList(), Color.Red, SymbolType.Circle);
         //   CurvePhase.IsY2Axis = true;


            var panec = colePlot.GraphPane;
            panec.XAxis.Title.Text = "Real";
            panec.XAxis.Scale.MinAuto = true;
            panec.XAxis.Scale.MaxAuto = true;

            panec.YAxis.Title.Text = "Imaginary";
            panec.YAxis.Scale.MinAuto = true;
            panec.YAxis.Scale.MaxAuto = true;

            CurveCole = panec.AddCurve("", new PointPairList(), Color.Black);
            colePlot.AxisChange();
        }

        private void btnSweep_Click(object sender, EventArgs e)
        {
            // Console.WriteLine("T = " + eval.DataStreamT);
            var save = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
              //  CurveReal.Clear();
              //  CurveImaginary.Clear();
              //  CurvePhase.Clear();
                CurveMagnitude.Clear();
                CurveCole.Clear();

                foreach (var snap in eval.SweepMeasure())
                {
                //    CurveReal.AddPoint(snap.Frequency, snap.RealPart);
                 //   CurveImaginary.AddPoint(snap.Frequency, snap.ImaginaryPart);
                 //   CurvePhase.AddPoint(snap.Frequency, snap.Phase);
                    CurveMagnitude.AddPoint(snap.Frequency, snap.Impedance);

                    CurveCole.AddPoint(snap.RealPart, snap.ImaginaryPart);
                    
                    mainGraph.AxisChange();
                    mainGraph.Invalidate();

                }
                colePlot.AxisChange();
                colePlot.Invalidate();
            }
            finally
            {
                this.Cursor = save;
            }



/*            if (eval != null)
           {
                eval.DoSweep().Subscribe((snap) =>
                {
                    zedGraphControl1.
                });
            } */
        }

        

        private void btnCalibrate_Click(object sender, EventArgs e)
        {
            eval.CalibrateMultipoint();
            propertyGrid1.Refresh();
            mustCalibrate = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // background data sending
        private void btnBackground_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Start getting background");
            
            var save = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            string mes;
            sendMessage(CONF_MES);
            for (int i = 0; i < 5; ++i)
            {
                try
                {
                    foreach (var snap in eval.SweepMeasure())
                    {
                        mes = snap.Frequency + "," + snap.Impedance + "," + snap.Phase;
                        sendMessage(mes);
                        // waitRecv();
                    }
                }
                catch
                {
                    Console.WriteLine("没法出去，凉了......");
                }
                finally
                {
                    Console.WriteLine("...........");
                }
                
            }
            this.Cursor = save;
            Console.WriteLine("Finished getting background");
        }

        private void btnPCali_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Start Python Calibration");
            
            var save = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            string mes;
            sendMessage(CONF_MES);
            for (int i = 0; i < 3; ++i)
            {
                try
                {
                    foreach (var snap in eval.SweepMeasure())
                    {
                        mes = snap.Frequency + "," + snap.Impedance + "," + snap.Phase;
                        sendMessage(mes);
                        // waitRecv();
                    }
                }
                catch
                {
                    Console.WriteLine("没法出去，凉了......");
                }
                finally
                {
                    Console.WriteLine("..........");
                }
            }
            this.Cursor = save;
            Console.WriteLine("Finished Python Calibration");
        }

        private void btnOneTest_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Start one single test...");
            DateTime beforDT = System.DateTime.Now;
            var save = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            string mes;
            sendMessage(CONF_MES);
            for (int i = 0; i < 3; ++i)
            {
                try
                {
                    foreach (var snap in eval.SweepMeasure())
                    {
                        mes = snap.Frequency + "," + snap.Impedance + "," + snap.Phase;
    
                        sendMessage(mes);
                        // waitRecv();
                        // sendTest(mes);
                    }
                }
                catch
                {
                    Console.WriteLine("没法出去，凉了......");
                }
                finally
                {
                    Console.WriteLine("finished");
                }
            }
            this.Cursor = save;
            DateTime afterDT = System.DateTime.Now;
            TimeSpan ts = afterDT.Subtract(beforDT);
            Console.WriteLine("DateTime总共花费{0}ms.", ts.TotalMilliseconds);
            Console.WriteLine("Finished one single test...");
        }

        private void btnStream_Click(object sender, EventArgs e)
        {
            Streaming = true;
            // var save = this.Cursor;
            // Console.WriteLine(Streaming);
            Console.WriteLine("Start streaming...");
            string mes;
            while (true)
            {
                sendMessage(CONF_MES);
                for (int i = 0; i < 3; ++i)
                {
                    try
                    {
                        foreach (var snap in eval.SweepMeasure())
                        {
                            mes = snap.Frequency + "," + snap.Impedance + "," + snap.Phase;

                            sendMessage(mes);
                            // waitRecv();
                            // sendTest(mes);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("没法出去，凉了......");
                    }
                    finally
                    {
                        Console.WriteLine("finished");
                    }
                }
            }
        }
    }
    
}
