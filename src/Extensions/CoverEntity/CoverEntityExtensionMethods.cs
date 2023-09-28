using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Extensions.CoverEntity;

public static class CoverEntityExtensionMethods
{
    public static void CloseCover(this ICoverEntityCore cover)
    {
        cover.CallService("close_cover");
    }
    
    public static void CloseCover(this IEnumerable<ICoverEntityCore> covers)
    {
        covers.CallService("close_cover");
    }
    
    public static void OpenCover(this ICoverEntityCore cover)
    {
        cover.CallService("open_cover");
    }
    
    public static void OpenCover(this IEnumerable<ICoverEntityCore> covers)
    {
        covers.CallService("open_cover");
    }
}