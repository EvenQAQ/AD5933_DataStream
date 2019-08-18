using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace AD5933_Lib
{
    static class AD_CYUSB_USB4
    {
        // Uint  Search_For_Boards (uint VID, uint PID, uint *Num_boards, char *PartPath[]);
        [DllImport("ADI_CYUSB_USB4.dll", EntryPoint = "Search_For_Boards")]
        public static extern unsafe uint Search_For_Boards(uint VID, uint PID, IntPtr Num_boards, IntPtr PartPath);

        [DllImport("ADI_CYUSB_USB4.dll", EntryPoint = "Connect")]
        public static extern unsafe int Connect(uint VID, uint PID, byte PartPath, IntPtr Handle);

        [DllImport("ADI_CYUSB_USB4.dll", EntryPoint = "Disconnect")]
        public static extern unsafe int Disconnect(ushort Handle);

        // Int Download_Firmware(Uint Handle, char  pcFilePath[]);
        [DllImport("ADI_CYUSB_USB4.dll", EntryPoint = "Download_Firmware")]
        public static extern unsafe int Download_Firmware(ushort Handle, IntPtr pcFilePath);

        // Int  Vendor_Request(UInt  Handle, UChar Request, UShort Value, UShort Index, UChar Direction, UShort DataLength, UChar *Buffer[]);

        [DllImport("ADI_CYUSB_USB4.dll", EntryPoint = "Vendor_Request")]
        public static extern unsafe int Vendor_Request(ushort Handle, byte Request, ushort Value, ushort Index, byte Direction, ushort DataLength, IntPtr Buffer);
    }

}
