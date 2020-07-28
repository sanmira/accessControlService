using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;
using System.Text;
using accessControlService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace accessControlService.Nfc
{
    public interface INfcReader
    {
        void setReaderMode(int mode);
    }

    public class NfcReaderService : INfcReader
    {
        private const int READER_MODE_IDLE = 0;
        private const int READER_MODE_WRITE_KEY = 1;
        private const int READER_MODE_READ_KEY = 2;

        [DllImport ("libnfc_nci_linux.so")]
        static extern int nfcManager_doInitialize();

        [DllImport ("libnfc_nci_linux.so")]
        static extern int nfcManager_registerTagCallback(IntPtr unmanagedAddr);

        [DllImport ("libnfc_nci_linux.so")]
        static extern int nfcManager_enableDiscovery(int mask, int reader_only_mode, int enable_host_routing, int restart);

        private readonly DatabaseContext _context;
        private NfcTagCallback_t g_TagCB;
        private int readerMode = READER_MODE_IDLE;

        public NfcReaderService(IServiceScopeFactory scopeFactory) 
        {      
            _context = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DatabaseContext>();

            Console.WriteLine("DbContext for nfc reader updated");
        }

        public void setReaderMode(int mode)
        {
            readerMode = mode;
        }

        private bool findUser(string key)
        {
            var user = _context.Users.Find(key);
            if (user == null)
                return false;
            else 
                return true;
        }

        private bool createUser(string key)
        {
            var user = _context.Users.Find(key);
            if (user != null)
                return false;

            User userModel = new User();
            userModel.key = key;
            userModel.name = "";
            _context.Users.Add(userModel);
            _context.SaveChanges();
            
            return true;
        }

        private bool createAccessRequest(string key, bool result)
        {
            AccessRequest accessRequest = new AccessRequest();
            accessRequest.id = _context.AccessRequests.Count() + 1;
            accessRequest.date = DateTime.Now;;
            var user = _context.Users.Find(key);
            if (user == null)
                accessRequest.user = "";
            else
                accessRequest.user = key;
            accessRequest.result = result;
            _context.AccessRequests.Add(accessRequest);
            _context.SaveChanges();

            return true;
        }

        public void start()
        {
            g_TagCB.onTagArrival = (IntPtr unmanagedAddr) =>
            {    
                NfcTagInfo g_tagInfos = new NfcTagInfo(); 

                g_tagInfos = (NfcTagInfo) Marshal.PtrToStructure(unmanagedAddr, typeof(NfcTagInfo));

                byte[] uidArray = new byte[g_tagInfos.uid_length];
                for (int i = 0; i < g_tagInfos.uid_length; i++)
                {
                    uidArray[i] = g_tagInfos.uid[i];
                }
                string uidString = BitConverter.ToString(uidArray).Replace("-","");
                Console.WriteLine("Tag arrived! ID: {0}", uidString);

                if (readerMode == READER_MODE_IDLE)
                {
                    if (!findUser(uidString))
                    {
                        Console.WriteLine("Cannot find user");
                        createAccessRequest(uidString, false);
                    }
                    else
                    {
                        Console.WriteLine("User found!");
                        createAccessRequest(uidString, true);
                    } 
                } else if (readerMode == READER_MODE_WRITE_KEY)
                {
                    if (!createUser(uidString))
                        Console.WriteLine("Cannot create user");

                    else
                        Console.WriteLine("User created!");
                }
            };

            g_TagCB.onTagDeparture = () =>
            {
                Console.WriteLine("Tag departed!");
            };

            IntPtr unmanagedAddr = Marshal.AllocHGlobal(Marshal.SizeOf(g_TagCB));
            Marshal.StructureToPtr(g_TagCB, unmanagedAddr, true);

            nfcManager_doInitialize();
            nfcManager_registerTagCallback(unmanagedAddr);
            nfcManager_enableDiscovery(-1, 0x01, 0, 0);

            Console.Write("NFC poll started!\n");
        }

        private bool checkIfKeyWritten()
        {
            return false;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void onTagArrivalCallback(IntPtr unmanagedAddr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void onTagDepartureCallback();

        [StructLayout(LayoutKind.Sequential)]
        private struct NfcTagCallback_t 
        {
            /**
            * \brief NFC Tag callback function when tag is detected.
            * param pTagInfo       tag infomation
            */
            public onTagArrivalCallback onTagArrival;

            /**
            * \brief NFC Tag callback function when tag is removed.
            */
            public onTagDepartureCallback onTagDeparture;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NfcTagInfo
        {
            /**
            *  \brief indicates the technology of tag
            */
            public uint technology;
            /**
            *  \brief the handle of tag
            */
            public uint handle;
            /**
            *  \brief the uid of tag
            */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] uid;
            /**
            *  \brief the uid length
            */
            public uint uid_length;
            /**
            *  \brief activated protocol
            */
            public char protocol;
        }
    }
}