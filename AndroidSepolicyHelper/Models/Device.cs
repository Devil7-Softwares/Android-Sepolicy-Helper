namespace AndroidSepolicyHelper.Models
{
    public class Device
    {
        public Device(string DeviceName, string Status, string Type)
        {
            this.DeviceName = DeviceName.Trim();
            this.Status = Status.Trim();
            this.Type = Type.Trim();
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.DeviceName, this.Type);
        }

        public string DeviceName { get; set; }

        public string Status { get; set; }

        public string Type { get; set; }
    }
}

