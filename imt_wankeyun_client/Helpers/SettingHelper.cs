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
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\imt-wanke-settings.ini";
                System.IO.File.WriteAllText(path, wSettings);
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
                var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\imt-wanke-settings.ini";
                var txt = System.IO.File.ReadAllText(path);
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
