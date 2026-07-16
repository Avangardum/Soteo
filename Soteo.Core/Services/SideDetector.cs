using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services;

public sealed class SideDetector(Side side) : ISideDetector
{
    public Side Side => side;
}
