namespace Soteo.Main.Shared.Extensions;

public static class ControlExtensions
{
    extension (Control self)
    {
        public GdVector2 TopCenterGlobalPosition => self.RectGlobalPosition + self.RectSize * new GdVector2(0.5f, 0);
        public GdVector2 TopRightGlobalPosition => self.RectGlobalPosition + self.RectSize * new GdVector2(1, 0);
        public GdVector2 CenterLeftGlobalPosition => self.RectGlobalPosition + self.RectSize * new GdVector2(0, 0.5f);
        public GdVector2 CenterGlobalPosition => self.RectGlobalPosition + self.RectSize / 2;
        public GdVector2 CenterRightGlobalPosition => self.RectGlobalPosition + self.RectSize * new GdVector2(1, 0.5f);
        public GdVector2 BottomLeftGlobalPosition => self.RectGlobalPosition + self.RectSize * new GdVector2(0, 1);
        public GdVector2 BottomCenterGlobalPosition => self.RectGlobalPosition + self.RectSize * new GdVector2(0.5f, 1);
        public GdVector2 BottomRightGlobalPosition => self.RectGlobalPosition + self.RectSize;
    }
}
