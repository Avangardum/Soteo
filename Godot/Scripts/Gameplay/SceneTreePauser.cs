using Soteo.Core.Campaign;

namespace Soteo.Main.Gameplay;

public sealed class SceneTreePauser
{
    private readonly SceneTree _tree;
    private readonly ISynchronizedCampaignStatePuppetRepository _synchronizedCampaignStateRepo;

    public SceneTreePauser(ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo, SceneTree tree)
    {
        _tree = tree;
        _synchronizedCampaignStateRepo = synchronizedCampaignStateRepo;

        _tree.Paused = true;
        synchronizedCampaignStateRepo.Changed += Update;
    }

    private void Update()
    {
        _tree.Paused = _synchronizedCampaignStateRepo.State.IsPaused;
    }
}
