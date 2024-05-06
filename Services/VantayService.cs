using HW_Lib_Test;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Management;

namespace printer_2.Services
{
    public enum ColorCode
    {
        BLUE = 102,
        RED = 103,
    }

    public enum StatusVantay
    {
        DANG_DANGKY,
        DANGKY_THANHCONG,
        DANGKY_THATBAI
    }

    public class ResponseVantay
    {
        public string key { get; set; }
        public object data { get; set; }

        public int solanconlai { get; set; }
        public int size { get; set; }

        public StatusVantay status { get; set; }
    }
    public interface IVantayService
    {
        void USB_Added();

        void SetTrangThai(bool status);

        void removeDataSensor();
    }
    public class VantayService : IVantayService
    {
        private readonly ISocketService _socketService;
        public VantayService(ISocketService socketService)
        {
            _socketService = socketService;
        }

        byte[] g_FPBuffer;
        int g_FPBufferSize = 0;
        bool g_bIsTimeToDie = false;
        IntPtr g_Handle = IntPtr.Zero;
        IntPtr g_biokeyHandle = IntPtr.Zero;
        IntPtr g_FormHandle = IntPtr.Zero;
        int g_nWidth = 0;
        int g_nHeight = 0;
        public static bool g_IsRegister = false;
        public static int soLanDaDangKy = 0;
        // int g_RegisterCount = 0;
        const int TONG_SO_LAN_DANG_KY_MAC_DINH = 3;
        byte[][] g_RegTmps = new byte[3][];
        byte[] g_RegTmp = new byte[2048];
        byte[] g_VerTmp = new byte[2048];

        const int MESSAGE_FP_RECEIVED = 0x0400 + 6;

        private void DoCapture()
        {
            while (!g_bIsTimeToDie)
            {
                int ret = ZKFPCap.sensorCapture(g_Handle, g_FPBuffer, g_FPBufferSize);
                Console.WriteLine(2222);
                Console.WriteLine(ret);


                if (ret > 0)
                {
                    DefWndProc();
                }
            }
        }

        public void USB_Added()
        {
            int ret = 0;
            byte[] paramValue = new byte[64];

            Array.Clear(paramValue, 0, paramValue.Length);
            paramValue[0] = 1;
            ZKFPCap.sensorSetParameterEx(g_Handle, 1100, paramValue, 4);
            ret = ZKFPCap.sensorInit();
            if (ret != 0)
            {
                  
                return;
            }
            g_Handle = ZKFPCap.sensorOpen(0);

            Array.Clear(paramValue, 0, paramValue.Length);
            ZKFPCap.sensorGetVersion(paramValue, paramValue.Length);
            ret = paramValue.Length;
            Array.Clear(paramValue, 0, paramValue.Length);
            ZKFPCap.sensorGetParameterEx(g_Handle, 1, paramValue, ref ret);
            g_nWidth = BitConverter.ToInt32(paramValue, 0);

            ret = paramValue.Length;
            Array.Clear(paramValue, 0, paramValue.Length);
            ZKFPCap.sensorGetParameterEx(g_Handle, 2, paramValue, ref ret);
            g_nHeight = BitConverter.ToInt32(paramValue, 0);

            ret = paramValue.Length;
            Array.Clear(paramValue, 0, paramValue.Length);
            ZKFPCap.sensorGetParameterEx(g_Handle, 106, paramValue, ref ret);
            g_FPBufferSize = BitConverter.ToInt32(paramValue, 0);
            g_FPBuffer = new byte[g_FPBufferSize];
            Array.Clear(g_FPBuffer, 0, g_FPBuffer.Length);
            // get vid&pid
            ret = paramValue.Length;
            Array.Clear(paramValue, 0, paramValue.Length);
            ZKFPCap.sensorGetParameterEx(g_Handle, 1015, paramValue, ref ret);
            int nVid = BitConverter.ToInt16(paramValue, 0);
            int nPid = BitConverter.ToInt16(paramValue, 2);
            // Manufacturer
            ret = paramValue.Length;
            Array.Clear(paramValue, 0, paramValue.Length);
            ZKFPCap.sensorGetParameterEx(g_Handle, 1101, paramValue, ref ret);
            string manufacturer = System.Text.Encoding.ASCII.GetString(paramValue);
            // Product
            ret = paramValue.Length;
            Array.Clear(paramValue, 0, paramValue.Length);
            ZKFPCap.sensorGetParameterEx(g_Handle, 1102, paramValue, ref ret);
            string product = System.Text.Encoding.ASCII.GetString(paramValue);
            // SerialNumber
            ret = paramValue.Length;
            Array.Clear(paramValue, 0, paramValue.Length);
            ZKFPCap.sensorGetParameterEx(g_Handle, 1103, paramValue, ref ret);
            string serialNumber = System.Text.Encoding.ASCII.GetString(paramValue);
            // Fingerprint Alg
            short[] iSize = new short[24];
            iSize[0] = (short)g_nWidth;
            iSize[1] = (short)g_nHeight;
            iSize[20] = (short)g_nWidth;
            iSize[21] = (short)g_nHeight; ;
            g_biokeyHandle = ZKFinger10.BIOKEY_INIT(0, iSize, null, null, 0);
            if (g_biokeyHandle == IntPtr.Zero)
            {
                return;
            }
            // Set allow 360 angle of Press Finger
            ZKFinger10.BIOKEY_SET_PARAMETER(g_biokeyHandle, 4, 180);
            // Set Matching threshold
            ZKFinger10.BIOKEY_MATCHINGPARAM(g_biokeyHandle, 0, ZKFinger10.THRESHOLD_MIDDLE);
            // Init RegTmps


            for (int i = 0; i < 3; i++)
            {
                g_RegTmps[i] = new byte[2048];
            }
            Thread captureThread = new Thread(new ThreadStart(DoCapture));
            captureThread.IsBackground = true;
            captureThread.Start();
            g_bIsTimeToDie = false;

        }

