using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.ExtensionMethods.Attributes;

namespace NetEntityAutomation.Extensions.ExtensionMethods;
public static class CoverEntityExtensionMethods
{
    ///<summary>Closes a cover.</summary>
    public static void CloseCover(this ICoverEntityCore target)
    {
        target.CallService("close_cover");
    }

    ///<summary>Closes a cover.</summary>
    public static void CloseCover(this IEnumerable<ICoverEntityCore> target)
    {
        target.CallService("close_cover");
    }

    ///<summary>Tilts a cover to close.</summary>
    public static void CloseCoverTilt(this ICoverEntityCore target)
    {
        target.CallService("close_cover_tilt");
    }

    ///<summary>Tilts a cover to close.</summary>
    public static void CloseCoverTilt(this IEnumerable<ICoverEntityCore> target)
    {
        target.CallService("close_cover_tilt");
    }

    ///<summary>Opens a cover.</summary>
    public static void OpenCover(this ICoverEntityCore target)
    {
        target.CallService("open_cover");
    }

    ///<summary>Opens a cover.</summary>
    public static void OpenCover(this IEnumerable<ICoverEntityCore> target)
    {
        target.CallService("open_cover");
    }

    ///<summary>Tilts a cover open.</summary>
    public static void OpenCoverTilt(this ICoverEntityCore target)
    {
        target.CallService("open_cover_tilt");
    }

    ///<summary>Tilts a cover open.</summary>
    public static void OpenCoverTilt(this IEnumerable<ICoverEntityCore> target)
    {
        target.CallService("open_cover_tilt");
    }

    ///<summary>Moves a cover to a specific position.</summary>
    public static void SetCoverPosition(this ICoverEntityCore target, CoverSetCoverPositionParameters data)
    {
        target.CallService("set_cover_position", data);
    }

    ///<summary>Moves a cover to a specific position.</summary>
    public static void SetCoverPosition(this IEnumerable<ICoverEntityCore> target, CoverSetCoverPositionParameters data)
    {
        target.CallService("set_cover_position", data);
    }

    ///<summary>Moves a cover to a specific position.</summary>
    ///<param name="target">The ICoverEntityCore to call this service for</param>
    ///<param name="position">Target position.</param>
    public static void SetCoverPosition(this ICoverEntityCore target, long position)
    {
        target.CallService("set_cover_position", new CoverSetCoverPositionParameters { Position = position });
    }

    ///<summary>Moves a cover to a specific position.</summary>
    ///<param name="target">The IEnumerable&lt;ICoverEntityCore&gt; to call this service for</param>
    ///<param name="position">Target position.</param>
    public static void SetCoverPosition(this IEnumerable<ICoverEntityCore> target, long position)
    {
        target.CallService("set_cover_position", new CoverSetCoverPositionParameters { Position = position });
    }

    ///<summary>Moves a cover tilt to a specific position.</summary>
    public static void SetCoverTiltPosition(this ICoverEntityCore target, CoverSetCoverTiltPositionParameters data)
    {
        target.CallService("set_cover_tilt_position", data);
    }

    ///<summary>Moves a cover tilt to a specific position.</summary>
    public static void SetCoverTiltPosition(this IEnumerable<ICoverEntityCore> target, CoverSetCoverTiltPositionParameters data)
    {
        target.CallService("set_cover_tilt_position", data);
    }

    ///<summary>Moves a cover tilt to a specific position.</summary>
    ///<param name="target">The ICoverEntityCore to call this service for</param>
    ///<param name="tiltPosition">Target tilt positition.</param>
    public static void SetCoverTiltPosition(this ICoverEntityCore target, long tiltPosition)
    {
        target.CallService("set_cover_tilt_position", new CoverSetCoverTiltPositionParameters { TiltPosition = tiltPosition });
    }

    ///<summary>Moves a cover tilt to a specific position.</summary>
    ///<param name="target">The IEnumerable&lt;ICoverEntityCore&gt; to call this service for</param>
    ///<param name="tiltPosition">Target tilt positition.</param>
    public static void SetCoverTiltPosition(this IEnumerable<ICoverEntityCore> target, long tiltPosition)
    {
        target.CallService("set_cover_tilt_position", new CoverSetCoverTiltPositionParameters { TiltPosition = tiltPosition });
    }

    ///<summary>Stops the cover movement.</summary>
    public static void StopCover(this ICoverEntityCore target)
    {
        target.CallService("stop_cover");
    }

    ///<summary>Stops the cover movement.</summary>
    public static void StopCover(this IEnumerable<ICoverEntityCore> target)
    {
        target.CallService("stop_cover");
    }

    ///<summary>Stops a tilting cover movement.</summary>
    public static void StopCoverTilt(this ICoverEntityCore target)
    {
        target.CallService("stop_cover_tilt");
    }

    ///<summary>Stops a tilting cover movement.</summary>
    public static void StopCoverTilt(this IEnumerable<ICoverEntityCore> target)
    {
        target.CallService("stop_cover_tilt");
    }

    ///<summary>Toggles a cover open/closed.</summary>
    public static void Toggle(this ICoverEntityCore target)
    {
        target.CallService("toggle");
    }

    ///<summary>Toggles a cover open/closed.</summary>
    public static void Toggle(this IEnumerable<ICoverEntityCore> target)
    {
        target.CallService("toggle");
    }

    ///<summary>Toggles a cover tilt open/closed.</summary>
    public static void ToggleCoverTilt(this ICoverEntityCore target)
    {
        target.CallService("toggle_cover_tilt");
    }

    ///<summary>Toggles a cover tilt open/closed.</summary>
    public static void ToggleCoverTilt(this IEnumerable<ICoverEntityCore> target)
    {
        target.CallService("toggle_cover_tilt");
    }
}