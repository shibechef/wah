namespace Content.Shared.Xenoarchaeology.XenoArtifacts;

[RegisterComponent]
public sealed partial class RandomArtifactSpriteComponent : Component
{
    [DataField("eldritchSprites")]
    public int[] EldritchSprites = [1];

    [DataField("martianSprites")]
    public int[] MartianSprites = [1];

    [DataField("precursorSprites")]
    public int[] PrecursorSprites = [1];

    [DataField("siliconSprites")]
    public int[] SilicionSprites = [1];

    [DataField("wizardSprites")]
    public int[] WizardSprites = [1];

    [DataField("activationTime")]
    public double ActivationTime = 2.0;

    public TimeSpan? ActivationStart;
}