        public void SetTrangThai(bool status)
        {
            g_IsRegister = status;
            soLanDaDangKy = 0;
        }


        public async Task DefWndProc()
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                int ret = 0;
                // là chỉ số mờ hoặc rõ của vân tay
                int quality = 0;

                if (g_IsRegister)
                {
                    Array.Clear(g_RegTmp, 0, g_RegTmp.Length);
                    ret = ZKFinger10.BIOKEY_EXTRACT(g_biokeyHandle, g_FPBuffer, g_RegTmp, 0);
                    string responseVantayText = Convert.ToBase64String(g_RegTmp);

                    if (ret > 0)
                    {
                        Array.Copy(g_RegTmp, g_RegTmps[soLanDaDangKy++], ret);
                        quality = ZKFinger10.BIOKEY_GETLASTQUALITY();
                        Console.WriteLine(soLanDaDangKy);

                        if (soLanDaDangKy == TONG_SO_LAN_DANG_KY_MAC_DINH)
                        {
                            soLanDaDangKy = 0;
                            Array.Clear(g_RegTmp, 0, g_RegTmp.Length);
                            int size = getSizeVantay();

                            if (size > 0)
                            {
                                g_IsRegister = false;
                                await setSensorColor(ColorCode.BLUE);

                                ResponseVantay dataSendVantay = new ResponseVantay
                                {
                                    key = "dkthanhcong",
                                    solanconlai = 0,
                                    data = responseVantayText,
                                    size = size
                                };
                                await _socketService.SocketToClient("ReceiveRegister", dataSendVantay);
                            }
                            else
                            {
                                //dkthatbai
                                await setSensorColor(ColorCode.RED);
                                await Task.Delay(100);
                                await setSensorColor(ColorCode.RED);
                                ResponseVantay dataSendVantay = new ResponseVantay
                                {
                                    key = "dkthatbai",
                                };
                                await _socketService.SocketToClient("ReceiveRegister", dataSendVantay);
                            }

                        }
                        else
                        {
                            //dkvantay lan 1 2
                            byte[] paramValue = new byte[64];
                            await setSensorColor(ColorCode.BLUE);

                            ResponseVantay dataSendVantay = new ResponseVantay
                            {
                                key = "dkvantay",
                                solanconlai = TONG_SO_LAN_DANG_KY_MAC_DINH - soLanDaDangKy,
                            };
                            await _socketService.SocketToClient("ReceiveRegister", dataSendVantay);
                        }
                    }
                }
                else
                {
                    //xac nhan van tay
                    Array.Clear(g_VerTmp, 0, g_VerTmp.Length);
                    if ((ret = ZKFinger10.BIOKEY_EXTRACT(g_biokeyHandle, g_FPBuffer, g_VerTmp, 0)) > 0)
                    {
                        quality = ZKFinger10.BIOKEY_GETLASTQUALITY();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("errrrrrrrrrrrrror");
                Console.WriteLine(ex);
                throw new NotSupportedException(ex.ToString());
            }
        }

        public async Task setSensorSetParameterEx(int paramCode, int delay)
        {
            byte[] paramValue = new byte[64];
            paramValue[0] = 1;
            ZKFPCap.sensorSetParameterEx(g_Handle, paramCode, paramValue, 4);
            await Task.Delay(delay);
            paramValue[0] = 0;
            ZKFPCap.sensorSetParameterEx(g_Handle, paramCode, paramValue, 4);
        }

        public async Task setSensorColor(ColorCode type)
        {
            switch (type)
            {
                case ColorCode.BLUE:
                    await setSensorSetParameterEx((int)ColorCode.BLUE, 100);
                    break;
                case ColorCode.RED:
                    await setSensorSetParameterEx((int)ColorCode.RED, 10);
                    break;
                default:
                    throw new NotSupportedException($"{type} chưa được khai báo!");
            }
        }

        public int getSizeVantay()
        {
            int size = 0;

            unsafe
            {
                fixed (byte* Template1 = g_RegTmps[0])
                {
                    fixed (byte* Template2 = g_RegTmps[1])
                    {
                        fixed (byte* Template3 = g_RegTmps[2])
                        {
                            byte*[] pTemplate = new byte*[3] { Template1, Template2, Template3 };

                            size = ZKFinger10.BIOKEY_GENTEMPLATE(g_biokeyHandle, pTemplate, 3, g_RegTmp);
                        }
                    }
                }
            }

            return size;
        }

        public void removeDataSensor()
        {
            g_bIsTimeToDie = true;
            Thread.Sleep(200);
            ZKFPCap.sensorClose(g_Handle);
            // Disable log
            byte[] paramValue = new byte[4];
            paramValue[0] = 0;
            ZKFPCap.sensorSetParameterEx(g_Handle, 1100, paramValue, 4);
            //captureThread.
            ZKFPCap.sensorFree();

            ZKFinger10.BIOKEY_DB_CLEAR(g_biokeyHandle);
            ZKFinger10.BIOKEY_CLOSE(g_biokeyHandle);
        }


        public static string PrintJson(object printerOptions)
        {
            string response = JsonConvert.SerializeObject(printerOptions, Formatting.None,
                           new JsonSerializerSettings()
                           {
                               ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                           });
            Console.WriteLine(response);


            return response;
        }
    }
}
