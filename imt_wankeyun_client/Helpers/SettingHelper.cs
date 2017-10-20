using System;
using System.Diagnostics;
using imt_wankeyun_client.Entities;

namespace imt_wankeyun_client.Helpers
{
    public class SettingHelper
    {
        public static void WriteSettings(WankeSettings settings)
        {
            try
            {
                string json = JsonHelper.Serialize(settings);
                var wSettings = EncryptHelper.EncryptDES(json, "hahasetr");
                Debug.WriteLine("WriteSettings " + json);
                System.IO.File.WriteAllText("Settings.ini", wSettings);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        public static WankeSettings ReadSettings()
        {
            try
            {
                var txt = System.IO.File.ReadAllText("Settings.ini");
                var json = EncryptHelper.DecryptDES(txt, "hahasetr");
                Debug.WriteLine("ReadSettings " + json);
                var wSettings = JsonHelper.Deserialize<WankeSettings>(json);
                return wSettings;
            }
            catch
            {
                return null;
            }
        }
    }
}
