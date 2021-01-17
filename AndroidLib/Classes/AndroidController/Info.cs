namespace Mrivai.Pelitabangsa
{
    public class Info
    {
        private Device dev;
        internal Info(Device device)
        {
            dev = device;
        }

        private string getinfo(string property, bool mode)
        {
            var res = "";
            if(dev.State == DeviceState.ADB || mode == false)
            {
                res = dev.BuildProp.GetProp(property);
            } else if (dev.State == DeviceState.RECOVERY)
            {
                res = dev.BuildProp.GetProp(property);
            }
            return res;
        }
        
        public string Hardware { get { return getinfo("ro.hardware", true); } }
        public string Platform { get { return getinfo("ro.board.platform", true); } }
        public string Brand { get { return getinfo("ro.product.brand", true); } }
        public string Name { get { return getinfo("ro.product.device", true); } }
        public string Model { get { return dev.BuildProp.GetProp("ro.product.model"); } }

        public string AndroidVersion { get { return getinfo("ro.miui.ui.version.code", false); } }
        public string MIuiVersion { get { return getinfo("ro.miui.ui.version.name", false); } }
        public string ReleaseVersion { get { return getinfo("ro.build.version.release",false); } }
        public string SDK { get { return getinfo("ro.build.version.sdk",false); } }
    }
}
