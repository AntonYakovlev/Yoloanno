namespace Yoloanno.Types
{
    public class RegionSize
    {
        public double Width;
        public double Height;

        public override string ToString()
        {
            return string.Format("{0} x {1}", Width, Height);
        }
    }
}
