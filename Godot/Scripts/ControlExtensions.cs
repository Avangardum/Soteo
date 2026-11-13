namespace Soteo.Main.Shared.Extensions;

public static class ControlExtensions
{
    extension (Control self)
    {
        public GdVector2 TopCenterGlobalPosition => self.GlobalPosition + self.Size * new GdVector2(0.5f, 0);
        public GdVector2 TopRightGlobalPosition => self.GlobalPosition + self.Size * new GdVector2(1, 0);
        public GdVector2 CenterLeftGlobalPosition => self.GlobalPosition + self.Size * new GdVector2(0, 0.5f);
        public GdVector2 CenterGlobalPosition => self.GlobalPosition + self.Size / 2;
        public GdVector2 CenterRightGlobalPosition => self.GlobalPosition + self.Size * new GdVector2(1, 0.5f);
        public GdVector2 BottomLeftGlobalPosition => self.GlobalPosition + self.Size * new GdVector2(0, 1);
        public GdVector2 BottomCenterGlobalPosition => self.GlobalPosition + self.Size * new GdVector2(0.5f, 1);
        public GdVector2 BottomRightGlobalPosition => self.GlobalPosition + self.Size;
    }
}
