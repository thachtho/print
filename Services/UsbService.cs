﻿using System.Management;

namespace printer_2.Services
{
    public interface IUsbService
    {
        void CheckUsbAdd();

        bool getTrangThaiUsb();
    }
    public class UsbService : IUsbService
    {
        private bool isConnectUsb = false;
        private readonly ISocketService _socketService;
        private readonly IVantayService _vantayService;
        public UsbService(ISocketService socketService, IVantayService vantayService)
        {
            _socketService = socketService;
            _vantayService = vantayService;
        }
        static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            using var searcher = new ManagementObjectSearcher(
                @"Select * From Win32_USBHub");
            using ManagementObjectCollection collection = searcher.Get();

            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo(
                    (string)device.GetPropertyValue("DeviceID"),
                    (string)device.GetPropertyValue("PNPDeviceID"),
                    (string)device.GetPropertyValue("Description")
                    ));
            }
            return devices;
        }

        class USBDeviceInfo
        {
            public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
            {
                this.DeviceID = deviceID;
                this.PnpDeviceID = pnpDeviceID;
                this.Description = description;
            }
            public string DeviceID { get; private set; }
            public string PnpDeviceID { get; private set; }
            public string Description { get; private set; }
        }
        public void CheckUsbAdd()
        {
            try
            {

                bool isExitZK4500 = false;
                var usbDevices = GetUSBDevices();

                foreach (var usbDevice in usbDevices)
                {
                    if (usbDevice.Description == "ZK4500 Fingerprint Reader")
                    {
                        isExitZK4500 = true;
                        break;
                    }
                }

                //Tồn tại và isConnect = true(Dừng khi gọi liên tục)
                if (isExitZK4500 && isConnectUsb)
                {
                    return;
                }
                //ko Tồn tại và isConnect = false(Dừng khi gọi liên tục)
                if (!isExitZK4500 && !isConnectUsb)
                {
                    return;
                }

                //không tòn tại usb
                if (!isExitZK4500)
                {
                    _vantayService.removeDataSensor();
                    DisConnectUsbVantay();

                    return;
                }

                //tồn tại và isConnect = false
                if (isExitZK4500 && !isConnectUsb)
                {
                    try
                    {
                        _vantayService.USB_Added();
                        ConnectUsbVantay();
                    }
                    catch (Exception ex)
                    {
                        _vantayService.removeDataSensor();
                        DisConnectUsbVantay();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void ConnectUsbVantay()
        {
            _socketService?.SocketToClient("ReceiveDataConnect", true);
            isConnectUsb = true;
        }

        public void DisConnectUsbVantay()
        {
            _socketService?.SocketToClient("ReceiveDataConnect", false);
            isConnectUsb = false;
        }

        public bool getTrangThaiUsb()
        {
            return isConnectUsb;
        }
    }
}
