using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Game1
{
    public class Rms
    {
        public static int status;

        public static sbyte[] data;

        public static string filename;

        private const int INTERVAL = 5;

        private const int MAXTIME = 500;

        private const long AUTO_FLUSH_INTERVAL_MS = 1000L;

        private static bool hasPendingSave;

        private static long lastFlushAt;

        private static readonly object ioLock = new object();

        public static void saveRMS(string filename, sbyte[] data)
        {
            if (Thread.CurrentThread.Name == Main.mainThreadName)
            {
                __saveRMS(filename, data);
            }
            else
            {
                _saveRMS(filename, data);
            }
        }

        public static sbyte[] loadRMS(string filename)
        {
            if (Thread.CurrentThread.Name == Main.mainThreadName)
            {
                return __loadRMS(filename);
            }
            return _loadRMS(filename);
        }

        public static string loadRMSString(string fileName)
        {
            sbyte[] array = loadRMS(fileName);
            if (array == null)
            {
                return null;
            }
            DataInputStream dataInputStream = new DataInputStream(array);
            try
            {
                string result = dataInputStream.readUTF();
                dataInputStream.close();
                return result;
            }
            catch (Exception ex)
            {
                Cout.println(ex.StackTrace);
            }
            return null;
        }

        public static byte[] convertSbyteToByte(sbyte[] var)
        {
            byte[] array = new byte[var.Length];
            for (int i = 0; i < var.Length; i++)
            {
                if (var[i] > 0)
                {
                    array[i] = (byte)var[i];
                }
                else
                {
                    array[i] = (byte)(var[i] + 256);
                }
            }
            return array;
        }

        public static void saveRMSString(string filename, string data)
        {
            DataOutputStream dataOutputStream = new DataOutputStream();
            try
            {
                dataOutputStream.writeUTF(data);
                saveRMS(filename, dataOutputStream.toByteArray());
                dataOutputStream.close();
            }
            catch (Exception ex)
            {
                Cout.println(ex.StackTrace);
            }
        }

        private static void _saveRMS(string filename, sbyte[] data)
        {
            lock (ioLock)
            {
                if (!waitForIdle(filename, "save"))
                {
                    return;
                }
                Rms.filename = filename;
                Rms.data = data;
                status = 2;
                waitForCompletion(filename, "SAVE");
            }
        }

        private static sbyte[] _loadRMS(string filename)
        {
            lock (ioLock)
            {
                if (!waitForIdle(filename, "load"))
                {
                    return null;
                }
                Rms.filename = filename;
                data = null;
                status = 3;
                waitForCompletion(filename, "LOAD");
                return data;
            }
        }

        private static bool waitForIdle(string requestedFilename, string operation)
        {
            for (int i = 0; i < MAXTIME; i++)
            {
                if (status == 0)
                {
                    return true;
                }
                Thread.Sleep(INTERVAL);
            }
            Debug.LogError("TOO LONG TO " + operation.ToUpper() + " RMS " + requestedFilename + "; current file is " + filename);
            return false;
        }

        private static void waitForCompletion(string requestedFilename, string operation)
        {
            for (int i = 0; i < MAXTIME; i++)
            {
                Thread.Sleep(INTERVAL);
                if (status == 0)
                {
                    return;
                }
            }
            Debug.LogError("TOO LONG TO " + operation + " RMS " + requestedFilename);
        }

        public static void update()
        {
            if (status == 2)
            {
                status = 1;
                __saveRMS(filename, data);
                status = 0;
            }
            else if (status == 3)
            {
                status = 1;
                data = __loadRMS(filename);
                status = 0;
            }
            if (hasPendingSave && status == 0 && currentTimeMillis() - lastFlushAt >= AUTO_FLUSH_INTERVAL_MS)
            {
                flush();
            }
        }

        public static void flush()
        {
            if (!hasPendingSave)
            {
                return;
            }
            PlayerPrefs.Save();
            hasPendingSave = false;
            lastFlushAt = currentTimeMillis();
        }

        private static long currentTimeMillis()
        {
            return DateTime.UtcNow.Ticks / 10000L;
        }

        public static int loadRMSInt(string file)
        {
            sbyte[] array = loadRMS(file);
            if (array == null)
            {
                return -1;
            }
            return array[0];
        }

        public static int loadRMSVersion(string file)
        {
            sbyte[] array = loadRMS(file);
            if (array == null)
            {
                return -1;
            }
            return array[0];
        }

        public static void saveRMSInt(string file, int x)
        {
            try
            {
                saveRMS(file, new sbyte[1] { (sbyte)x });
            }
            catch (Exception)
            {
            }
        }

        public static void saveRMSIntVersion(int x)
        {
            try
            {
                saveRMS("001000000011001000110011001000000100010100110010001000000011000000110011001000000100010100110010001000000011000100110011001000000100010000110010", new sbyte[1] { (sbyte)x });
            }
            catch (Exception)
            {
            }
        }

        private static void __saveRMS(string filename, sbyte[] data)
        {
            string value = ByteArrayToString(ArrayCast.cast(data));
            PlayerPrefs.SetString(filename + TabType.Tab1, value);
            hasPendingSave = true;
        }

        private static sbyte[] __loadRMS(string filename)
        {
            string @string = PlayerPrefs.GetString(filename + TabType.Tab1);
            byte[] array;
            try
            {
                array = StringToByteArray(@string);
            }
            catch (Exception ex)
            {
                Debug.LogError("PARSE RMS STRING FAIL " + ex.StackTrace);
                return null;
            }
            if (array.Length == 0)
            {
                return null;
            }
            return ArrayCast.cast(array);
        }

        public static void clearAll()
        {
            // A two-tab client shares one PlayerPrefs database. DeleteAll() here
            // used to remove login information and the other tab's downloaded data.
            clearRMS();
            string[] cacheIndexes = new string[14]
            {
                "ResVersion", "NR_image", "NR_part", "NR_skill", "NR_dart", "NR_arrow", "NR_effect",
                "NRdataVersion", "NRmap", "NRmapVersion", "NRskill", "NRskillVersion", "NRitemVersion", "ImageSource"
            };
            for (int i = 0; i < cacheIndexes.Length; i++)
            {
                deleteRecord(cacheIndexes[i]);
            }
            flush();
        }

        public static void DeleteStorage(string path)
        {
            PlayerPrefs.DeleteKey(path + TabType.Tab1);
            PlayerPrefs.DeleteKey(path);
            hasPendingSave = true;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", string.Empty);
        }

        public static byte[] StringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] array = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                array[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return array;
        }

        public static void deleteRecord(string name)
        {
            try
            {
                PlayerPrefs.DeleteKey(name + TabType.Tab1);
                PlayerPrefs.DeleteKey(name);
                hasPendingSave = true;
            }
            catch (Exception ex)
            {
                Cout.println("loi xoa RMS --------------------------" + ex.ToString());
            }
        }

        public static void clearRMS()
        {
            deleteRecord("data");
            deleteRecord("dataVersion");
            deleteRecord("map");
            deleteRecord("mapVersion");
            deleteRecord("skill");
            deleteRecord("skillVersion");
            deleteRecord("killVersion");
            deleteRecord("item");
            deleteRecord("itemVersion");
        }

        public static void saveIP(string strID)
        {
            saveRMSString("NRIPlink", strID);
        }

        public static string loadIP()
        {
            string text = loadRMSString("NRIPlink");
            if (text == null)
            {
                return null;
            }
            return text;
        }
    }
}
